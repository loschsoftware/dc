using System.Diagnostics;

#pragma warning disable IDE1006

namespace Dassie.Core;

/// <summary>
/// Provides support for the global function 'assert'.
/// </summary>
public static class Assert
{
    /// <summary>
    /// Checks for a condition; if the condition is <see langword="false"/>, throws a runtime exception.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    public static void assert(bool condition) => Debug.Assert(condition);

    /// <summary>
    /// Checks for a condition; if the condition is <see langword="false"/>, throws a runtime exception with the specified message.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">The message to display on failure.</param>
    public static void assert(bool condition, string message) => Debug.Assert(condition, message);
}