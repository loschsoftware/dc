﻿using Dassie.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static Dassie.CLI.Helpers.TypeHelpers;

namespace Dassie.CodeGeneration;

internal static class EmitHelpers
{
    public static void EmitAdd(Type type, bool doOverflowCheck = false)
    {
        if (IsUnsignedIntegerType(type))
            CurrentMethod.IL.Emit(OpCodes.Add_Ovf_Un);
        else if (doOverflowCheck)
            CurrentMethod.IL.Emit(OpCodes.Add_Ovf);
        else
            CurrentMethod.IL.Emit(OpCodes.Add);
    }

    public static void EmitSub(Type type, bool doOverflowCheck = false)
    {
        if (IsUnsignedIntegerType(type))
            CurrentMethod.IL.Emit(OpCodes.Sub_Ovf_Un);
        else if (doOverflowCheck)
            CurrentMethod.IL.Emit(OpCodes.Sub_Ovf);
        else
            CurrentMethod.IL.Emit(OpCodes.Sub);
    }

    public static void EmitMul(Type type, bool doOverflowCheck = false)
    {
        if (IsUnsignedIntegerType(type))
            CurrentMethod.IL.Emit(OpCodes.Mul_Ovf_Un);
        else if (doOverflowCheck)
            CurrentMethod.IL.Emit(OpCodes.Mul_Ovf);
        else
            CurrentMethod.IL.Emit(OpCodes.Mul);
    }

    public static void EmitDiv(Type type)
    {
        if (IsUnsignedIntegerType(type))
            CurrentMethod.IL.Emit(OpCodes.Div_Un);
        else
            CurrentMethod.IL.Emit(OpCodes.Div);
    }

    public static void EmitRem(Type type)
    {
        if (IsUnsignedIntegerType(type))
            CurrentMethod.IL.Emit(OpCodes.Rem_Un);
        else
            CurrentMethod.IL.Emit(OpCodes.Rem);
    }

    public static void EmitShr(Type type)
    {
        if (IsUnsignedIntegerType(type))
            CurrentMethod.IL.Emit(OpCodes.Shr_Un);
        else
            CurrentMethod.IL.Emit(OpCodes.Shr);
    }

    public static void EmitCgt(Type type)
    {
        if (IsUnsignedIntegerType(type))
            CurrentMethod.IL.Emit(OpCodes.Cgt_Un);
        else
            CurrentMethod.IL.Emit(OpCodes.Cgt);
    }

    public static void EmitClt(Type type)
    {
        if (IsUnsignedIntegerType(type))
            CurrentMethod.IL.Emit(OpCodes.Clt_Un);
        else
            CurrentMethod.IL.Emit(OpCodes.Clt);
    }

