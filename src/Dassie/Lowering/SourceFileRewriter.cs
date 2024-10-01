using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Parser;

namespace Dassie.Lowering;

internal static class SourceFileRewriter
{
    public static string Rewrite(string source)
    {
        ICharStream charStream = CharStreams.fromString(source);
        DassieLexer lexer = new DassieLexer(charStream);
        ITokenStream tokens = new CommonTokenStream(lexer);
        DassieParser parser = new(tokens);

        lexer.RemoveErrorListeners();        
        parser.RemoveErrorListeners();

        IParseTree compilationUnit = parser.compilation_unit();

        LoweringListener lowerer = new(charStream, source);
        ParseTreeWalker.Default.Walk(lowerer, compilationUnit);

        return lowerer.Text.ToString();
    }
}