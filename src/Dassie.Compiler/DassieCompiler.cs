using Dassie.Cli;
using Dassie.Cli.Commands;
using Dassie.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dassie.Compiler;

/// <summary>
/// Provides functionality for compiling Dassie programs.
/// </summary>
public static class DassieCompiler
{
    /// <summary>
    /// Compiles a Dassie program.
    /// </summary>
    /// <param name="context">A <see cref="CompilationContext"/> object containing information about the compilation.</param>
    /// <returns>The result of the compilation.</returns>
    /// <exception cref="ArgumentNullException"/>
    public static CompilationResult Compile(this CompilationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        List<string> arglist = [];
        foreach (SourceDocument doc in context.Documents)
            arglist.Add($"--Document:{doc.SymbolicName}:{doc.SourceText}");

        int result = CompileCommand.Instance.Invoke(arglist.ToArray(), context.Configuration);
        bool success = result == 0 && !ErrorWriter.messages.Where(m => m.Severity == Severity.Error).Any();
        return new(success, ErrorWriter.messages);
    }

    private static CompilationResult CompileProjectWithArguments(string projectFilePath, string[] args)
    {
        ArgumentNullException.ThrowIfNull(projectFilePath);
        if (!File.Exists(projectFilePath))
            throw new FileNotFoundException();

        Directory.SetCurrentDirectory(Path.GetDirectoryName(projectFilePath));
        int result = BuildCommand.Instance.Invoke(args);
        bool success = result == 0 && !ErrorWriter.messages.Where(m => m.Severity == Severity.Error).Any();
        return new(success, ErrorWriter.messages);
    }

    /// <summary>
    /// Compiles a Dassie project stored on disk.
    /// </summary>
    /// <param name="projectFilePath">The path to the project file (dsconfig.xml).</param>
    /// <param name="buildProfile">The build profile to use.</param>
    /// <returns>The result of the compilation.</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="FileNotFoundException"/>
    public static CompilationResult CompileProject(string projectFilePath, string buildProfile)
    {
        ArgumentNullException.ThrowIfNull(buildProfile);
        return CompileProjectWithArguments(projectFilePath, [buildProfile]);
    }

    /// <summary>
    /// Compiles a Dassie project stored on disk.
    /// </summary>
    /// <param name="projectFilePath">The path to the project file (dsconfig.xml).</param>
    /// <returns>The result of the compilation.</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="FileNotFoundException"/>
    public static CompilationResult CompileProject(string projectFilePath) => CompileProjectWithArguments(projectFilePath, []);
}