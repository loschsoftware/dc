using System;

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
    /// Emits an error message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The actual error message.</param>
    public static void emitError(string code, string message) { }

    /// <summary>
    /// Emits a warning message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The actual error message.</param>
    public static void emitWarning(string code, string message) { }
    
    /// <summary>
    /// Emits a message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The actual error message.</param>
    public static void emitMessage(string code, string message) { }

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
}