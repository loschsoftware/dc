using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Parser;

namespace Dassie.Lowering;

internal class CompoundAssignmentRewriter : ITreeToStringRewriter
{
    public string Rewrite(IParseTree tree, LoweringListener listener)
    {
        IParseTree op;
        string left, right, expr;

        if (tree is DassieParser.AssignmentContext a)
        {
            op = a.assignment_operator();
            left = a.expression()[0].GetText();
            expr = a.expression()[1].GetText();
            right = $"{a.expression()[0].GetText()} ";
        }

        else
        {
            var local = (DassieParser.Local_declaration_or_assignmentContext)tree;

            op = local.assignment_operator();
            left = $"{(local.Var() == null ? "" : "var ")}{local.Identifier().GetIdentifier()}{(local.type_name() == null ? " " : $": {local.type_name().GetText()}")}";
            expr = local.expression().GetText();
            right = $"{local.Identifier().GetIdentifier()} ";
        }

        if (op.GetText() == "=")
            return listener.GetTextForRule((ParserRuleContext)tree);

        right += op.GetText()[0..^1];
        right += $" {expr}";

        return $"{left} = {right}";
    }
}