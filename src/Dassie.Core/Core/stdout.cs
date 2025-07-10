using System;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable CS8981
#pragma warning disable IDE1006

namespace Dassie.Core;

/// <summary>
/// Provides functionality to write to the standard output.
/// </summary>
public static class stdout
{
    /// <summary>
    /// Formats the specified string using the provided formatting arguments.
    /// </summary>
    /// <param name="format">The string to format.</param>
    /// <param name="args">The formatting arguments.</param>
    /// <returns>The formatted string.</returns>
    public static string fmt(string format, params object[] args) => string.Format(format, args);

    /// <summary>
    /// Formats the specified string using the provided formatting argument.
    /// </summary>
    /// <param name="format">The string to format.</param>
    /// <param name="arg1">The formatting argument.</param>
    /// <returns>The formatted string.</returns>
    public static string fmt(string format, object arg1) => string.Format(format, arg1);

    /// <summary>
    /// Formats the specified string using the provided formatting arguments.
    /// </summary>
    /// <param name="format">The string to format.</param>
    /// <param name="arg1">The first formatting argument.</param>
    /// <param name="arg2">The second formatting argument.</param>
    /// <returns>The formatted string.</returns>
    public static string fmt(string format, object arg1, object arg2) => string.Format(format, arg1, arg2);

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
    public static void println(object msg)
    {
        if (msg is IEnumerable)
            msg = ObjectDump.Dump(msg).TrimEnd();

        Console.WriteLine(msg);
    }

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
    public static void print(object msg)
    {
        if (msg is IEnumerable)
            msg = ObjectDump.Dump(msg).TrimEnd();

        Console.Write(msg);
    }

    /// <summary>
    /// Formats the provided string using the specified arguments and prints it to the standard output.
    /// </summary>
    /// <param name="format">The string to format and print.</param>
    /// <param name="args">The format arguments.</param>
    public static void printf(string format, params object[] args) => print(string.Format(format, args));

    /// <summary>
    /// Formats the provided string using the specified arguments and prints it, followed by a newline character, to the standard output.
    /// </summary>
    /// <param name="format">The string to format and print.</param>
    /// <param name="args">The format arguments.</param>
    public static void printfn(string format, params object[] args) => printfn(string.Format(format, args));

    /// <summary>
    /// Dumps the properties of the specified object.
    /// </summary>
    /// <param name="obj">The object to dump.</param>
    public static void dump(object obj) => print(obj.Dump());

    /// <summary>
    /// Prints the specified value along with type information to the standard output.
    /// </summary>
    /// <param name="obj">The value to print.</param>
    public static void printv(object obj)
    {
        print(obj);
        print($" : {obj.GetType()}");
    }

    /// <summary>
    /// Prints the specified value along with type information, followed by a newline character, to the standard output.
    /// </summary>
    /// <param name="obj">The value to print.</param>
    public static void printvn(object obj)
    {
        printv(obj);
        println();
    }
}