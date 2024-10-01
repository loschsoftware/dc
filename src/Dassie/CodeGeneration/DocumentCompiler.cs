using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Configuration;
using Dassie.Data;
using Dassie.Errors;
using Dassie.Lowering;
using Dassie.Parser;
using Dassie.Validation;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Dassie.CodeGeneration;

internal static class DocumentCompiler
{
    public static ErrorInfo[] CompileDocument(InputDocument document, DassieConfig config)
    {
        if (config.Verbosity >= 1)
            EmitBuildLogMessage($"Compiling source file '{document.Name}'.");

        SetupBogusAssembly();

        Context.Files.Add(new(document.Name));
        CurrentFile = Context.GetFile(document.Name);

        if (!config.ImplicitImports)
        {
            CurrentFile.Imports.Clear();
            CurrentFile.ImportedTypes.Clear();
        }

        if (!config.ImplicitTypeAliases)
            CurrentFile.Aliases.Clear();

        CurrentFile.SymbolDocumentWriter = Context.Module.DefineDocument(document.Name);

        if (config.Verbosity >= 1)
            EmitBuildLogMessage("    Lowering...");

        string lowered = SourceFileRewriter.Rewrite(document.Text);

        Directory.CreateDirectory(".temp");
        string intermediatePath = Path.Combine(".temp", Path.GetFileNameWithoutExtension(document.Name) + ".i.ds");
        File.WriteAllText(intermediatePath, lowered);

        if (config.Verbosity >= 1)
            EmitBuildLogMessage("    Parsing...");

        ICharStream charStream = CharStreams.fromString(lowered);
        DassieLexer lexer = new DassieLexer(charStream);
        ITokenStream tokens = new CommonTokenStream(lexer);
        DassieParser parser = new(tokens);

        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new LexerErrorListener());
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ParserErrorListener());

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
            if (File.Exists(intermediatePath))
                File.Delete(intermediatePath);

            Directory.Delete(".temp", true);
        }

        return CurrentFile.Errors.ToArray();
    }
}