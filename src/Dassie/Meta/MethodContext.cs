using Dassie.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Dassie.Meta;

internal class MethodContext
{
    public MethodContext(bool add = true)
    {
        CurrentMethod = this;

        if (add)
            TypeContext.Current.Methods.Add(this);
    }

    public int AnonymousFunctionIndex { get; set; } = -1;
    public string GetAnonymousFunctionName(int index) => $"{Builder.DeclaringType.FullName}${Builder.Name}_f{index}$";

    public static string GetThrowawayCounterVariableName(int index)
    {
        return $"<>g_Index{index}";
    }
    public static string GetLoopArrayReturnValueVariableName(int index)
    {
        return $"<>g_LoopArray{index}";
    }

    public static MethodContext SpecialStep1CurrentMethod { get; set; }

    public static MethodContext VisitorStep1CurrentMethod
    {
        get
        {
            if (VisitorStep1 == null)
                return null;

            if (CurrentMethod.Builder == null && CurrentMethod.ConstructorBuilder == null)
                return null;

            if (VisitorStep1.Types.Any(t => t.FullName == TypeContext.Current.FullName))
            {
                TypeContext type = VisitorStep1.Types.First(t => t.FullName == TypeContext.Current.FullName);

                // TODO: Find correct constructor (parameters)
                if (CurrentMethod.ConstructorBuilder != null && type.Methods.Any(m => m.ConstructorBuilder != null) /*&& type.Methods.Select(m => m.ConstructorBuilder).Any(c => c != null && c.GetParameters() == CurrentMethod.ConstructorBuilder.GetParameters())*/)
                {
                    MethodContext constructor = type.Methods.First(m => m.ConstructorBuilder != null /*&& m.ConstructorBuilder.GetParameters() == CurrentMethod.ConstructorBuilder.GetParameters()*/);
                    return constructor;
                }

                try
                {
                    if (type.Methods.Any(m => m.Builder.Name == CurrentMethod.Builder.Name && m.Builder.ReturnType.FullName == CurrentMethod.Builder.ReturnType.FullName))
                    {
                        MethodContext m = type.Methods.First(m => m.Builder.Name == CurrentMethod.Builder.Name && m.Builder.ReturnType.FullName == CurrentMethod.Builder.ReturnType.FullName);
                        return m;
                    }
                }
                catch { }

                try
                {
                    if (type.Methods.Any(m => m.Builder.Name == CurrentMethod.Builder.Name && m.Builder.GetParameters().Select(p => p.ParameterType).SequenceEqual(CurrentMethod.Builder.GetParameters().Select(p => p.ParameterType))))
                    {
                        MethodContext m = type.Methods.First(m => m.Builder.Name == CurrentMethod.Builder.Name && m.Builder.GetParameters().Select(p => p.ParameterType).SequenceEqual(CurrentMethod.Builder.GetParameters().Select(p => p.ParameterType)));
                        return m;
                    }
                }
                catch { }
            }

            if (SpecialStep1CurrentMethod != null)
                return SpecialStep1CurrentMethod;

            return null;
        }
    }

    public static string GetTempVariableName(int index) => $"<>g_Temp{index}";

    public static MethodContext CurrentMethod { get; set; }

    public string UniqueMethodName
    {
        get
        {
            MethodInfo[] methods = TypeContext.Current.Methods.Select(m => m.Builder).Where(b => b.Name == Builder.Name).ToArray();

            StringBuilder name = new();
            name.Append($"{Builder.Name}'");

            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i] == Builder)
                {
                    name.Append(i);
                    break;
                }
            }

            return name.ToString();
        }
    }

    public ILGenerator IL { get; set; }

    public List<string> FilesWhereDefined { get; private set; } = [];

    public MethodBuilder Builder { get; set; }

    public ConstructorBuilder ConstructorBuilder { get; set; }

    private int _localIndex = -1;
    public int LocalIndex
    {
        get => _localIndex;
        set
        {
            if (value > 65534)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0074_TooManyLocals,
                    "Only 65534 locals can be declared per function.");
            }

            _localIndex = value;
        }
    }

    public int ParameterIndex { get; set; } = 0;

    public List<LocalInfo> Locals { get; private set; } = [];

    public List<ParamInfo> Parameters { get; private set; } = [];

    public List<SymbolInfo> AvailableSymbols { get; private set; } = [];

    public UnionValue CurrentUnion { get; set; } = new(null, typeof(object));

    public int ThrowawayCounterVariableIndex { get; set; } = 0;

    public int LoopArrayReturnValueIndex { get; set; } = 0;

    public int TempValueIndex { get; set; } = 0;

    public bool SkipPop { get; set; }

    public List<Type> ArgumentTypesForNextMethodCall { get; private set; } = [];

    private Type[] _typeArgs = [];
    public Type[] TypeArgumentsForNextMethodCall
    {
        get
        {
            Type[] result = _typeArgs;
            TypeArgumentsForNextMethodCall = [];
            return result;
        }

        set => _typeArgs = value;
    }

    private bool _shouldLoadAddressIfValueType = false;
    public bool ShouldLoadAddressIfValueType
    {
        get
        {
            bool ret = _shouldLoadAddressIfValueType;
            _shouldLoadAddressIfValueType = false;
            return ret;
        }
        set => _shouldLoadAddressIfValueType = value;
    }

    public Type StaticCallType { get; set; }

    public bool IgnoreTypesInSymbolResolve { get; set; } = false;

    public Dictionary<int, List<int>> ParameterBoxIndices { get; set; } = [];

    public List<int> ByRefArguments { get; set; } = [];

    public int CurrentArg { get; set; }

    public bool BoxCallingType { get; set; }

    public List<TypeContext> ClosureTypes { get; private set; } = [];

    public bool AllowTailCallEmission { get; set; }

    public bool EmitTailCall { get; set; }

    public List<int> Scopes { get; set; } = [0];

    public int CurrentScope { get; set; }

    public List<TypeParameterContext> TypeParameters { get; set; } = [];

    public List<SymbolInfo> CapturedSymbols { get; set; } = [];

    public List<MethodContext> LocalFunctions { get; set; } = [];

    public bool IsLocalFunction { get; set; }

    public MethodContext Parent { get; set; }

    public TypeContext LocalFunctionContainerType { get; set; }

    public bool CaptureSymbols { get; set; }

    public Dictionary<SymbolInfo, (FieldInfo Field, string LocalName)> AdditionalStorageLocations { get; set; } = [];

    public TypeBuilder ClosureContainerType { get; set; }
    public List<FieldBuilder> ClosureCapturedFields { get; set; }

    public bool IsClosureInvocationFunction { get; set; }
    
    public int LoopArrayTypeProbeIndex { get; set; } = 0;
    public Dictionary<int, Type> LoopArrayTypeProbes { get; } = [];

    public bool LoadReference { get; set; }
    public bool LoadIndirectIfByRef { get; set; } = true;
    
    public bool LoadAddressForDirectObjectInit { get; set; }
    public int DirectObjectInitIndex { get; set; }

    public bool LocalSetExternally { get; set; }

    public bool IsCustomOperator { get; set; }
}