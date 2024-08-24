using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.CodeAnalysis.Structure;
using Dassie.Configuration;
using Dassie.Data;
using Dassie.Errors;
using Dassie.Parser;
using Dassie.Text.FragmentStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dassie.CodeGeneration;

/// <summary>
/// Provides functionality for compiling Dassie programs.
/// </summary>
public static class Compiler
{
    internal static TextWriter LogOut { get; set; } = Console.Out;

    /// <summary>
    /// Compiles the specified Dassie source code.
    /// </summary>
    /// <param name="sourceFiles">An array of paths to the files to compile.</param>
    /// <param name="outputPath">The executable file name to be produced.</param>
    /// <param name="type">The type of the application.</param>
    /// <param name="config">Additional configuration.</param>
    /// <returns>Returns a list of errors that occured during compilation for every file.</returns>
    public static IEnumerable<ErrorInfo[]> CompileSource(string[] sourceFiles, string outputPath, ApplicationType type, DassieConfig config = null)
    {
        config.AssemblyName = outputPath;
        config.ApplicationType = type;

        return CompileSource(sourceFiles, config ?? new());
    }

    /// <summary>
    /// Compiles all Dassie source files in the specified directory.
    /// </summary>
    /// <param name="rootDirectory">The root directory containing source files to be compiled.</param>
    /// <param name="includeSubDirectories">Specifies wheter source files in subdirectories should be included in the compilation process.</param>
    /// <param name="config">Optional configuration for the compiler.</param>
    /// <returns></returns>
    public static IEnumerable<ErrorInfo[]> CompileSource(string rootDirectory, bool includeSubDirectories, DassieConfig config = null)
    {
        return CompileSource(Directory.EnumerateFiles(rootDirectory, "*.ds", includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToArray(), config);
    }

    internal static IEnumerable<ErrorInfo[]> CompileSource(IEnumerable<InputDocument> documents, DassieConfig config = null, string configFileName = "dsconfig.xml")
    {
        DassieConfig cfg = config ?? new();

        Context = new()
        {
            Configuration = cfg,
            ConfigurationPath = configFileName
        };

        Context.FilePaths.AddRange(documents.Select(d => d.Name));

        if (config.Verbosity >= 1)
            EmitBuildLogMessage($"Compilation started at {DateTime.Now:HH:mm:ss} on {DateTime.Now.ToShortDateString()} at log verbosity level {config.Verbosity}.");

        string asmFileName = $"{config.AssemblyName}{(config.ApplicationType == ApplicationType.Library ? ".dll" : ".exe")}";

        AssemblyName name = new(string.IsNullOrEmpty(config.AssemblyName) ? Path.GetFileNameWithoutExtension(documents.First().Name) : config.AssemblyName);
        //PersistedAssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(name, PersistedAssemblyBuilderAccess.RunAndSave);
        PersistedAssemblyBuilder ab = new(name, typeof(object).Assembly);

        ModuleBuilder mb = ab.DefineDynamicModule(config.AssemblyName);

        Context.Assembly = ab;
        Context.Module = mb;

        List<ErrorInfo[]> errors = new();

        foreach (InputDocument doc in documents)
            errors.Add(DocumentCompiler.CompileDocument(doc, cfg));

        if (config.ApplicationType != ApplicationType.Library && !Context.EntryPointIsSet)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0030_NoEntryPoint,
                "Program contains no entry point. Use the '<EntryPoint>' attribute to specify an application entry point.",
                "dc");
        }

        return errors;
    }

    /// <summary>
    /// Compiles the specified Dassie source code.
    /// </summary>
    /// <param name="sourceFiles">An array of paths to the files to compile.</param>
    /// <param name="config">Optional configuration for the compiler.</param>
    /// <param name="configFileName">The file path to the configuration file.</param>
    /// <returns>Returns a list of errors that occured during compilation for every file.</returns>
    public static IEnumerable<ErrorInfo[]> CompileSource(string[] sourceFiles, DassieConfig config = null, string configFileName = "dsconfig.xml")
    {
        return CompileSource(sourceFiles.Select(s => new InputDocument(File.ReadAllText(s), s)), config, configFileName);
    }

    /// <summary>
    /// Generates a code structure diagram based on a shallow analysis of the specified source files. Used by LSEdit for creating structure views.
    /// </summary>
    /// <param name="sourceFiles">An array of paths to the files to be included in the structure diagram.</param>
    /// <returns>A <see cref="ProjectStructure"/> representing the code structure of the current compilation context.</returns>
    public static ProjectStructure GenerateCodeStructure(string[] sourceFiles)
    {
        ProjectStructure structure = new()
        {
            Namespaces = [],
            Types = []
        };

        foreach (string path in sourceFiles)
        {
            ICharStream charStream = CharStreams.fromString(File.ReadAllText(path));
            ITokenSource lexer = new DassieLexer(charStream);
            ITokenStream tokens = new CommonTokenStream(lexer);

            DassieParser parser = new(tokens);
            StructureListener listener = new(structure, path);
            ParseTreeWalker.Default.Walk(listener, parser.compilation_unit());

            structure = listener.Structure;
        }

        // TODO: Structure isn't completely correct yet, but I can't be bothered to try to fix it right now
        return structure;
    }
}