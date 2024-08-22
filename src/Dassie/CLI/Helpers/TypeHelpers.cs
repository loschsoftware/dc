using System;
using System.Linq;
using System.Reflection;
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

    /// <summary>
    /// Checks wheter a specified type can be used in a place that requires a boolean value.
    /// </summary>
    /// <param name="t">The type to check</param>
    /// <returns>Returns <see langword="true"/> if at least one of the following is <see langword="true"/>:
    /// <list type="bullet">
    ///     <item><paramref name="t"/> is of type <see cref="bool"/>.</item>
    ///     <item><paramref name="t"/> defines an implicit conversion into <see cref="bool"/></item>
    ///     <item><paramref name="t"/> overloads the <c>op_True</c> and <c>op_False</c> operators.</item>
    /// </list>
    /// </returns>
    public static bool IsBoolean(Type t)
    {
        if (t == typeof(bool))
            return true;

        // Should we allow implicit conversions into other types that are IsBoolean() instead of just bool?
        if (t.GetMethods()
            .Where(m => m.Name == "op_Implicit")
            .Where(m => m.ReturnType == typeof(bool))
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == t)
            .Any())
            return true;

        return t.GetMethods()
            .Where(m => m.Name == "op_True")
            .Where(m => m.ReturnType == typeof(bool))
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == t)
            .Any();
    }

    /// <summary>
    /// Checks if <paramref name="t"/> is a <see cref="bool"/> or can be converted into one and then performs the conversion if necessary. If <paramref name="t"/> is incompatible with <see cref="bool"/>, an error is thrown.
    /// </summary>
    /// <param name="t">The type of the conversion.</param>
    /// <param name="line">The line in the code used for error messages.</param>
    /// <param name="col">The column in the code used for error messages.</param>
    /// <param name="length">The length of the symbol causing a code error..</param>
    /// <param name="throwError">Wheter to throw an error if the type could not be converted.</param>
    public static void EnsureBoolean(Type t, int line = 0, int col = 0, int length = 0, bool throwError = true)
    {
        if (!IsBoolean(t))
        {
            if (!throwError)
                return;

            EmitErrorMessage(
                line, col, length,
                DS0038_ConditionalExpressionClauseNotBoolean,
                $"The type '{t.FullName}' cannot be converted to type '{typeof(bool).FullName}'.");
        }

        if (t != typeof(bool))
            EmitBoolConversion(t);
    }

    /// <summary>
    /// Checks wheter type <paramref name="from"/> can be converted into type <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The type to convert from.</param>
    /// <param name="to">The type to convert into.</param>
    /// <returns>Wheter or not a conversion from <paramref name="from"/> to <paramref name="to"/> exists.</returns>
    public static bool CanBeConverted(Type from, Type to)
    {
        if (from == to)
            return true;

        if (from.GetMethods()
            .Where(m => m.Name == "op_Implicit")
            .Where(m => m.ReturnType == to)
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == from)
            .Any())
            return true;

        if (to.GetMethods()
            .Where(m => m.Name == "op_Explicit")
            .Where(m => m.ReturnType == to)
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == from)
            .Any())
            return true;

        return false;
    }
}