using Dassie.Configuration;
using Dassie.Errors;
using Dassie.Text.FragmentStore;
using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Compiles the specified Dassie source code.
    /// </summary>
    /// <param name="sourceFiles">An array of paths to the files to compile.</param>
    /// <param name="config">Optional configuration for the compiler.</param>
    /// <param name="configFileName">The file path to the configuration file.</param>
    /// <returns>Returns a list of errors that occured during compilation for every file.</returns>
    public static IEnumerable<ErrorInfo[]> CompileSource(string[] sourceFiles, DassieConfig config = null, string configFileName = "dsconfig.xml")
    {
        DassieConfig cfg = config ?? new();

        Context = new();
        Context.Configuration = cfg;
        Context.ConfigurationPath = configFileName;

        string asmFileName = $"{config.AssemblyName}{(config.ApplicationType == ApplicationType.Library ? ".dll" : ".exe")}";

        AssemblyName name = new(string.IsNullOrEmpty(config.AssemblyName) ? Path.GetFileNameWithoutExtension(sourceFiles[0]) : config.AssemblyName);
        AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);

        ModuleBuilder mb = ab.DefineDynamicModule(asmFileName, asmFileName, config.CreatePdb || config.Configuration == ApplicationConfiguration.Debug);

        Context.Assembly = ab;
        Context.Module = mb;

        List<ErrorInfo[]> errors = new();

        foreach (string file in sourceFiles)
            errors.Add(FileCompiler.CompileSingleFile(file, cfg));

        if (config.ApplicationType != ApplicationType.Library && !Context.EntryPointIsSet)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0030_NoEntryPoint,
                "Program contains no entry point. Use the '<EntryPoint>' attribute to specify an application entry point.");
        }

        return errors;
    }
}