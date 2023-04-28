﻿using Losch.LoschScript.Configuration;
using LoschScript.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoschScript.CodeGeneration;

/// <summary>
/// Provides functionality for compiling LoschScript programs.
/// </summary>
public static class Compiler
{
    internal static TextWriter LogOut { get; set; } = Console.Out;

    /// <summary>
    /// Compiles the specified LoschScript source code.
    /// </summary>
    /// <param name="sourceFiles">An array of paths to the files to compile.</param>
    /// <param name="outputPath">The executable file name to be produced.</param>
    /// <param name="type">The type of the application.</param>
    /// <param name="config">Additional configuration.</param>
    /// <returns>Returns a list of errors that occured during compilation for every file.</returns>
    public static IEnumerable<ErrorInfo[]> CompileSource(string[] sourceFiles, string outputPath, ApplicationType type, LSConfig config = null)
    {
        config.AssemblyName = outputPath;
        config.ApplicationType = type;

        return CompileSource(sourceFiles, config ?? new());
    }

    /// <summary>
    /// Compiles all LoschScript source files in the specified directory.
    /// </summary>
    /// <param name="rootDirectory">The root directory containing source files to be compiled.</param>
    /// <param name="includeSubDirectories">Specifies wheter source files in subdirectories should be included in the compilation process.</param>
    /// <param name="config">Optional configuration for the compiler.</param>
    /// <returns></returns>
    public static IEnumerable<ErrorInfo[]> CompileSource(string rootDirectory, bool includeSubDirectories, LSConfig config = null)
    {
        return CompileSource(Directory.EnumerateFiles(rootDirectory, "*.ls", includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToArray(), config);
    }

    /// <summary>
    /// Compiles the specified LoschScript source code.
    /// </summary>
    /// <param name="sourceFiles">An array of paths to the files to compile.</param>
    /// <param name="config">Optional configuration for the compiler.</param>
    /// <returns>Returns a list of errors that occured during compilation for every file.</returns>
    public static IEnumerable<ErrorInfo[]> CompileSource(string[] sourceFiles, LSConfig config = null)
    {
        LogOut.WriteLine("Compiling files...");

        foreach (string file in sourceFiles)
            yield return FileCompiler.CompileSingleFile(file, config ?? new());
    }
}