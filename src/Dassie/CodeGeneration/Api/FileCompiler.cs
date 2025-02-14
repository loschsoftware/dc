using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.CodeGeneration.Helpers;
using Dassie.CodeGeneration.Structure;
using Dassie.Configuration;
using Dassie.Core;
using Dassie.Data;
using Dassie.Errors;
using Dassie.Meta;
using Dassie.Parser;
using Dassie.Text.FragmentStore;
using Dassie.Validation;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dassie.CodeGeneration.Api;

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
        string text = "";

        try
        {
            text = File.ReadAllText(path);
        }
        catch (IOException ex)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0029_FileAccessDenied,
                $"Could not read from '{path}': {ex.Message}",
                path);
        }

        InputDocument doc = new(text, path);
        DassieParser parser = DocumentCompiler.CreateParser(doc, config, out string intermediatePath);

        return DocumentCompiler.CompileDocument(doc, config, parser.compilation_unit(), intermediatePath).ToArray();
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

            SetupBogusAssembly();
            Context.Module = Context.BogusModule;

            GlobalConfig.DisableDebugInfo = true;

            ICharStream charStream = CharStreams.fromString(source);
            DassieLexer lexer = new DassieLexer(charStream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            DassieParser parser = new(tokens);

            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new LexerErrorListener());
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ParserErrorListener());

            Reference[] refs = ReferenceValidation.ValidateReferences(config.References);
            var refsToAdd = refs.Where(r => r is AssemblyReference).Select(r => Assembly.LoadFrom(Path.GetFullPath(Path.Combine(GlobalConfig.RelativePathResolverDirectory, (r as AssemblyReference).AssemblyPath))));

            if (refsToAdd != null)
                Context.ReferencedAssemblies.AddRange(refsToAdd);

            if (!config.NoStdLib)
                Context.ReferencedAssemblies.Add(typeof(stdout).Assembly);

            IParseTree compilationUnit = parser.compilation_unit();

            Visitor v = new();
            v.VisitCompilation_unit((DassieParser.Compilation_unitContext)compilationUnit);

            ffrag.Fragments.AddRange(CurrentFile.Fragments);
        }
        catch (Exception ex)
        {
            string dir = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie")).FullName;

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