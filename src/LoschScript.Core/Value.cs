namespace LoschScript.Core;

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
}