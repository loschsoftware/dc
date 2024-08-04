using System;
using System.Reflection.Emit;

namespace Dassie.CLI.Helpers;

internal static class TypeHelpers
{
    public static Type RemoveByRef(this Type t)
    {
        if (t.IsByRef /*|| t.IsByRefLike*/)
            t = t.GetElementType();

        return t;
    }

    public static OpCode GetLoadIndirectOpCode(this Type t)
    {
        if (t == typeof(byte) || t == typeof(sbyte))
            return OpCodes.Ldind_I1;

        if (t == typeof(short) || t == typeof(ushort))
            return OpCodes.Ldind_I2;

        if (t == typeof(int) || t == typeof(uint))
            return OpCodes.Ldind_I4;

        if (t == typeof(long) || t == typeof(ulong))
            return OpCodes.Ldind_I8;

        if (t == typeof(float))
            return OpCodes.Ldind_R4;

        if (t == typeof(double))
            return OpCodes.Ldind_R8;

        if (t == typeof(nint) || t == typeof(nuint))
            return OpCodes.Ldind_I;

        if (t.IsClass)
            return OpCodes.Ldind_Ref;

        throw new InvalidOperationException();
    }

    public static OpCode GetSetIndirectOpCode(this Type t)
    {
        if (t == typeof(byte) || t == typeof(sbyte))
            return OpCodes.Stind_I1;

        if (t == typeof(short) || t == typeof(ushort))
            return OpCodes.Stind_I2;

        if (t == typeof(int) || t == typeof(uint))
            return OpCodes.Stind_I4;

        if (t == typeof(long) || t == typeof(ulong))
            return OpCodes.Stind_I8;

        if (t == typeof(float))
            return OpCodes.Stind_R4;

        if (t == typeof(double))
            return OpCodes.Stind_R8;

        if (t == typeof(nint) || t == typeof(nuint))
            return OpCodes.Stind_I;

        if (t.IsClass)
            return OpCodes.Stind_Ref;

        throw new InvalidOperationException();
    }

    public static void LoadIndirectlyIfPossible(this Type t)
    {
        if (!t.IsByRef /*&& !t.IsByRefLike*/)
            return;

        if (!CliHelpers.IsNumericType(t.RemoveByRef()))
            return;

        CurrentMethod.IL.Emit(GetLoadIndirectOpCode(t.RemoveByRef()));
    }
}