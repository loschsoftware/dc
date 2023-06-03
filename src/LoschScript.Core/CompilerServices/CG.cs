namespace LoschScript.Core.CompilerServices;

/// <summary>
/// Provides special functions for interacting with the LoschScript code generator.
/// </summary>
public static class CG
{
    /// <summary>
    /// Emits an IL instruction.
    /// </summary>
    /// <remarks>This function has no effect on its own, as it is evaluated by the LoschScript compiler at compile time.</remarks>
    /// <param name="instruction">The IL instruction to emit, in human-readable format.</param>
    public static void il(string instruction) { }
}