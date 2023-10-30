using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Configuration;
using Dassie.CLI;
using Dassie.Errors;
using Dassie.Lowering;
using Dassie.Parser;
using Dassie.Text.FragmentStore;
using Dassie.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace Dassie.CodeGeneration;

/// <summary>
/// Provides functionality for compiling single Dassie source files.
/// </summary>
public static class FileCompiler
{
    /// <summary>
    /// Compiles a Dassie source file.
    /// </summary>
    /// <param name="path">The path to the file to compile.</param>
    /// <param name="config">The compiler configuration.</param>
    /// <returns>An array of compilation errors that occured during the compilation. If no errors occured, this is an empty array.</returns>
    public static ErrorInfo[] CompileSingleFile(string path, DassieConfig config)
    {
        Helpers.SetupBogusAssembly();

        Context.Files.Add(new(path));
        CurrentFile = Context.GetFile(path);

        if (!config.ImplicitImports)
        {
            CurrentFile.Imports.Clear();
            CurrentFile.ImportedTypes.Clear();
        }

        if (!config.ImplicitTypeAliases)
            CurrentFile.Aliases.Clear();

        string source = File.ReadAllText(path);
        string lowered = SourceFileRewriter.Rewrite(source);

        Directory.CreateDirectory("obj");
        string intermediatePath = Path.Combine("obj", Path.GetFileNameWithoutExtension(path) + ".i.ds");
        File.WriteAllText(intermediatePath, lowered);

        ICharStream charStream = CharStreams.fromString(lowered);
        ITokenSource lexer = new DassieLexer(charStream);
        ITokenStream tokens = new CommonTokenStream(lexer);

        DassieParser parser = new(tokens);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new SyntaxErrorListener());

        Reference[] refs = ReferenceValidation.ValidateReferences(config.References);
        var refsToAdd = refs.Where(r => r is AssemblyReference).Select(r => Assembly.LoadFrom((r as AssemblyReference).AssemblyPath));

        if (refsToAdd != null)
            Context.ReferencedAssemblies.AddRange(refsToAdd);

        IParseTree compilationUnit = parser.compilation_unit();

        SymbolListener listener = new();
        ParseTreeWalker.Default.Walk(listener, compilationUnit);

        ExpressionEvaluator eval = new();

        Visitor v = new(eval);
        v.VisitCompilation_unit((DassieParser.Compilation_unitContext)compilationUnit);

        if (!config.KeepIntermediateFiles)
        {
            File.Delete(intermediatePath);
            Directory.Delete("obj");
        }

        return CurrentFile.Errors.ToArray();
    }

    /// <summary>
    /// Gets metadata for a Dassie source file used to support language-specific features of text editors.
    /// </summary>
    /// <param name="source">The source code to emit fragments for.</param>
    /// <param name="config">The compiler configuration.</param>
    /// <returns>The editor info of the source code.</returns>
    public static EditorInfo GetEditorInfo(string source, DassieConfig config)
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
            ITokenSource lexer = new DassieLexer(charStream);
            ITokenStream tokens = new CommonTokenStream(lexer);

            DassieParser parser = new(tokens);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new SyntaxErrorListener());

            Reference[] refs = ReferenceValidation.ValidateReferences(config.References);
            var refsToAdd = refs.Where(r => r is AssemblyReference).Select(r => Assembly.LoadFrom((r as AssemblyReference).AssemblyPath));

            if (refsToAdd != null)
                Context.ReferencedAssemblies.AddRange(refsToAdd);

            IParseTree compilationUnit = parser.compilation_unit();

            ExpressionEvaluator eval = new();

            Visitor v = new(eval);
            v.VisitCompilation_unit((DassieParser.Compilation_unitContext)compilationUnit);

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