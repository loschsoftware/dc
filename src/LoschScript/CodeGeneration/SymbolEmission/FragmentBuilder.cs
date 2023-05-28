using Antlr4.Runtime.Misc;
using LoschScript.Parser;
using LoschScript.Text;
using System.Linq;

namespace LoschScript.CodeGeneration.SymbolEmission;

internal class FragmentBuilder : LoschScriptParserBaseListener
{
    public override void EnterLocal_declaration_or_assignment([NotNull] LoschScriptParser.Local_declaration_or_assignmentContext context)
    {
        CurrentFile.Fragments.Add(new()
        {
            Line = context.Identifier().Symbol.Line,
            Column = context.Identifier().Symbol.Column,
            Length = context.Identifier().GetText().Length,
            Color = context.Var() == null ? Color.LocalValue : Color.LocalVariable
        });
    }

    public override void EnterMember_access_expression([NotNull] LoschScriptParser.Member_access_expressionContext context)
    {
        CurrentFile.Fragments.Add(new()
        {
            Line = context.Identifier().Symbol.Line,
            Column = context.Identifier().Symbol.Column,
            Length = context.Identifier().GetText().Length,
            Color = Color.Function
        });
    }

    public override void EnterFull_identifier_member_access_expression([NotNull] LoschScriptParser.Full_identifier_member_access_expressionContext context)
    {
        CurrentFile.Fragments.Add(new()
        {
            Line = context.full_identifier().Identifier().Last().Symbol.Line,
            Column = context.full_identifier().Identifier().Last().Symbol.Column,
            Length = context.full_identifier().Identifier().Last().GetText().Length,
            Color = Color.Function
        });
    }
}