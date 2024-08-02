using Dassie.CLI;
using Dassie.Errors;
using System.IO;

namespace Dassie.Compiler;

/// <summary>
/// Allows invoking the Dassie compiler.
/// </summary>
public class DassieCompiler
{
    internal DassieCompiler(string baseDir)
    {
        Directory.SetCurrentDirectory(baseDir);
    }

    /// <summary>
    /// Builds the project.
    /// </summary>
    /// <returns>An array of build errors that occured during the compilation.</returns>
    public ErrorInfo[] Build()
    {
        CliHelpers.CompileAll([]);
        return ErrorWriter.messages.ToArray();
    }

    /// <summary>
    /// Executes the specified build profile.
    /// </summary>
    /// <param name="profileName">The profile to build.</param>
    /// <returns>An array of build errors that occured during the compilation.</returns>
    public ErrorInfo[] ExecuteBuildProfile(string profileName)
    {
        CliHelpers.CompileAll([profileName]);
        return ErrorWriter.messages.ToArray();
    }
}