using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Configuration;
using Dassie.Data;
using Dassie.Errors;
using Dassie.Lowering;
using Dassie.Parser;
using System.Collections.Generic;

namespace Dassie.CodeGeneration.Api;

internal static class DocumentCompiler
{
    public static DassieParser CreateParser(InputDocument document, DassieConfig config, out string intermediatePath)
    {
        if (string.IsNullOrEmpty(CurrentFile.Path))
            CurrentFile.Path = document.Name;

        intermediatePath = Path.Combine(TemporaryBuildDirectoryName, Path.GetFileNameWithoutExtension(document.Name) + ".i.ds");
        EmitBuildLogMessage($"Rewriting document '{document.Name}' -> '{intermediatePath}'.", 2);

        string lowered = SourceFileRewriter.Rewrite(document.Text);

        Directory.CreateDirectory(TemporaryBuildDirectoryName);
        File.WriteAllText(intermediatePath, lowered);

        EmitBuildLogMessage($"Parsing document '{intermediatePath}'.", 2);

        CurrentFile.CharStream = CharStreams.fromString(lowered);
        DassieLexer lexer = new(CurrentFile.CharStream);
        ITokenStream tokens = new CommonTokenStream(lexer);
        DassieParser parser = new(tokens);

        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new LexerErrorListener());
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ParserErrorListener());

        return parser;
    }

    public static List<ErrorInfo> CompileDocument(InputDocument document, DassieConfig config, IParseTree compilationUnit, string intermediatePath, DassieParser parser)
    {
        EmitBuildLogMessage($"Compiling document '{document.Name}'.", 2);

        SetupBogusAssembly();
        CurrentFile = Context.GetFile(document.Name);
        CurrentFile.Parser = parser;

        if (!config.ImplicitImports || config.NoStdLib)
        {
            CurrentFile.Imports.Clear();
            CurrentFile.ImportedTypes.Clear();
        }

        if (!config.ImplicitTypeAliases || config.NoStdLib)
            CurrentFile.Aliases.Clear();

        CurrentFile.SymbolDocumentWriter = Context.Module.DefineDocument(document.Name);

        Visitor v = new();
        v.Visit(compilationUnit);

        if (!config.KeepIntermediateFiles)
        {
            if (File.Exists(intermediatePath))
                File.Delete(intermediatePath);

            if (Directory.Exists(TemporaryBuildDirectoryName))
                Directory.Delete(TemporaryBuildDirectoryName, true);
        }

        return CurrentFile.Errors;
    }

    public static List<ErrorInfo> DeclareSymbols(InputDocument document, DassieConfig config, IParseTree compilationUnit)
    {
        SetupBogusAssembly();
        CurrentFile = Context.GetFile(document.Name);

        SymbolVisitor visitor = new();
        visitor.Visit(compilationUnit);
        return CurrentFile.Errors;
    }
}