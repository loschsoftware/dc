namespace Dassie.Syntax.Synthesis;

internal static partial class SyntaxFactory
{
    public static class Expressions
    {
        public static LiteralExpressionSyntax Literal(SyntaxToken literalToken) => new()
        {
            FirstToken = literalToken,
            LastToken = literalToken,
            LiteralToken = literalToken,
            Value = literalToken.Value
        };
    }
}