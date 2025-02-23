using Antlr4.Runtime.Tree;
using Dassie.Parser;
using System.Linq;
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

        CurrentFile.FunctionParameterConstraints.Add(member.Identifier().GetIdentifier(), []);

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

        string typeName = "";
        if (member.type_name() != null)
            typeName = $"{member.Colon().Symbol.Text} {listener.GetTextForRule(member.type_name())}";

        StringBuilder attribsText = new();

        if (member.attribute() != null)
        {
            foreach (var attrib in member.attribute())
                attribsText.Append(listener.GetTextForRule(attrib));
        }

        sb.Append($"{attribsText.ToString()} {val}{var} {listener.GetTextForRule(member.member_access_modifier())} {listener.GetTextForRule(member.member_oop_modifier())} {specialModsText} {member.Identifier().Symbol.Text} {listener.GetTextForRule(member.generic_parameter_list())} {paramListBuilder.ToString()} {typeName}");
        sb.Append("= {");

        foreach (var param in member.parameter_list().parameter())
        {
            if (param.parameter_constraint() == null)
                continue;

            sb.Append($"? !({listener.GetTextForRule(param.parameter_constraint().expression())}) = {{");
            sb.Append($"\t$throw Dassie.Runtime.ConstraintViolationException \"{param.Identifier().Symbol.Text}\", \"{listener.GetTextForRule(param.parameter_constraint().expression())}\"");
            sb.Append("} : = {}");

            CurrentFile.FunctionParameterConstraints[member.Identifier().GetIdentifier()].Add(param.Identifier().GetIdentifier(), listener.GetTextForRule(param.parameter_constraint().expression()));
        }

        sb.Append(listener.GetTextForRule(member.expression()));
        sb.Append("}");

        return sb.ToString();
    }
}