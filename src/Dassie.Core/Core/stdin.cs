using System;
using System.IO;

#pragma warning disable CS8981
#pragma warning disable IDE1006

namespace Dassie.Core;

/// <summary>
/// Provides functionality for reading from standard input.
/// </summary>
public static class stdin
{
    /// <summary>
    /// Reads a line from the standard input.
    /// </summary>
    /// <returns>Returns a line of text read from the standard input.</returns>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static string scan() => Console.ReadLine();

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an integer.
    /// </summary>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static int rdint() => int.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an integer.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static int rdint(string prompt)
    {
        Console.Write(prompt);
        return rdint();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an unsigned integer.
    /// </summary>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static uint rduint() => uint.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an unsigned integer.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static uint rduint(string prompt)
    {
        Console.Write(prompt);
        return rduint();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an integer.
    /// </summary>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static int rdint32() => int.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an integer.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static int rdint32(string prompt)
    {
        Console.Write(prompt);
        return rdint32();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an unsigned integer.
    /// </summary>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static uint rduint32() => uint.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an unsigned integer.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static uint rduint32(string prompt)
    {
        Console.Write(prompt);
        return rduint32();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a 64-bit integer.
    /// </summary>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static long rdint64() => long.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a 64-bit integer.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static long rdint64(string prompt)
    {
        Console.Write(prompt);
        return rdint64();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an unsigned 64-bit integer.
    /// </summary>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static ulong rduint64() => ulong.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an unsigned 64-bit integer.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static ulong rduint64(string prompt)
    {
        Console.Write(prompt);
        return rduint64();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a 16-bit integer.
    /// </summary>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static short rdint16() => short.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a 16-bit integer.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static short rdint16(string prompt)
    {
        Console.Write(prompt);
        return rdint16();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an unsigned 16-bit integer.
    /// </summary>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static ushort rduint16() => ushort.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an unsigned 16-bit integer.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static ushort rduint16(string prompt)
    {
        Console.Write(prompt);
        return rduint16();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an 8-bit integer.
    /// </summary>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static sbyte rdint8() => sbyte.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an 8-bit integer.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static sbyte rdint8(string prompt)
    {
        Console.Write(prompt);
        return rdint8();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an unsigned 8-bit integer.
    /// </summary>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static byte rduint8() => byte.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to an unsigned 8-bit integer.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted integer.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static byte rduint8(string prompt)
    {
        Console.Write(prompt);
        return rduint8();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a boolean.
    /// </summary>
    /// <returns>The converted boolean.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static bool rdbool() => bool.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a boolean.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted boolean.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static bool rdbool(string prompt)
    {
        Console.Write(prompt);
        return rdbool();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a 32-bit floating point number.
    /// </summary>
    /// <returns>The converted floating point number.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static float rdfloat32() => float.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a 32-bit floating point number.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted number.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static float rdfloat32(string prompt)
    {
        Console.Write(prompt);
        return rdfloat32();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a 64-bit floating point number.
    /// </summary>
    /// <returns>The converted floating point number.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static double rdfloat64() => double.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a 64-bit floating point number.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted number.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static double rdfloat64(string prompt)
    {
        Console.Write(prompt);
        return rdfloat64();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a decimal number.
    /// </summary>
    /// <returns>The converted decimal number.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static decimal rddec() => decimal.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a decimal number.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted decimal.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static decimal rddec(string prompt)
    {
        Console.Write(prompt);
        return rddec();
    }

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a UTF-16 character.
    /// </summary>
    /// <returns>The converted character.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static char rdchar() => char.Parse(Console.ReadLine());

    /// <summary>
    /// Reads a line from the standard input and attempts to convert it to a UTF-16 character.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The converted character.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static char rdchar(string prompt)
    {
        Console.Write(prompt);
        return rdchar();
    }

    /// <summary>
    /// Reads a line from the standard input.
    /// </summary>
    /// <returns>The read line.</returns>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static string rdstr() => Console.ReadLine();

    /// <summary>
    /// Reads a line from standard input.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The read line.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static string rdstr(string prompt)
    {
        Console.Write(prompt);
        return rdstr();
    }

    /// <summary>
    /// Reads a single key press from standard input.
    /// </summary>
    /// <returns>The read line.</returns>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static ConsoleKeyInfo rdkey() => Console.ReadKey();

    /// <summary>
    /// Reads a single key press from standard input.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading from standard input.</param>
    /// <returns>The read key.</returns>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="OverflowException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="OutOfMemoryException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static ConsoleKeyInfo rdkey(string prompt)
    {
        Console.Write(prompt);
        return rdkey();
    }

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