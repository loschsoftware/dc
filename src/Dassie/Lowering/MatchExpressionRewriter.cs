using Antlr4.Runtime.Tree;
using Dassie.Parser;
using System;
using System.Text;

namespace Dassie.Lowering;

internal class MatchExpressionRewriter : ITreeToStringRewriter
{
    public string Rewrite(IParseTree tree, LoweringListener listener)
    {
        StringBuilder sb = new();
        DassieParser.Match_exprContext matchExpr = (tree as DassieParser.Match_expressionContext).match_expr();

        string matchedValueVarName = $"_{Guid.NewGuid():N}";

        sb.AppendLine("{");
        sb.AppendLine($"{matchedValueVarName} = {listener.GetTextForRule(matchExpr.expression())}");

        if (matchExpr.match_block().match_first_case() != null)
            sb.AppendLine($"? {matchedValueVarName} == {listener.GetTextForRule(matchExpr.match_block().match_first_case().match_case_expression())} = {listener.GetTextForRule(matchExpr.match_block().match_first_case().expression())}");

        foreach (DassieParser.Match_alternative_caseContext matchCase in matchExpr.match_block().match_alternative_case() ?? [])
            sb.AppendLine($": {matchedValueVarName} == {listener.GetTextForRule(matchCase.match_case_expression())} = {listener.GetTextForRule(matchCase.expression())}");

        if (matchExpr.match_block().match_default_case() != null)
            sb.AppendLine($": = {listener.GetTextForRule(matchExpr.match_block().match_default_case().expression())}");

        sb.AppendLine("}");
        return sb.ToString();
    }
}