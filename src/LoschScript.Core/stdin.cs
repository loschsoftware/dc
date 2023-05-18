using System;

namespace LoschScript.Core;

/// <summary>
/// Provides functionality to read from the standard input.
/// </summary>
public static class stdin
{
    /// <summary>
    /// Reads a line from the standard input.
    /// </summary>
    /// <returns>Returns a line of text read from the standard input.</returns>
    public static string scan() => Console.ReadLine();

    /// <summary>
    /// Reads a line from the standard input and formats it using the specified arguments.
    /// </summary>
    /// <param name="args">The formatting arguments.</param>
    /// <returns>Returns the formatted string.</returns>
    public static string scanf(params object[] args) => string.Format(Console.ReadLine(), args);
}