using Antlr4.Runtime.Tree;

namespace LoschScript.Lowering;

internal interface IRewriter
{
    public string Rewrite(IParseTree tree, string originalText);
}