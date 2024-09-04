using System;
using System.Collections;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Dassie.CLI.Helpers;

/// <summary>
/// Provides helper methods regarding data types.
/// </summary>
internal static class TypeHelpers
{
    /// <summary>
    /// Removes the 'ByRef' modifier from a type if the specified type is a ByRef type.
    /// </summary>
    /// <param name="t">The type to remove the modifier from.</param>
    /// <returns>A type representing <paramref name="t"/> without a ByRef modifier.</returns>
    public static Type RemoveByRef(this Type t)
    {
        if (t.IsByRef /*|| t.IsByRefLike*/)
            t = t.GetElementType();

        return t;
    }

    /// <summary>
    /// Gets the IL instruction to load a value of the specified type indirectly.
    /// </summary>
    /// <param name="t">The type to load indirectly.</param>
    /// <returns>The <c>ldind.X</c> opcode corresponding to the specified type.</returns>
    /// <exception cref="InvalidOperationException"></exception>
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

    /// <summary>
    /// Gets the IL instruction to set a value of the specified type indirectly.
    /// </summary>
    /// <param name="t">The type to set indirectly.</param>
    /// <returns>The <c>stind.X</c> opcode corresponding to the specified type.</returns>
    /// <exception cref="InvalidOperationException"></exception>
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

    /// <summary>
    /// Emits an <c>ldind</c> instruction if the specified type is a ByRef type.
    /// </summary>
    /// <param name="t">The type to load indirectly.</param>
    public static void LoadIndirectlyIfPossible(this Type t)
    {
        if (!t.IsByRef /*&& !t.IsByRefLike*/)
            return;

        if (!IsNumericType(t.RemoveByRef()))
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

    public static bool IsNumericType(Type type)
    {
        Type[] numerics =
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(nint),
            typeof(nuint),
            typeof(char)
        };

        return numerics.Contains(type.RemoveByRef());
    }

    public static bool IsIntegerType(Type type)
    {
        Type[] numerics =
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(nint),
            typeof(nuint),
            typeof(char)
        };

        return numerics.Contains(type.RemoveByRef());
    }

    public static bool IsUnsignedIntegerType(Type type)
    {
        Type[] numerics =
        {
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(nuint)
        };

        return numerics.Contains(type.RemoveByRef());
    }

    public static bool IsFloatingPointType(Type type)
    {
        Type[] floats =
        {
            typeof(float),
            typeof(double)
        };

        return floats.Contains(type.RemoveByRef());
    }

    public static Type GetEnumeratedType(this Type type) =>
        (type?.GetElementType() ?? (typeof(IEnumerable).IsAssignableFrom(type)
            ? type.GenericTypeArguments.FirstOrDefault()
            : null))!;

    private static string GetTypeNameOrAlias(Type type)
    {
        string name = type.FullName ?? type.Name;

        if (type.IsGenericType)
            name = name.Split('`')[0];

        if (CurrentFile.Aliases.Any(a => a.Name == name))
            return CurrentFile.Aliases.First(a => a.Name == name).Alias;

        return name.Split('.').Last();
    }

    /// <summary>
    /// Formats the specified type name to be used in error messages.
    /// </summary>
    /// <param name="type">The type to format.</param>
    /// <returns>The formatted type name.</returns>
    public static string Format(Type type)
    {
        StringBuilder name = new(GetTypeNameOrAlias(type));

        if (type.IsGenericType)
        {
            name.Clear();
            name.Append(GetTypeNameOrAlias(type));
            name.Append('[');

            foreach (Type typeArg in type.GetGenericArguments()[..^1])
            {
                name.Append(Format(typeArg));
                name.Append(", ");
            }

            name.Append(Format(type.GetGenericArguments().Last()));
            name.Append(']');
        }

        return name.ToString();
    }
}