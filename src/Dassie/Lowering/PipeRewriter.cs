using Antlr4.Runtime.Tree;
using Dassie.Parser;
using System.Linq;

namespace Dassie.Lowering;

internal class PipeRewriter : ITreeToStringRewriter
{
    // TODO: Rewrite so it supports more than one argument
    public string Rewrite(IParseTree tree, LoweringListener listener)
    {
        if (tree is DassieParser.Right_pipe_expressionContext r)
            return $"{listener.GetTextForRule(r.expression().Last())} {listener.GetTextForRule(r.expression().First())}";

        var l = (DassieParser.Left_pipe_expressionContext)tree;
        return $"{listener.GetTextForRule(l.expression().First())} {listener.GetTextForRule(l.expression().Last())}";
    }
}