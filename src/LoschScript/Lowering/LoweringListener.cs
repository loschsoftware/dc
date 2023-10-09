using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
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

    public string GetTextForRule(ParserRuleContext rule)
    {
        return CharStream.GetText(new(rule.Start.StartIndex, rule.Stop.StopIndex));
    }

    public override void EnterAssignment([NotNull] LoschScriptParser.AssignmentContext context)
    {
        Text = Text.Replace(GetTextForRule(context), compoundAssignmentRewriter.Rewrite(context, GetTextForRule(context)));
    }

    public override void EnterLocal_declaration_or_assignment([NotNull] LoschScriptParser.Local_declaration_or_assignmentContext context)
    {
        Text = Text.Replace(GetTextForRule(context), compoundAssignmentRewriter.Rewrite(context, GetTextForRule(context)));
    }
}