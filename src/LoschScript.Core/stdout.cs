using System;

namespace LoschScript.Core;

/// <summary>
/// Provides functionality to write to the standard output.
/// </summary>
public static class stdout
{
    /// <summary>
    /// Prints a newline character to the standard output.
    /// </summary>
    public static void println() => Console.WriteLine();

    /// <summary>
    /// Prints the specified string followed by a newline character to the standard output.
    /// </summary>
    /// <param name="msg">The string to print.</param>
    public static void println(string msg) => Console.WriteLine(msg);
    /// <summary>
    /// Prints the specified integer followed by a newline character to the standard output.
    /// </summary>
    /// <param name="msg">The integer to print.</param>
    public static void println(int msg) => Console.WriteLine(msg);
    /// <summary>
    /// Prints the specified double followed by a newline character to the standard output.
    /// </summary>
    /// <param name="msg">The double to print.</param>
    public static void println(double msg) => Console.WriteLine(msg);
    /// <summary>
    /// Prints the specified float followed by a newline character to the standard output.
    /// </summary>
    /// <param name="msg">The float to print.</param>
    public static void println(float msg) => Console.WriteLine(msg);
    /// <summary>
    /// Prints the specified character followed by a newline character to the standard output.
    /// </summary>
    /// <param name="msg">The character to print.</param>
    public static void println(char msg) => Console.WriteLine(msg);
    /// <summary>
    /// Prints the specified boolean followed by a newline character to the standard output.
    /// </summary>
    /// <param name="msg">The boolean to print.</param>
    public static void println(bool msg) => Console.WriteLine(msg);
    /// <summary>
    /// Prints the specified object followed by a newline character to the standard output.
    /// </summary>
    /// <param name="msg">The object to print.</param>
    public static void println(object msg) => Console.WriteLine(msg);

    /// <summary>
    /// Prints the specified string to the standard output.
    /// </summary>
    /// <param name="msg">The string to print.</param>
    public static void print(string msg) => Console.Write(msg);
    /// <summary>
    /// Prints the specified integer to the standard output.
    /// </summary>
    /// <param name="msg">The integer to print.</param>
    public static void print(int msg) => Console.Write(msg);
    /// <summary>
    /// Prints the specified double to the standard output.
    /// </summary>
    /// <param name="msg">The double to print.</param>
    public static void print(double msg) => Console.Write(msg);
    /// <summary>
    /// Prints the specified float to the standard output.
    /// </summary>
    /// <param name="msg">The float to print.</param>
    public static void print(float msg) => Console.Write(msg);
    /// <summary>
    /// Prints the specified character to the standard output.
    /// </summary>
    /// <param name="msg">The character to print.</param>
    public static void print(char msg) => Console.Write(msg);
    /// <summary>
    /// Prints the specified boolean to the standard output.
    /// </summary>
    /// <param name="msg">The boolean to print.</param>
    public static void print(bool msg) => Console.Write(msg);
    /// <summary>
    /// Prints the specified object to the standard output.
    /// </summary>
    /// <param name="msg">The object to print.</param>
    public static void print(object msg) => Console.Write(msg);

    /// <summary>
    /// Formats the provided string using the specified arguments and prints it to the standard output.
    /// </summary>
    /// <param name="format">The string to format and print.</param>
    /// <param name="args">The format arguments.</param>
    public static void printf(string format, params object[] args) => Console.Write(string.Format(format, args));

    /// <summary>
    /// Formats the provided string using the specified arguments and prints it, followed by a newline character, to the standard output.
    /// </summary>
    /// <param name="format">The string to format and print.</param>
    /// <param name="args">The format arguments.</param>
    public static void printfn(string format, params object[] args) => Console.WriteLine(string.Format(format, args));
}