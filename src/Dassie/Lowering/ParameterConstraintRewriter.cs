using Antlr4.Runtime.Tree;
using Dassie.Parser;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace Dassie.Lowering;

internal class ParameterConstraintRewriter : ITreeToStringRewriter
{
    private void AppendParameter(DassieParser.ParameterContext parameter, StringBuilder sb, LoweringListener listener)
    {
        string ddot = "";
        if (parameter.Double_Dot() != null)
            ddot = parameter.Double_Dot().Symbol.Text;

        string colon = "";
        if (parameter.Colon() != null)
            colon = parameter.Colon().Symbol.Text;

        string equals = "";
        if (parameter.Equals() != null)
            equals = parameter.Equals().Symbol.Text;
        sb.Append($"{listener.GetTextForRule(parameter.attribute())} {listener.GetTextForRule(parameter.parameter_modifier())} {parameter.Identifier().Symbol.Text} {ddot} {colon} {listener.GetTextForRule(parameter.type_name())} {equals} {listener.GetTextForRule(parameter.expression())}");
    }

    public string Rewrite(IParseTree tree, LoweringListener listener)
    {
        DassieParser.Type_memberContext member = (DassieParser.Type_memberContext)tree;

        if (member.parameter_list() == null)
            return listener.GetTextForRule(member);

        if (!member.parameter_list().parameter().Any(p => p.parameter_constraint() != null))
            return listener.GetTextForRule(member);

        CurrentFile.FunctionParameterConstraints.Add(member.Identifier().GetText(), []);

        string specialModsText = "";
        foreach (var mod in member.member_special_modifier())
            specialModsText += $" {listener.GetTextForRule(mod)}";

        StringBuilder paramListBuilder = new();

        if (member.parameter_list().Open_Paren() != null)
            paramListBuilder.Append(member.parameter_list().Open_Paren().Symbol.Text);

        foreach (var parameter in member.parameter_list().parameter()[..^1])
        {
            AppendParameter(parameter, paramListBuilder, listener);
            paramListBuilder.Append($", ");
        }

        AppendParameter(member.parameter_list().parameter().Last(), paramListBuilder, listener);

        if (member.parameter_list().Close_Paren() != null)
            paramListBuilder.Append(member.parameter_list().Close_Paren().Symbol.Text);

        StringBuilder sb = new();

        string val = "";
        if (member.Val() != null)
            val = member.Val().Symbol.Text;

        string var = "";
        if (member.Var() != null)
            var = member.Var().Symbol.Text;

        string ovr = "";
        if (member.Override() != null)
            ovr = member.Override().Symbol.Text;

        string typeName = "";
        if (member.type_name() != null)
            typeName = $"{member.Colon().Symbol.Text} {listener.GetTextForRule(member.type_name())}";

        sb.Append($"{listener.GetTextForRule(member.attribute())} {val}{var} {listener.GetTextForRule(member.member_access_modifier())} {listener.GetTextForRule(member.member_oop_modifier())} {specialModsText} {ovr} {member.Identifier().Symbol.Text} {listener.GetTextForRule(member.type_parameter_list())} {paramListBuilder.ToString()} {typeName}");
        sb.AppendLine("= {");

        foreach (var param in member.parameter_list().parameter())
        {
            if (param.parameter_constraint() == null)
                continue;

            sb.AppendLine($"? !({listener.GetTextForRule(param.parameter_constraint().expression())}) = {{");
            sb.AppendLine($"\tthrow Dassie.Runtime.ConstraintViolationException \"{param.Identifier().Symbol.Text}\", \"{listener.GetTextForRule(param.parameter_constraint().expression())}\"");
            sb.AppendLine("} : = {}");

            CurrentFile.FunctionParameterConstraints[member.Identifier().GetText()].Add(param.Identifier().GetText(), listener.GetTextForRule(param.parameter_constraint().expression()));
        }

        sb.AppendLine(listener.GetTextForRule(member.expression()));
        sb.AppendLine("}");

        return sb.ToString();
    }
}