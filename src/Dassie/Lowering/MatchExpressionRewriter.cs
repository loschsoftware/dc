﻿using Antlr4.Runtime.Tree;
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
        sb.AppendLine($"var {matchedValueVarName} = {listener.GetTextForRule(matchExpr.expression())}");

        if (matchExpr.match_block().match_first_case() != null)
            sb.AppendLine($"? {GetTextForCaseExpression(matchedValueVarName, matchExpr.match_block().match_first_case().match_case_expression(), listener)} = {listener.GetTextForRule(matchExpr.match_block().match_first_case().expression())}");

        foreach (DassieParser.Match_alternative_caseContext matchCase in matchExpr.match_block().match_alternative_case() ?? [])
            sb.AppendLine($": {GetTextForCaseExpression(matchedValueVarName, matchCase.match_case_expression(), listener)} = {listener.GetTextForRule(matchCase.expression())}");

        if (matchExpr.match_block().match_default_case() != null)
            sb.AppendLine($": = {listener.GetTextForRule(matchExpr.match_block().match_default_case().expression())}");

        sb.AppendLine("}");
        return sb.ToString();
    }

    private string GetTextForCaseExpression(string matchedValueVarName, DassieParser.Match_case_expressionContext caseExpr, LoweringListener listener)
    {
        return $"{matchedValueVarName} == {listener.GetTextForRule(caseExpr)}";
    }
}