namespace Dassie.Syntax.Synthesis;

internal static partial class SyntaxFactory
{
    public static class Literals
    {
        public static SyntaxToken BooleanLiteral(bool value) => new()
        {
            TokenKind = value ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword,
            Value = value,
            Text = value.ToString()
        };
    }
}