#pragma warning disable IDE1006

namespace Dassie.Core;

/// <summary>
/// Provides functionality related to values.
/// </summary>
public static class Value
{
    /// <summary>
    /// Computes the identity of the specified value, which is the value itself.
    /// </summary>
    /// <param name="obj">The value, whose identity should be returned.</param>
    /// <returns>Returns the identity of the specified value.</returns>
    public static object id(object obj) => obj;

    /// <summary>
    /// Discards the specified value.
    /// </summary>
    /// <param name="obj">The value to discard.</param>
    public static void ignore(object obj) { }

    /// <summary>
    /// Discards the specified value.
    /// </summary>
    /// <param name="obj">The value to discard.</param>
    public static void discard(object obj) { }

    /// <summary>
    /// Negates the specified value.
    /// </summary>
    /// <param name="val">The value to negate.</param>
    /// <returns>The negated value.</returns>
    public static object negate(object val)
    {
        if (val.GetType().GetMethod("op_UnaryNegation") is not null)
            return val.GetType().GetMethod("op_UnaryNegation").Invoke(null, new[] { val });

        if (val is int or uint or short or ushort or long or ulong or byte or sbyte or double or float or decimal)
            return -(dynamic)val;

        return val;
    }
}