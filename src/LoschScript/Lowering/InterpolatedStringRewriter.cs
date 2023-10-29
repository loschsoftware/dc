using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using LoschScript.Lowering;

namespace LoschScript.Lowerin;

internal class InterpolatedStringRewriter : IRewriter
{
    public string Rewrite(IParseTree tree, LoweringListener listener)
    {
        return listener.GetTextForRule((ParserRuleContext)tree);
    }
}