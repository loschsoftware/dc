using Dassie.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dassie.Meta;

internal class MethodContext
{
    public MethodContext(bool add = true)
    {
        CurrentMethod = this;

        if (add)
            TypeContext.Current.Methods.Add(this);
    }

    public int ClosureIndex = 0;
    public static string GetClosureTypeName(int index) => $"<>g_Anon{index}";

    public static string GetThrowawayCounterVariableName(int index)
    {
        return $"<>g_Index{index}";
    }
    public static string GetLoopArrayReturnValueVariableName(int index)
    {
        return $"<>g_LoopArray{index}";
    }

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

                if (type.Methods.Any(m => m.Builder.Name == CurrentMethod.Builder.Name && m.Builder.ReturnType.FullName == CurrentMethod.Builder.ReturnType.FullName))
                {
                    MethodContext m = type.Methods.First(m => m.Builder.Name == CurrentMethod.Builder.Name && m.Builder.ReturnType.FullName == CurrentMethod.Builder.ReturnType.FullName);
                    return m;
                }
            }

            return null;
        }
    }

    public static string GetTempVariableName(int index) => $"<>g_Temp{index}";

    public static MethodContext CurrentMethod { get; set; }

    public ILGenerator IL { get; set; }

    public List<string> FilesWhereDefined { get; } = new();

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

    public List<LocalInfo> Locals { get; } = new();

    public List<ParamInfo> Parameters { get; } = new();

    public List<SymbolInfo> AvailableSymbols { get; } = new();

    public UnionValue CurrentUnion { get; set; } = new(null, typeof(object));

    public int ThrowawayCounterVariableIndex { get; set; } = 0;

    public int LoopArrayReturnValueIndex { get; set; } = 0;

    public int TempValueIndex { get; set; } = 0;

    public bool SkipPop { get; set; }

    public List<Type> ArgumentTypesForNextMethodCall { get; } = new();

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
    
    public Dictionary<int, List<int>> ParameterBoxIndices { get; set; } = new();

    public List<int> ByRefArguments { get; set; } = [];

    public int CurrentArg { get; set; }

    public bool BoxCallingType { get; set; }

    public List<TypeContext> ClosureTypes { get; } = new();
}