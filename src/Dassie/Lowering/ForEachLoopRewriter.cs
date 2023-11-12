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
        string indexVarName = $"_{Guid.NewGuid():N}";
        string exprVarName = $"_{Guid.NewGuid():N}";
        
        DassieParser.Foreach_loopContext loop = (DassieParser.Foreach_loopContext)tree;

        string elementNameIdentifier = loop.Identifier().Length == 2
            ? loop.Identifier()[1].GetText()
            : loop.Identifier()[0].GetText();

        if (loop.Identifier().Length == 2)
            indexVarName = loop.Identifier()[0].GetText();

        StringBuilder sb = new();

        sb.AppendLine("{");

        sb.AppendLine($"var {indexVarName} = -1");
        sb.AppendLine($"{exprVarName} = {listener.GetTextForRule(loop.expression().First())}");

        sb.AppendLine($"@ {indexVarName} < (({exprVarName}.Length) - 1) = {{");

        sb.AppendLine($"\t{indexVarName} = {indexVarName} + 1");
        sb.AppendLine($"\t{(loop.Var() != null ? "var " : "")}{elementNameIdentifier} = ({exprVarName}::{indexVarName})");

        sb.AppendLine($"\t{listener.GetTextForRule(loop.expression().Last())}");

        sb.AppendLine("}");
        sb.AppendLine("}");

        return sb.ToString();
    }
}