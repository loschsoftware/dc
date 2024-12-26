using System;

#pragma warning disable IDE1006
#pragma warning disable IDE0060

namespace Dassie.CompilerServices;

/// <summary>
/// Provides special functions for interacting with the Dassie code generator.
/// </summary>
public static class CodeGeneration
{
    /// <summary>
    /// Emits an IL instruction.
    /// </summary>
    /// <remarks>This function has no effect on its own, as it is evaluated by the Dassie compiler at compile time.</remarks>
    /// <param name="instruction">The IL instruction to emit, in human-readable format.</param>
    public static void il(string instruction) { }

    /// <summary>
    /// Imports a namespace or module into the current compilation unit.
    /// </summary>
    /// <remarks>This function has no effect on its own, as it is evaluated by the Dassie compiler at compile time.</remarks>
    /// <param name="namespace">The namespace or module to import.</param>
    public static void localImport(string @namespace) { }

    /// <summary>
    /// Imports a namepace or module into the current project.
    /// </summary>
    /// <remarks>This function has no effect on its own, as it is evaluated by the Dassie compiler at compile time.</remarks>
    /// <param name="namespace">The namespace or module to import.</param>
    public static void globalImport(string @namespace) { }

    /// <summary>
    /// Assigns an alias to a namespace or module name.
    /// </summary>
    /// <remarks>This function has no effect on its own, as it is evaluated by the Dassie compiler at compile time.</remarks>
    /// <param name="alias">The alias to set.</param>
    /// <param name="namespaceOrModule">The namespace or module to set an alias for.</param>
    public static void localAlias(string alias, string namespaceOrModule) { }

    /// <summary>
    /// Assigns an alias to a namespace or module name.
    /// </summary>
    /// <remarks>This function has no effect on its own, as it is evaluated by the Dassie compiler at compile time.</remarks>
    /// <param name="alias">The alias to set.</param>
    /// <param name="namespaceOrModule">The namespace or module to set an alias for.</param>
    public static void globalAlias(string alias, string namespaceOrModule) { }

    /// <summary>
    /// Prints a message to the standard output indicating that a certain operation is not yet implemented.
    /// </summary>
    /// <remarks>This function has no effect on its own, as it is evaluated by the Dassie compiler at compile time.</remarks>
    /// <param name="message">The todo message to print.</param>
    public static void todo(string message) { }

    /// <summary>
    /// Throws a <see cref="NotImplementedException"/> with a message indicating that a certain operation is not yet implemented."
    /// </summary>
    /// <param name="message">The todo message to print.</param>
    public static void ptodo(string message) { }

    /// <summary>
    /// Returns the line number in the source code of the invocation.
    /// </summary>
    /// <remarks>This function has no effect on its own, as it is evaluated by the Dassie compiler at compile time.</remarks>
    /// <returns>The line number of the invocation.</returns>
    public static int line() => 0;

    /// <summary>
    /// Sets the current line number.
    /// </summary>
    /// <param name="line">The line number to set to.</param>
    public static void setLine(int line) { }

    /// <summary>
    /// Sets the line number offset.
    /// </summary>
    /// <param name="offset">The offset to set.</param>
    public static void setLineOffset(int offset) { }
}