    public static void EmitLdcI4(int value)
    {
        if (value >= -128 && value <= 127)
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)value);
        else
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4, value);
    }

    public static void EmitLdcI4(uint value)
    {
        if (value <= 127)
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)value);
        else
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4, value);
    }

    public static void EmitStloc(int index)
    {
        if (index <= 255)
            CurrentMethod.IL.Emit(OpCodes.Stloc_S, (byte)index);
        else
            CurrentMethod.IL.Emit(OpCodes.Stloc, index);
    }

    public static void EmitStarg(int index)
    {
        if (index <= 255)
            CurrentMethod.IL.Emit(OpCodes.Starg_S, (byte)index);
        else
            CurrentMethod.IL.Emit(OpCodes.Starg, index);
    }

    public static void EmitLdloc(int index)
    {
        if (index <= 255)
            CurrentMethod.IL.Emit(OpCodes.Ldloc_S, (byte)index);
        else
            CurrentMethod.IL.Emit(OpCodes.Ldloc, index);
    }

    public static void EmitLdarg(int index)
    {
        if (index <= 255)
            CurrentMethod.IL.Emit(OpCodes.Ldarg_S, (byte)index);
        else
            CurrentMethod.IL.Emit(OpCodes.Ldarg, index);
    }

    public static void EmitLdloca(int index)
    {
        if (index <= 255)
            CurrentMethod.IL.Emit(OpCodes.Ldloca_S, (byte)index);
        else
            CurrentMethod.IL.Emit(OpCodes.Ldloca, index);
    }

    public static void EmitLdarga(int index)
    {
        if (index <= 255)
            CurrentMethod.IL.Emit(OpCodes.Ldarga_S, (byte)index);
        else
            CurrentMethod.IL.Emit(OpCodes.Ldarga, index);
    }

    public static void EmitLdarg0IfCurrentType(Type t)
    {
        if (t == TypeContext.Current.Builder)
            CurrentMethod.IL.Emit(OpCodes.Ldarg_S, (byte)0);
    }

    public static void EmitConst(object value)
    {
        if (value == null)
            return;

        if (value is sbyte or byte or short or ushort or int or char)
        {
            EmitLdcI4((int)value);
            return;
        }

        if (value is uint i)
        {
            EmitLdcI4(i);
            return;
        }

        if (value is long l)
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_I8, l);
            return;
        }

        if (value is ulong u)
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_I8, u); // Nobody's gonna know
            return;
        }

        if (value is float f)
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_R4, f);
            return;
        }

        if (value is double d)
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_R8, d);
            return;
        }

        if (value is decimal dec)
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_R8, (double)dec); // Who uses decimal anyway
            return;
        }

        if (value is bool b)
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, b ? (byte)1 : (byte)0);
            return;
        }

        if (value is string s)
        {
            CurrentMethod.IL.Emit(OpCodes.Ldstr, s);
            return;
        }

        if (value.GetType().IsEnum)
        {
            EmitLdcI4((int)value);
            return;
        }
    }

    // Very rudimentary and almost useless - fix ASAP
    public static void EmitInlineIL(string instruction, int line = 0, int column = 0, int length = 0)
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
                DS0045_InlineILInvalidOpCode,
                $"'{rawOpcode}' is not a valid IL opcode."
                );
            return;
        }

        if (byte.TryParse(operandString, out byte argI1))
            CurrentMethod.IL.Emit((OpCode)opcodeField.GetValue(null), argI1);

        else if (int.TryParse(operandString, out int argI4))
            CurrentMethod.IL.Emit((OpCode)opcodeField.GetValue(null), argI4);

        else if (double.TryParse(operandString, out double argR8))
            CurrentMethod.IL.Emit((OpCode)opcodeField.GetValue(null), argR8);

        else if (operandString.StartsWith("\"") && operandString.EndsWith("\""))
            CurrentMethod.IL.Emit((OpCode)opcodeField.GetValue(null), operandString[1..^1]);

        else if (CurrentMethod.Locals.Any(l => l.Name == operandString))
        {
            int index = CurrentMethod.Locals.Where(l => l.Name == operandString).First().Index;

            if (index <= 255)
                CurrentMethod.IL.Emit((OpCode)opcodeField.GetValue(null), (byte)index);
            else
                CurrentMethod.IL.Emit((OpCode)opcodeField.GetValue(null), index);
        }

        else
            CurrentMethod.IL.Emit((OpCode)opcodeField.GetValue(null));
    }

    public static void LoadField(FieldInfo f)
    {
        try
        {
            if (f.IsStatic)
                CurrentMethod.IL.Emit(OpCodes.Ldsfld, f);
            else
                CurrentMethod.IL.Emit(OpCodes.Ldfld, f);
        }
        catch (NullReferenceException) { }
    }

    public static void EmitStfld(FieldInfo f)
    {
        if (f.IsStatic)
            CurrentMethod.IL.Emit(OpCodes.Stsfld, f);
        else
            CurrentMethod.IL.Emit(OpCodes.Stfld, f);
    }

    public static void EmitCall(Type type, MethodInfo m)
    {
        if (m.IsStatic || type.IsValueType)
            CurrentMethod.IL.EmitCall(OpCodes.Call, m, null);
        else
            CurrentMethod.IL.EmitCall(OpCodes.Callvirt, m, null);

        if (m.ReturnType == typeof(void))
            CurrentMethod.SkipPop = true;
    }

    public static void EmitLdftn(MethodInfo m)
    {
        if (m.IsStatic || m.DeclaringType.IsValueType)
            CurrentMethod.IL.Emit(OpCodes.Ldftn, m);
        else
            CurrentMethod.IL.Emit(OpCodes.Ldvirtftn, m);
    }

    public static bool TryGetConstantValue(FieldInfo field, out object value)
    {
        try
        {
            value = field.GetRawConstantValue();
            return true;
        }
        catch (Exception)
        {
            value = null;
            return false;
        }
    }

    public static void SetLocalSymInfo(LocalBuilder lb, string name)
    {
        if (GlobalConfig.DisableDebugInfo)
            return;

        try
        {
            lb.SetLocalSymInfo(name);
        }
        catch (NotSupportedException) { }
    }

    public static void MarkSequencePoint(int line, int col, int length)
    {
        if (GlobalConfig.DisableDebugInfo)
            return;

        try
        {
            CurrentMethod.IL.MarkSequencePoint(CurrentFile.SymbolDocumentWriter, line, col, line, col + length);
        }
        catch (NotSupportedException) { }
    }

    public static void EmitTailcall()
    {
        if (CurrentMethod.EmitTailCall && CurrentMethod.AllowTailCallEmission)
            CurrentMethod.IL.Emit(OpCodes.Tailcall);

        CurrentMethod.EmitTailCall = false;
        CurrentMethod.AllowTailCallEmission = false;
    }

    /// <summary>
    /// Converts a type into a <see cref="bool"/> according to the rules specified in <see cref="Dassie.CLI.Helpers.TypeHelpers.IsBoolean(Type)"/>.
    /// </summary>
    /// <param name="t">The type to perform the conversion on.</param>
    public static void EmitBoolConversion(Type t)
    {
        IEnumerable<MethodInfo> implicitConversions = t.GetMethods()
            .Where(m => m.Name == "op_Implicit")
            .Where(m => m.ReturnType == typeof(bool))
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == t);

        if (implicitConversions.Any())
        {
            EmitCall(t, implicitConversions.First());
            return;
        }

        MethodInfo opTrue = t.GetMethods()
            .Where(m => m.Name == "op_True")
            .Where(m => m.ReturnType == typeof(bool))
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == t)
            .First();

        EmitCall(t, opTrue);
    }

    public static void EmitConversionOperator(Type from, Type to)
    {
        if (from == to)
            return;

        IEnumerable<MethodInfo> implicitConversions = from.GetMethods()
            .Where(m => m.Name == "op_Implicit")
            .Where(m => m.ReturnType == to)
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == from);

        if (implicitConversions.Any())
        {
            EmitCall(from, implicitConversions.First());
            return;
        }

        IEnumerable<MethodInfo> explicitConversions = to.GetMethods()
            .Where(m => m.Name == "op_Explicit")
            .Where(m => m.ReturnType == to)
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == from);

        // TODO: Calling explicit conversions implicitly might be dangerous..?
        if (explicitConversions.Any())
            EmitCall(from, explicitConversions.First());
    }

    public static bool TryGetAlternativeLocation(SymbolInfo sym, out (FieldInfo field, string LocalName) result)
    {
        if (VisitorStep1CurrentMethod != null && VisitorStep1CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == sym.Name()))
            result = VisitorStep1CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == sym.Name()).Value;

        else if (CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == sym.Name()))
            result = CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == sym.Name()).Value;

        result = default;
        return result != default;
    }
}