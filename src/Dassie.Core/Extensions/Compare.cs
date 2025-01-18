using Dassie.Runtime;
using System;

#pragma warning disable IDE1006

namespace Dassie.Extensions;

/// <summary>
/// Provides implementations of the three-way comparison operator, <c>&lt;=&gt;</c>.
/// </summary>
[ContainsCustomOperators]
public static class Compare
{
    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(sbyte a, sbyte b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(byte a, byte b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(short a, short b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(ushort a, ushort b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(uint a, uint b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(int a, int b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(long a, long b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(ulong a, ulong b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(float a, float b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(double a, double b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(nint a, nint b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(nuint a, nuint b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(decimal a, decimal b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(Half a, Half b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(Int128 a, Int128 b) => a.CompareTo(b);

    /// <summary>
    /// Implements a three-way comparison.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>
    /// Returns <c>0</c> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, returns <c>-1</c> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, returns <c>1</c>.
    /// </returns>
    [Operator]
    public static int op_LessEqualGreater(UInt128 a, UInt128 b) => a.CompareTo(b);
}