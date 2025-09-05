using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Cli.Commands;
using Dassie.Parser;
using System.Collections.Generic;
using System.Linq;

namespace Dassie.Lowering;

internal static class SourceFileRewriter
{
    public static string Rewrite(string source)
    {
        ICharStream charStream = CharStreams.fromString(source);
        DassieLexer lexer = new(charStream);
        CommonTokenStream tokenStream = new(lexer);
        DassieParser parser = new(tokenStream);
        IEnumerable<IToken> tokens = tokenStream.GetTokens();

        lexer.RemoveErrorListeners();
        parser.RemoveErrorListeners();

        IParseTree compilationUnit = parser.compilation_unit();
        
        EmitBuildLogMessage("Lexer tokens:", 3);
        foreach ((int i, IToken token) in tokens.Index())
            EmitBuildLogMessage($"#{i + 1} [{token.StartIndex}-{token.StopIndex}] {DassieLexer.DefaultVocabulary.GetSymbolicName(token.Type)}: \"{token.Text}\"", 3);

        EmitBuildLogMessage("Parse tree structure:", 3);
        EmitBuildLogMessage(DbgCommand.ParseTreePrinter.PrintTree(compilationUnit, parser), 3);

        LoweringListener lowerer = new(charStream, source);
        ParseTreeWalker.Default.Walk(lowerer, compilationUnit);

        return lowerer.Text.ToString();
    }
}