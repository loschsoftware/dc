using Dassie.Runtime;

#pragma warning disable IDE1006

namespace Dassie.Extensions.Logic;

/// <summary>
/// Implements the 'implies' boolean operator (<c>--></c>).
/// </summary>
[ContainsCustomOperators]
public static class Implication
{
    /// <summary>
    /// Implements the 'implies' boolean operator (<c>--></c>).
    /// </summary>
    /// <param name="p">The proposition of the implication.</param>
    /// <param name="q">The consequence of the implication.</param>
    /// <returns>If <paramref name="p"/>, returns <paramref name="q"/>. Otherwise, returns <see langword="true"/>.</returns>
    [Operator]
    public static bool op_MinusMinusGreater(bool p, bool q) => !p || q;
}