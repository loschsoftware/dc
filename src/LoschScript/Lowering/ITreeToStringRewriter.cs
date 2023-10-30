using Antlr4.Runtime.Tree;

namespace LoschScript.Lowering;

internal interface ITreeToStringRewriter
{
    public string Rewrite(IParseTree tree, LoweringListener listener);
}