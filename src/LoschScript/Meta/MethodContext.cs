using LoschScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LoschScript.Meta;

internal class MethodContext
{
    public MethodContext(bool add = true)
    {
        CurrentMethod = this;

        if (add)
            TypeContext.Current.Methods.Add(this);
    }

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

            if (VisitorStep1.Types.Any(t => t.FullName == TypeContext.Current.FullName))
            {
                TypeContext type = VisitorStep1.Types.First(t => t.FullName == TypeContext.Current.FullName);

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

    public int LocalIndex { get; set; } = -1;

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

    public bool ShouldLoadAddressIfValueType { get; set; } = false;

    public Type StaticCallType { get; set; }

    public bool IgnoreTypesInSymbolResolve { get; set; } = false;

    public Dictionary<int, List<int>> ParameterBoxIndices { get; set; } = new();

    public bool BoxCallingType { get; set; }
}
