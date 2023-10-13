namespace LoschScript.CompilerServices;

/// <summary>
/// Provides special functions for interacting with the LoschScript code generator.
/// </summary>
public static class CodeGeneration
{
    /// <summary>
    /// Emits an IL instruction.
    /// </summary>
    /// <remarks>This function has no effect on its own, as it is evaluated by the LoschScript compiler at compile time.</remarks>
    /// <param name="instruction">The IL instruction to emit, in human-readable format.</param>
    public static void il(string instruction) { }

    /// <summary>
    /// Imports a namespace or module into the current compilation unit.
    /// </summary>
    /// <remarks>This function has no effect on its own, as it is evaluated by the LoschScript compiler at compile time.</remarks>
    /// <param name="namespace">The namespace or module to import.</param>
    public static void localImport(string @namespace) { }

    /// <summary>
    /// Imports a namepace or module into the current project.
    /// </summary>
    /// <remarks>This function has no effect on its own, as it is evaluated by the LoschScript compiler at compile time.</remarks>
    /// <param name="namespace">The namespace or module to import.</param>
    public static void globalImport(string @namespace) { }

    /// <summary>
    /// Assigns an alias to a namespace or module name.
    /// </summary>
    /// <remarks>This function has no effect on its own, as it is evaluated by the LoschScript compiler at compile time.</remarks>
    /// <param name="alias">The alias to set.</param>
    /// <param name="namespaceOrModule">The namespace or module to set an alias for.</param>
    public static void localAlias(string alias, string namespaceOrModule) { }

    /// <summary>
    /// Assigns an alias to a namespace or module name.
    /// </summary>
    /// <remarks>This function has no effect on its own, as it is evaluated by the LoschScript compiler at compile time.</remarks>
    /// <param name="alias">The alias to set.</param>
    /// <param name="namespaceOrModule">The namespace or module to set an alias for.</param>
    public static void globalAlias(string alias, string namespaceOrModule) { }
}