using System;

#pragma warning disable IDE1006

namespace Dassie.Core;

/// <summary>
/// Implements global assertion functions.
/// </summary>
public static class Assert
{
    /// <summary>
    /// The exception thrown when an assertion fails.
    /// </summary>
    public class AssertionException : Exception
    {
        /// <inheritdoc/>
        public AssertionException(string message): base(message) { }
    }

    /// <summary>
    /// Checks for a condition; if the condition is <see langword="false"/>, throws a runtime exception.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <exception cref="AssertionException"/>
    public static void assert(bool condition)
        => assert(condition, "Assertion failed.");

    /// <summary>
    /// Checks wheter its two arguments are equal. If they are not, throws a runtime exception.
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <exception cref="AssertionException"/>
    public static void assertEqual(object obj1, object obj2)
        => assert(obj1 == obj2, $"Assertion failed.{Environment.NewLine}\tExpected: {obj1.Dump()}{Environment.NewLine}\tActual: {obj2.Dump()}");

    /// <summary>
    /// Checks for a condition; if the condition is <see langword="false"/>, throws a runtime exception with the specified message.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">The message to display on failure.</param>
    /// <exception cref="AssertionException"/>
    public static void assert(bool condition, string message)
    {
        if (!condition)
            throw new AssertionException(message);
    }
}