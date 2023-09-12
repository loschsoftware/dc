﻿using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Losch.LoschScript.Configuration;
using LoschScript.CLI;
using LoschScript.Errors;
using LoschScript.Parser;
using LoschScript.Text.FragmentStore;
using LoschScript.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace LoschScript.CodeGeneration;

/// <summary>
/// Provides functionality for compiling single LoschScript source files.
/// </summary>
public static class FileCompiler
{
    /// <summary>
    /// Compiles a LoschScript source file.
    /// </summary>
    /// <param name="path">The path to the file to compile.</param>
    /// <param name="config">The compiler configuration.</param>
    /// <returns>An array of compilation errors that occured during the compilation. If no errors occured, this is an empty array.</returns>
    public static ErrorInfo[] CompileSingleFile(string path, LSConfig config)
    {
        Helpers.SetupBogusAssembly();

        Context.Files.Add(new(path));
        CurrentFile = Context.GetFile(path);

        string source = File.ReadAllText(path);

        ICharStream charStream = CharStreams.fromString(source);
        ITokenSource lexer = new LoschScriptLexer(charStream);
        ITokenStream tokens = new CommonTokenStream(lexer);

        LoschScriptParser parser = new(tokens);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new SyntaxErrorListener());

        Reference[] refs = ReferenceValidation.ValidateReferences(config.References);
        var refsToAdd = refs.Where(r => r is AssemblyReference).Select(r => Assembly.LoadFrom((r as AssemblyReference).AssemblyPath));

        if (refsToAdd != null)
            Context.ReferencedAssemblies.AddRange(refsToAdd);

        IParseTree compilationUnit = parser.compilation_unit();

        ExpressionEvaluator eval = new();

        Visitor v = new(eval);
        v.VisitCompilation_unit((LoschScriptParser.Compilation_unitContext)compilationUnit);

        return CurrentFile.Errors.ToArray();
    }

    /// <summary>
    /// Gets metadata for a LoschScript source file used to support language-specific features of text editors.
    /// </summary>
    /// <param name="source">The source code to emit fragments for.</param>
    /// <param name="config">The compiler configuration.</param>
    /// <returns>The editor info of the source code.</returns>
    public static EditorInfo GetEditorInfo(string source, LSConfig config)
    {
        FileFragment ffrag = new()
        {
            FilePath = "",
            Fragments = new()
        };

        try
        {
            Context = new();
            CurrentFile = new("");

            Context.Configuration = config;

            Helpers.SetupBogusAssembly();

            ICharStream charStream = CharStreams.fromString(source);
            ITokenSource lexer = new LoschScriptLexer(charStream);
            ITokenStream tokens = new CommonTokenStream(lexer);

            LoschScriptParser parser = new(tokens);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new SyntaxErrorListener());

            Reference[] refs = ReferenceValidation.ValidateReferences(config.References);
            var refsToAdd = refs.Where(r => r is AssemblyReference).Select(r => Assembly.LoadFrom((r as AssemblyReference).AssemblyPath));

            if (refsToAdd != null)
                Context.ReferencedAssemblies.AddRange(refsToAdd);

            IParseTree compilationUnit = parser.compilation_unit();

            ExpressionEvaluator eval = new();

            Visitor v = new(eval);
            v.VisitCompilation_unit((LoschScriptParser.Compilation_unitContext)compilationUnit);

            ffrag.Fragments.AddRange(CurrentFile.Fragments);
        }
        catch (Exception ex)
        {
            string dir = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Losch", "Script")).FullName;

            using StreamWriter sw = File.AppendText(Path.Combine(dir, "exception.log"));
            sw.WriteLine(ex.ToString());
        }

        return new()
        {
            Fragments = ffrag,
            Errors = CurrentFile.Errors,
            FoldingRegions = CurrentFile.FoldingRegions,
            GuideLines = CurrentFile.GuideLines
        };
    }
}