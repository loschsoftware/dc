using System.Collections.Generic;
using System.Linq;

namespace LoschScript.Core.Numerics;

/// <summary>
/// Provides functionality for creating sequences of numbers.
/// </summary>
public static class NumericSequence
{
    /// <summary>
    /// Creates a sequence of numbers ranging from 0 to the specified end.
    /// </summary>
    /// <param name="end">The last number of the sequence.</param>
    /// <returns>Returns an enumerable containing the generated numbers.</returns>
    public static IEnumerable<int> range(int end) => Enumerable.Range(0, end);

    /// <summary>
    /// Creates a sequence of numbers within a specified range.
    /// </summary>
    /// <param name="start">The first number of the sequence.</param>nt
    /// <param name="count">The amount of numbers to include in the sequence.</param>
    /// <returns>Returns an enumerable containing the generated numbers.</returns>
    public static IEnumerable<int> range(int start, int count) => Enumerable.Range(start, count);

    /// <summary>
    /// Creates a sequence of numbers between the specified ends using the specified step size.
    /// </summary>
    /// <param name="start">The first number of the sequence.</param>
    /// <param name="step">The step size.</param>
    /// <param name="count">The last number of the sequence.</param>
    /// <returns>Returns an enumerable containing the generated numbers.</returns>
    public static IEnumerable<int> range(int start, int step, int count) => Enumerable.Range(start, count).Where(i => (i - start) % step == 0);
}