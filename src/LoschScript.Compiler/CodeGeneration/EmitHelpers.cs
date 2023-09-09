using LoschScript.CLI;
using LoschScript.Meta;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

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

    public static void EmitLdarg(ILGenerator generator, int index)
    {
        if (index <= 255)
            generator.Emit(OpCodes.Ldarg_S, (byte)index);
        else
            generator.Emit(OpCodes.Ldarg, index);
    }

    public static void EmitLdloca(ILGenerator generator, int index)
    {
        if (index <= 255)
            generator.Emit(OpCodes.Ldloca_S, (byte)index);
        else
            generator.Emit(OpCodes.Ldloca, index);
    }

    public static void EmitLdarga(ILGenerator generator, int index)
    {
        if (index <= 255)
            generator.Emit(OpCodes.Ldarga_S, (byte)index);
        else
            generator.Emit(OpCodes.Ldarga, index);
    }

    public static void EmitLdarg0IfCurrentType(ILGenerator generator, Type t)
    {
        if (t == TypeContext.Current.Builder)
            generator.Emit(OpCodes.Ldarg_S, (byte)0);
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
    
    // Very rudimentary and almost useless - fix ASAP
    public static void EmitInlineIL(ILGenerator generator, string instruction, int line = 0, int column = 0, int length = 0)
    {
        if (!instruction.Contains(' '))
            instruction += " ";

        string rawOpcode = instruction.Split(' ')[0].TrimEnd('\r', '\n');

        string opcodeString = rawOpcode.Replace(".", "_");

        if (opcodeString.EndsWith("_"))
            opcodeString = opcodeString[..^1];

        string operandString = instruction.Split(' ')[1];
        operandString = operandString.TrimEnd('\r', '\n');

        FieldInfo opcodeField = typeof(OpCodes).GetField(opcodeString, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static);

        if (opcodeField == null)
        {
            EmitErrorMessage(
                line,
                column,
                length,
                LS0045_InlineILInvalidOpCode,
                $"'{rawOpcode}' is not a valid IL opcode."
                );
            return;
        }

        if (byte.TryParse(operandString, out byte argI1))
            generator.Emit((OpCode)opcodeField.GetValue(null), argI1);

        else if (int.TryParse(operandString, out int argI4))
            generator.Emit((OpCode)opcodeField.GetValue(null), argI4);

        else if (double.TryParse(operandString, out double argR8))
            generator.Emit((OpCode)opcodeField.GetValue(null), argR8);

        else if (operandString.StartsWith("\"") && operandString.EndsWith("\""))
            generator.Emit((OpCode)opcodeField.GetValue(null), operandString[1..^1]);

        else if (CurrentMethod.Locals.Any(l => l.Name == operandString))
        {
            int index = CurrentMethod.Locals.Where(l => l.Name == operandString).First().Index;

            if (index <= 255)
                generator.Emit((OpCode)opcodeField.GetValue(null), (byte)index);
            else
                generator.Emit((OpCode)opcodeField.GetValue(null), index);
        }

        else
            generator.Emit((OpCode)opcodeField.GetValue(null));
    }
}