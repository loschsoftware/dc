using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using LoschScript.Lowerin;
using LoschScript.Parser;
using System.Text;

namespace LoschScript.Lowering;

internal class LoweringListener : LoschScriptParserBaseListener
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

    public string GetTextForRule(ParserRuleContext rule)
    {
        return CharStream.GetText(new(rule.Start.StartIndex, rule.Stop.StopIndex));
    }

    public StringBuilder Replace(string text, ParserRuleContext rule)
    {
        int start = Text.ToString().IndexOf(GetTextForRule(rule), rule.Start.StartIndex);
        return Text.Replace(GetTextForRule(rule), text, start, GetTextForRule(rule).Length);
    }

    public override void EnterAssignment([NotNull] LoschScriptParser.AssignmentContext context)
    {
        Text = Replace(compoundAssignmentRewriter.Rewrite(context, this), context);
    }

    public override void EnterLocal_declaration_or_assignment([NotNull] LoschScriptParser.Local_declaration_or_assignmentContext context)
    {
        Text = Replace(compoundAssignmentRewriter.Rewrite(context, this), context);
    }

    public override void EnterAccess_modifier_member_group([NotNull] LoschScriptParser.Access_modifier_member_groupContext context)
    {
        if (context.member_access_modifier().Global() != null)
        {
            EmitWarningMessage(
                context.member_access_modifier().Start.Line,
                context.member_access_modifier().Start.Column,
                context.member_access_modifier().GetText().Length,
                LS0078_RedundantAccessModifierGroup,
                "Access modifier group 'global' is redundant since it is the default access modifier.");
        }

        Text = Replace(accessModifierGroupRewriter.Rewrite(context, this), context);
    }

    public override void EnterString_atom([NotNull] LoschScriptParser.String_atomContext context)
    {
        Text = Replace(interpolatedStringRewriter.Rewrite(context, this), context);
    }
}