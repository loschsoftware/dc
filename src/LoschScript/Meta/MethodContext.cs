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

    public static MethodContext CurrentMethod { get; set; }

    public ILGenerator IL { get; set; }

    public List<string> FilesWhereDefined { get; } = new();

    public MethodBuilder Builder { get; set; }

    public int LocalIndex { get; set; } = -1;

    public List<(string Name, LocalBuilder Builder, bool IsConstant, int Index)> Locals { get; } = new();

    public int ThrowawayCounterVariableIndex { get; set; } = 0;
    
    public int LoopArrayReturnValueIndex { get; set; } = 0;

    public List<Type> ArgumentTypesForNextMethodCall { get; } = new();
}
