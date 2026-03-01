using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Core.Commands;
using Dassie.Parser;
using Dassie.Resources;
using System.Collections.Generic;
using System.Linq;
using static Dassie.Messages.MessageWriter;

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
        
        EmitBuildLogMessageFormatted(nameof(StringHelper.SourceFileRewriter_LexerTokens), [], 3);
        foreach ((int i, IToken token) in tokens.Index())
            EmitBuildLogMessageFormatted(nameof(StringHelper.SourceFileRewriter_TokenDetail), [i + 1, token.StartIndex, token.StopIndex, DassieLexer.DefaultVocabulary.GetSymbolicName(token.Type), token.Text], 3);

        EmitBuildLogMessageFormatted(nameof(StringHelper.SourceFileRewriter_ParseTreeStructure), [], 3);
        EmitBuildLogMessage(DbgCommand.ParseTreePrinter.PrintTree(compilationUnit, parser), 3);

        LoweringListener lowerer = new(charStream, source);
        ParseTreeWalker.Default.Walk(lowerer, compilationUnit);

        return lowerer.Text.ToString();
    }
}