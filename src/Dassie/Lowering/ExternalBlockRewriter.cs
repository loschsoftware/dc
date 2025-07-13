using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Parser;
using System;
using System.Linq;
using System.Text;

namespace Dassie.Lowering;

internal class ExternalBlockRewriter : ITreeToStringRewriter
{
    public string Rewrite(IParseTree tree, LoweringListener listener)
    {
        DassieParser.External_blockContext rule = (DassieParser.External_blockContext)tree;
        if (rule.type_member() == null || rule.type_member().Length == 0)
            return listener.GetTextForRule(rule);

        StringBuilder sb = new();

        foreach (DassieParser.Type_memberContext member in rule.type_member())
        {
            if (member.external_block() != null)
            {
                EmitErrorMessage(
                    member.Start.Line,
                    member.Start.Column,
                    member.GetText().Length,
                    DS0214_NestedExternalBlock,
                    $"'extern' blocks cannot be nested.");

                continue;
            }

            sb.AppendLine($"<System.Runtime.InteropServices.DllImport {rule.String_Literal()}>");
            sb.Append($"{string.Join(Environment.NewLine, (member.attribute() ?? []).Select(listener.GetTextForRule))} ");
            sb.Append($"{listener.GetTextForRule(member.member_access_modifier())} ");
            sb.Append($"{listener.GetTextForRule(member.member_oop_modifier())} ");
            sb.Append($"{string.Join(' ', (member.member_special_modifier() ?? []).Select(listener.GetTextForRule))} ");

            if (!(member.member_special_modifier() ?? []).Any(m => m.Extern() != null))
                sb.Append("extern ");

            int startIndex = 0;
            if (member.Val() != null) startIndex = member.Val().Symbol.StartIndex;
            else if (member.Var() != null) startIndex = member.Var().Symbol.StartIndex;
            else if (member.Open_Paren() != null) startIndex = member.Open_Paren().Symbol.StartIndex;
            else if (member.Custom_Operator() != null) startIndex = member.Custom_Operator().Symbol.StartIndex;
            else startIndex = member.Identifier().Symbol.StartIndex;
            
            sb.AppendLine(listener.CharStream.GetText(new(startIndex, member.Stop.StopIndex)));
        }

        return sb.ToString();
    }
}