using Antlr4.Runtime.Tree;

namespace Dassie.Lowering;

internal interface ITreeToStringRewriter
{
    public string Rewrite(IParseTree tree, LoweringListener listener);
}