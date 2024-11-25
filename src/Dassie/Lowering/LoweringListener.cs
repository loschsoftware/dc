using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Dassie.Parser;
using System.Text;

namespace Dassie.Lowering;

internal class LoweringListener : DassieParserBaseListener
{
    public ICharStream CharStream { get; set; }
    public StringBuilder Text { get; set; }

    public LoweringListener(ICharStream cs, string text)
    {
        CharStream = cs;
        Text = new(text);
    }

    readonly CompoundAssignmentRewriter compoundAssignmentRewriter = new();
    readonly AccessModifierGroupRewriter accessModifierGroupRewriter = new();
    readonly InterpolatedStringRewriter interpolatedStringRewriter = new();
    readonly ForEachLoopRewriter forEachLoopRewriter = new();
    readonly PipeRewriter pipeRewriter = new();
    readonly ParameterConstraintRewriter constraintRewriter = new();
    readonly MatchExpressionRewriter matchExpressionRewriter = new();

    public string GetTextForRule(ParserRuleContext rule)
    {
        if (rule == null)
            return "";

        return CharStream.GetText(new(rule.Start.StartIndex, rule.Stop.StopIndex));
    }

    public StringBuilder Replace(string text, ParserRuleContext rule)
    {
        try
        {
            int start = Text.ToString().IndexOf(GetTextForRule(rule), rule.Start.StartIndex);
            return Text.Replace(GetTextForRule(rule), text, start, GetTextForRule(rule).Length);
        }
        catch
        {
            return Text;
        }
    }

    public override void EnterAssignment([NotNull] DassieParser.AssignmentContext context)
    {
        Text = Replace(compoundAssignmentRewriter.Rewrite(context, this), context);
    }

    public override void EnterLocal_declaration_or_assignment([NotNull] DassieParser.Local_declaration_or_assignmentContext context)
    {
        Text = Replace(compoundAssignmentRewriter.Rewrite(context, this), context);
    }

    public override void EnterAccess_modifier_member_group([NotNull] DassieParser.Access_modifier_member_groupContext context)
    {
        if (context.member_access_modifier() != null && context.member_access_modifier().Global() != null)
        {
            EmitWarningMessage(
                context.member_access_modifier().Start.Line,
                context.member_access_modifier().Start.Column,
                context.member_access_modifier().GetText().Length,
                DS0078_RedundantAccessModifierGroup,
                "Access modifier group 'global' is redundant since it is the default access modifier.");
        }

        Text = Replace(accessModifierGroupRewriter.Rewrite(context, this), context);
    }

    public override void EnterString_atom([NotNull] DassieParser.String_atomContext context)
    {
        Text = Replace(interpolatedStringRewriter.Rewrite(context, this), context);
    }

    public override void EnterForeach_loop([NotNull] DassieParser.Foreach_loopContext context)
    {
        Text = Replace(forEachLoopRewriter.Rewrite(context, this), context);
    }

    public override void EnterRight_pipe_expression([NotNull] DassieParser.Right_pipe_expressionContext context)
    {
        Text = Replace(pipeRewriter.Rewrite(context, this), context);
    }

    public override void EnterLeft_pipe_expression([NotNull] DassieParser.Left_pipe_expressionContext context)
    {
        Text = Replace(pipeRewriter.Rewrite(context, this), context);
    }

    public override void EnterType_member([NotNull] DassieParser.Type_memberContext context)
    {
        Text = Replace(constraintRewriter.Rewrite(context, this), context);
    }

    public override void EnterMatch_expression([NotNull] DassieParser.Match_expressionContext context)
    {
        Text = Replace(matchExpressionRewriter.Rewrite(context, this), context);
    }
}