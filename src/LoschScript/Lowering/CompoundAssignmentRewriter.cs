using Antlr4.Runtime.Tree;
using LoschScript.Parser;
using System;
using System.Collections.Generic;

namespace LoschScript.Lowering;

internal class CompoundAssignmentRewriter : IRewriter
{
    public string Rewrite(IParseTree tree, string originalText)
    {
        IParseTree op;
        string left, right, expr;

        if (tree is LoschScriptParser.AssignmentContext a)
        {
            op = a.assignment_operator();
            left = a.expression()[0].GetText();
            expr = a.expression()[1].GetText();
            right = $"{a.expression()[0].GetText()} ";
        }

        else
        {
            var local = (LoschScriptParser.Local_declaration_or_assignmentContext)tree;

            op = local.assignment_operator();
            left = $"{(local.Var() == null ? "" : "var ")}{local.Identifier().GetText()}{(local.type_name() == null ? " " : $": {local.type_name().GetText()}")}";
            expr = local.expression().GetText();
            right = $"{local.Identifier().GetText()} ";
        }

        if (op.GetText() == "=")
            return originalText;

        right += op.GetText()[0..^1];
        right += $" {expr}";

        return $"{left} = {right}";
    }
}