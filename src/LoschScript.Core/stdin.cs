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
    /// Reads a line from the standard input with a prompt.
    /// </summary>
    /// <param name="prompt">The prompt message to display.</param>
    /// <returns>Returns a line of text read from the standard input.</returns>
    public static string prompt(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }

    /// <summary>
    /// Reads a line from the standard input and formats it using the specified arguments.
    /// </summary>
    /// <param name="args">The formatting arguments.</param>
    /// <returns>Returns the formatted string.</returns>
    public static string scanf(params object[] args) => string.Format(Console.ReadLine(), args);

    /// <summary>
    /// Reads a line from the standard input with a prompt and formats it using the specified arguments.
    /// </summary>
    /// <param name="prompt">The prompt message to display.</param>
    /// <param name="args">The formatting arguments.</param>
    /// <returns>Returns the formatted string.</returns>
    public static string promptf(string prompt, params object[] args)
    {
        Console.Write(prompt);
        return string.Format(Console.ReadLine(), args);
    }
}