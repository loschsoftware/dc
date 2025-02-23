using Antlr4.Runtime.Tree;
using Dassie.Parser;
using System.Linq;
using System.Text;

namespace Dassie.Lowering;

internal class LocalFunctionRewriter : ITreeToStringRewriter
{
    public string Rewrite(IParseTree tree, LoweringListener listener)
    {
        StringBuilder sb = new();
        DassieParser.Local_functionContext localFunc = (DassieParser.Local_functionContext)tree;

        StringBuilder paramListSb = new();
        if (localFunc.parameter_list().parameter() != null)
        {
            foreach (DassieParser.ParameterContext param in localFunc.parameter_list().parameter()[..^1])
                paramListSb.Append($"{listener.GetTextForRule(param)}, ");

            paramListSb.Append($"{listener.GetTextForRule(localFunc.parameter_list().parameter().Last())}");
        }

        sb.AppendLine($"{localFunc.Identifier().GetIdentifier()} = (func ({paramListSb.ToString()}) => {{{listener.GetTextForRule(localFunc.expression())}}})");

        return sb.ToString();
    }
}