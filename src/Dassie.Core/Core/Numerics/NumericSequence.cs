﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable IDE1006

namespace Dassie.Core.Numerics;

/// <summary>
/// Provides functionality for creating sequences of numbers.
/// </summary>
public static class NumericSequence
{
    /// <summary>
    /// Formats the specified integer array as a string.
    /// </summary>
    /// <param name="array">The array to format.</param>
    /// <returns>The formatted string.</returns>
    public static string formatArray(int[] array)
    {
        StringBuilder sb = new();
        sb.Append("@[ ");

        foreach (int i in array[..^1])
            sb.Append($"{i}, ");

        sb.Append($"{array.Last()}");

        sb.Append(" ]");

        return sb.ToString();
    }

    /// <summary>
    /// Formats the specified boxed integer array as a string.
    /// </summary>
    /// <param name="array">The array to format.</param>
    /// <returns>The formatted string.</returns>
    public static string formatArray(object[] array)
    {
        StringBuilder sb = new();
        sb.Append("@[ ");

        foreach (int i in array[..^1])
            sb.Append($"{i}, ");

        sb.Append($"{array.Last()}");

        sb.Append(" ]");

        return sb.ToString();
    }

    /// <summary>
    /// Formats the specified boxed integer list as a string.
    /// </summary>
    /// <param name="list">The list to format.</param>
    /// <returns>The formatted string.</returns>
    public static string formatArray(List<object> list)
    {
        StringBuilder sb = new();
        sb.Append("@[ ");

        foreach (int i in list.ToArray()[..^1])
            sb.Append($"{i}, ");

        sb.Append($"{list.Last()}");

        sb.Append(" ]");

        return sb.ToString();
    }

    /// <summary>
    /// Formats the specified integer array as a string representing a table row.
    /// </summary>
    /// <param name="array">The array to format.</param>
    /// <param name="width">The width of each integer.</param>
    /// <returns>The formatted string.</returns>
    public static string table(int[] array, int width)
    {
        StringBuilder sb = new();

        foreach (int i in array[..^1])
            sb.Append($"{i.ToString().PadLeft(width, '0')}, ");

        sb.Append($"{array.Last().ToString().PadLeft(width, '0')}");

        return sb.ToString();
    }

    /// <summary>
    /// Computes the sum of an array of integers.
    /// </summary>
    /// <param name="array">The array of which to compute the sum.</param>
    /// <returns>The sum of the array elements.</returns>
    public static int sum(int[] array) => array.Sum();

    /// <summary>
    /// Computes the product of all array elements.
    /// </summary>
    /// <param name="array">The array of which to compute the product.</param>
    /// <returns>The product of the array elements.</returns>
    public static int product(int[] array) => array.Aggregate(1, (a, b) => a * b);

    /// <summary>
    /// Creates a sequence of numbers ranging from 0 to the specified end.
    /// </summary>
    /// <param name="end">The last number of the sequence.</param>
    /// <returns>Returns an enumerable containing the generated numbers.</returns>
    public static int[] range(int end) => Enumerable.Range(0, end).ToArray();

    /// <summary>
    /// Creates a sequence of numbers within a specified range.
    /// </summary>
    /// <param name="start">The first number of the sequence.</param>nt
    /// <param name="count">The amount of numbers to include in the sequence.</param>
    /// <returns>Returns an enumerable containing the generated numbers.</returns>
    public static int[] range(int start, int count) => Enumerable.Range(start, count).ToArray();

    /// <summary>
    /// Creates a sequence of numbers between the specified ends using the specified step size.
    /// </summary>
    /// <param name="start">The first number of the sequence.</param>
    /// <param name="step">The step size.</param>
    /// <param name="count">The last number of the sequence.</param>
    /// <returns>Returns an enumerable containing the generated numbers.</returns>
    public static int[] range(int start, int step, int count) => Enumerable.Range(start, count).Where(i => (i - start) % step == 0).ToArray();

    /// <summary>
    /// Converts the specified list of numbers to an array.
    /// </summary>
    /// <param name="numbers">The list to convert.</param>
    /// <returns>The converted array.</returns>
    public static int[] seq(List<int> numbers) => numbers.ToArray();

    /// <summary>
    /// Converts the specified list of boxed numbers to an array.
    /// </summary>
    /// <param name="numbers">The list to convert.</param>
    /// <returns>The converted array.</returns>
    public static int[] seq(List<object> numbers) => numbers.Select(n => (int)n).ToArray();
}
