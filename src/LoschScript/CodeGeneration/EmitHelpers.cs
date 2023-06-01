using LoschScript.CLI;
using System;
using System.Reflection.Emit;

namespace LoschScript.CodeGeneration;

internal static class EmitHelpers
{
    public static void EmitAdd(ILGenerator il, Type type, bool doOverflowCheck = false)
    {
        if (Helpers.IsUnsignedIntegerType(type))
            il.Emit(OpCodes.Add_Ovf_Un);
        else if (doOverflowCheck)
            il.Emit(OpCodes.Add_Ovf);
        else
            il.Emit(OpCodes.Add);
    }

    public static void EmitSub(ILGenerator il, Type type, bool doOverflowCheck = false)
    {
        if (Helpers.IsUnsignedIntegerType(type))
            il.Emit(OpCodes.Sub_Ovf_Un);
        else if (doOverflowCheck)
            il.Emit(OpCodes.Sub_Ovf);
        else
            il.Emit(OpCodes.Sub);
    }

    public static void EmitMul(ILGenerator il, Type type, bool doOverflowCheck = false)
    {
        if (Helpers.IsUnsignedIntegerType(type))
            il.Emit(OpCodes.Mul_Ovf_Un);
        else if (doOverflowCheck)
            il.Emit(OpCodes.Mul_Ovf);
        else
            il.Emit(OpCodes.Mul);
    }

    public static void EmitDiv(ILGenerator il, Type type)
    {
        if (Helpers.IsUnsignedIntegerType(type))
            il.Emit(OpCodes.Div_Un);
        else
            il.Emit(OpCodes.Div);
    }

    public static void EmitRem(ILGenerator il, Type type)
    {
        if (Helpers.IsUnsignedIntegerType(type))
            il.Emit(OpCodes.Rem_Un);
        else
            il.Emit(OpCodes.Rem);
    }

    public static void EmitShr(ILGenerator il, Type type)
    {
        if (Helpers.IsUnsignedIntegerType(type))
            il.Emit(OpCodes.Shr_Un);
        else
            il.Emit(OpCodes.Shr);
    }

    public static void EmitCgt(ILGenerator il, Type type)
    {
        if (Helpers.IsUnsignedIntegerType(type))
            il.Emit(OpCodes.Cgt_Un);
        else
            il.Emit(OpCodes.Cgt);
    }

    public static void EmitClt(ILGenerator il, Type type)
    {
        if (Helpers.IsUnsignedIntegerType(type))
            il.Emit(OpCodes.Clt_Un);
        else
            il.Emit(OpCodes.Clt);
    }

    public static void EmitLdcI4(ILGenerator il, int value)
    {
        if (value >= -128 && value <= 127)
            il.Emit(OpCodes.Ldc_I4_S, (byte)value);
        else
            il.Emit(OpCodes.Ldc_I4, value);
    }

    public static void EmitLdcI4(ILGenerator il, uint value)
    {
        if (value <= 127)
            il.Emit(OpCodes.Ldc_I4_S, (byte)value);
        else
            il.Emit(OpCodes.Ldc_I4, value);
    }

    public static void EmitStloc(ILGenerator generator, int index)
    {
        if (index <= 255)
            generator.Emit(OpCodes.Stloc_S, (byte)index);
        else
            generator.Emit(OpCodes.Stloc, index);
    }

    public static void EmitLdloc(ILGenerator generator, int index)
    {
        if (index <= 255)
            generator.Emit(OpCodes.Ldloc_S, (byte)index);
        else
            generator.Emit(OpCodes.Ldloc, index);
    }

    public static void EmitLdloca(ILGenerator generator, int index)
    {
        if (index <= 255)
            generator.Emit(OpCodes.Ldloca_S, (byte)index);
        else
            generator.Emit(OpCodes.Ldloca, index);
    }

    public static void EmitConst(ILGenerator il, object value)
    {
        if (value == null)
            return;

        if (value is sbyte or byte or short or ushort or int or char)
        {
            EmitLdcI4(il, (int)value);
            return;
        }

        if (value is uint i)
        {
            EmitLdcI4(il, i);
            return;
        }

        if (value is long l)
        {
            il.Emit(OpCodes.Ldc_I8, l);
            return;
        }

        if (value is ulong u)
        {
            il.Emit(OpCodes.Ldc_I8, u); // Nobody's gonna know
            return;
        }

        if (value is float f)
        {
            il.Emit(OpCodes.Ldc_R4, f);
            return;
        }

        if (value is double d)
        {
            il.Emit(OpCodes.Ldc_R8, d);
            return;
        }

        if (value is decimal dec)
        {
            il.Emit(OpCodes.Ldc_R8, (double)dec); // Who uses decimal anyway
            return;
        }

        if (value is bool b)
        {
            il.Emit(OpCodes.Ldc_I4_S, b ? (byte)1 : (byte)0);
            return;
        }

        if (value is string s)
        {
            il.Emit(OpCodes.Ldstr, s);
            return;
        }

        if (value.GetType().IsEnum)
        {
            EmitLdcI4(il, (int)value);
            return;
        }
    }
}