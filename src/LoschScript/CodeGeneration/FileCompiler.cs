using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Losch.LoschScript.Configuration;
using LoschScript.Errors;
using LoschScript.Parser;
using LoschScript.Text.FragmentStore;
using LoschScript.Validation;
using System;
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
    /// <param name="emitFragmentInfo">Wheter to emit fragments.</param>
    /// <returns>An array of compilation errors that occured during the compilation. If no errors occured, this is an empty array.</returns>
    public static ErrorInfo[] CompileSingleFile(string path, LSConfig config, bool emitFragmentInfo = false)
    {
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
        Visitor v = new();
        v.VisitCompilation_unit((LoschScriptParser.Compilation_unitContext)compilationUnit);

        if (emitFragmentInfo)
        {
            string dir = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "LoschScript", "Fragments")).FullName;

            FileFragment ffrag = new()
            {
                FilePath = path,
                Fragments = CurrentFile.Fragments
            };

            FragmentSerializer.Serialize(Path.Combine(dir, $"{Path.GetFileName(path)}.xml"), ffrag);
        }

        return CurrentFile.Errors.ToArray();
    }

    /// <summary>
    /// Emits fragments for a string of LoschScript source code.
    /// </summary>
    /// <param name="source">The source code to emit fragments for.</param>
    /// <param name="config">The compiler configuration.</param>
    /// <returns>Returns a <see cref="FileFragment"/> object containing the fragments of the source file. The <see cref="FileFragment.FilePath"/> property is set to an empty string.</returns>
    public static FileFragment GetFragments(string source, LSConfig config)
    {
        try
        {
            FileFragment ffrag = new()
            {
                FilePath = "",
                Fragments = new()
            };

            Context = new();
            CurrentFile = new("");

            Context.Configuration = config;

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
            Visitor v = new(false);
            v.VisitCompilation_unit((LoschScriptParser.Compilation_unitContext)compilationUnit);

            ffrag.Fragments.AddRange(CurrentFile.Fragments);

            return ffrag;
        }
        catch (Exception ex)
        {
            File.AppendAllText("exception.txt", ex.ToString());

            return new()
            {
                Fragments = new()
            };
        }
    }
}