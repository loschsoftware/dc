using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using LoschScript.Parser;

namespace LoschScript.Lowering;

internal static class SourceFileRewriter
{
    public static string Rewrite(string source)
    {
        ICharStream charStream = CharStreams.fromString(source);
        ITokenSource lexer = new LoschScriptLexer(charStream);
        ITokenStream tokens = new CommonTokenStream(lexer);

        LoschScriptParser parser = new(tokens);
        parser.RemoveErrorListeners();

        IParseTree compilationUnit = parser.compilation_unit();

        LoweringVisitor lowerer = new(charStream);
        lowerer.Visit(compilationUnit);

        // TODO: Return early until LoweringVisitor is implemented
        return source;

        return lowerer.Builder.ToString();
    }
}