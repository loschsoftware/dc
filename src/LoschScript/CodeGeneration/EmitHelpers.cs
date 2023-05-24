using System;
using System.Reflection.Emit;

namespace LoschScript.CodeGeneration;

internal static class EmitHelpers
{
    public static void EmitStloc(ILGenerator generator, int index)
    {
        if (index <= 255)
            generator.Emit(OpCodes.Stloc_S, index);
        else
            generator.Emit(OpCodes.Stloc, index);
    }

    public static void EmitLdloc(ILGenerator generator, int index)
    {
        if (index <= 255)
            generator.Emit(OpCodes.Ldloc_S, index);
        else
            generator.Emit(OpCodes.Ldloc, index);
    }

    public static void EmitLdloca(ILGenerator generator, int index)
    {
        if (index <= 255)
            generator.Emit(OpCodes.Ldloca_S, index);
        else
            generator.Emit(OpCodes.Ldloca, index);
    }

    public static void EmitLdlocOrLdloca(ILGenerator generator, Type type, int index)
    {
        if (type.IsValueType)
            EmitLdloca(generator, index);
        else
            EmitLdloc(generator, index);
    }
}