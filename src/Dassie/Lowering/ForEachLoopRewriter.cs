using Antlr4.Runtime.Tree;
using Dassie.Parser;
using System;
using System.Linq;
using System.Text;

namespace Dassie.Lowering;

internal class ForEachLoopRewriter : ITreeToStringRewriter
{
    public string Rewrite(IParseTree tree, LoweringListener listener)
    {
        string rndVarName = $"_{Guid.NewGuid():N}";
        string exprVarName = $"_{Guid.NewGuid():N}";
        
        DassieParser.Foreach_loopContext loop = (DassieParser.Foreach_loopContext)tree;

        StringBuilder sb = new();

        sb.AppendLine("{");

        sb.AppendLine($"var {rndVarName} = 0");
        sb.AppendLine($"{exprVarName} = {listener.GetTextForRule(loop.expression().First())}");

        sb.AppendLine($"@ {rndVarName} < ({exprVarName}.Length) = {{");

        sb.AppendLine($"\t{(loop.Var() != null ? "var " : "")}{loop.Identifier().GetText()} = ({exprVarName}::{rndVarName})");
        sb.AppendLine($"\t{listener.GetTextForRule(loop.expression().Last())}");
        sb.AppendLine($"\t{rndVarName} = {rndVarName} + 1");

        sb.AppendLine("}");
        sb.AppendLine("}");

        return sb.ToString();
    }
}