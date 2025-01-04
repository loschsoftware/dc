using Antlr4.Runtime.Misc;
using Dassie.CodeAnalysis.Rules;
using Dassie.Configuration;
using Dassie.Errors;
using Dassie.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dassie.CodeAnalysis.Default;

internal class AnalyzingListener : DassieParserBaseListener
{
    public List<ErrorInfo> Messages { get; } = [];
    public AnalysisRuleSet RuleSet { get; }
    public CodeAnalysisConfiguration Configuration { get; }

    public AnalyzingListener(AnalysisRuleSet ruleSet = null, CodeAnalysisConfiguration config = null)
    {
        RuleSet = ruleSet ?? AnalysisRuleSet.Default;
        Configuration = config;
    }

    private bool _isInModule = false;

    public override void EnterLocal_declaration_or_assignment([NotNull] DassieParser.Local_declaration_or_assignmentContext context)
    {
        if (context.Var() != null && char.IsUpper(context.Identifier().GetText()[0]))
        {
            Messages.Add(ErrorHelper.GetError(
                AnalysisErrorKind.DS5001_NamingConvention,
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetText().Length,
                $"'{context.Identifier().GetText()}': Naming convention violation: Mutable local variables should use camelCase capitalization.",
                config: Configuration));
        }
    }

    public override void EnterType_member([NotNull] DassieParser.Type_memberContext context)
    {
        string id = context.Identifier()?.GetText();
        id ??= context.Custom_Operator().GetText();

        if (!_isInModule && context.parameter_list() != null && (context.member_special_modifier() == null || !context.member_special_modifier().Any(m => m.Static() != null)) && char.IsLower(id[0]))
        {
            Messages.Add(ErrorHelper.GetError(
                AnalysisErrorKind.DS5001_NamingConvention,
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetText().Length,
                $"'{context.Identifier().GetText()}': Naming convention violation: Instance methods should use PascalCase capitalization.",
                config: Configuration));
        }

        if (context.attribute().Any(a => a.type_name().GetText() == "EntryPoint") && id != "Main")
        {
            Messages.Add(ErrorHelper.GetError(
                AnalysisErrorKind.DS5002_EntryPointWrongName,
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetText().Length,
                $"'{context.Identifier().GetText()}': Application entry point should be called 'Main'.",
                config: Configuration));
        }
    }

    public override void EnterType([NotNull] DassieParser.TypeContext context)
    {
        if (context.type_kind().Module() != null)
            _isInModule = true;

        if (char.IsLower(context.Identifier().GetText()[0]))
        {
            Messages.Add(ErrorHelper.GetError(
                AnalysisErrorKind.DS5001_NamingConvention,
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetText().Length,
                $"'{context.Identifier().GetText()}': Naming convention violation: Type names should use PascalCase capitalization.",
                config: Configuration));
        }
    }

    public override void ExitType([NotNull] DassieParser.TypeContext context)
    {
        _isInModule = false;
    }

    public override void EnterParameter([NotNull] DassieParser.ParameterContext context)
    {
        if (context.Parent.Parent is DassieParser.TypeContext)
        {
            if (char.IsLower(context.Identifier().GetText()[0]))
            {
                Messages.Add(ErrorHelper.GetError(
                    AnalysisErrorKind.DS5001_NamingConvention,
                    context.Identifier().Symbol.Line,
                    context.Identifier().Symbol.Column,
                    context.Identifier().GetText().Length,
                    $"'{context.Identifier().GetText()}': Naming convention violation: Global properties should use PascalCase capitalization.",
                    config: Configuration));
            }

            return;
        }

        if (char.IsUpper(context.Identifier().GetText()[0]))
        {
            Messages.Add(ErrorHelper.GetError(
                AnalysisErrorKind.DS5001_NamingConvention,
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetText().Length,
                $"'{context.Identifier().GetText()}': Naming convention violation: Parameters should use camelCase capitalization.",
                config: Configuration));
        }
    }
}