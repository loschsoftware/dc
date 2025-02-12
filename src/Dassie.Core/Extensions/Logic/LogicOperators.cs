using Dassie.Runtime;

#pragma warning disable IDE1006

namespace Dassie.Extensions.Logic;

/// <summary>
/// Implements advanced boolean logic operators.
/// </summary>
[ContainsCustomOperators]
public static class LogicOperators
{
    /// <summary>
    /// Implements the 'implies' boolean operator (<c>==></c>).
    /// </summary>
    /// <param name="p">The proposition of the implication.</param>
    /// <param name="q">The consequence of the implication.</param>
    /// <returns>If <paramref name="p"/>, returns <paramref name="q"/>. Otherwise, returns <see langword="true"/>.</returns>
    [Operator]
    public static bool op_EqualEqualGreater(bool p, bool q) => !p || q;

    /// <summary>
    /// Implements the 'nand' boolean operator (<c>!&amp;</c>).
    /// </summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    /// <returns>Returns <see langword="false"/> if <paramref name="left"/> and <paramref name="right"/> are both <see langword="true"/>. Otherwise, returns <see langword="true"/>.</returns>
    [Operator]
    public static bool op_BangAmp(bool left, bool right) => !(left & right);

    /// <summary>
    /// Implements the 'nor' boolean operator (<c>!|</c>).
    /// </summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    /// <returns>Returns <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are both <see langword="false"/>. Otherwise, returns <see langword="false"/>.</returns>
    [Operator]
    public static bool op_BangBar(bool left, bool right) => !(left | right);
}