using LoschScript.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LoschScript.Meta;

internal class MethodContext
{
    public MethodContext()
    {
        CurrentMethod = this;
    }

    public static string GetThrowawayCounterVariableName(int index)
    {
        return $"<>g_Index{index}";
    }

    public static string GetLoopArrayReturnValueVariableName(int index)
    {
        return $"<>g_LoopArray{index}";
    }

    public static string GetTempVariableName(int index) => $"<>g_Temp{index}";

    public static MethodContext CurrentMethod { get; set; }

    public ILGenerator IL { get; set; }

    public List<string> FilesWhereDefined { get; } = new();

    public MethodBuilder Builder { get; set; }

    public int LocalIndex { get; set; } = -1;

    public List<(string Name, LocalBuilder Builder, bool IsConstant, int Index, UnionValue Union)> Locals { get; } = new();
    
    public UnionValue CurrentUnion { get; set; } = new(null, typeof(object));

    public int ThrowawayCounterVariableIndex { get; set; } = 0;
    
    public int LoopArrayReturnValueIndex { get; set; } = 0;

    public int TempValueIndex { get; set; } = 0;

    public List<Type> ArgumentTypesForNextMethodCall { get; } = new();
}
