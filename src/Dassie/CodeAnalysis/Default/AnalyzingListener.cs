using Antlr4.Runtime.Misc;
using Dassie.CodeAnalysis.Rules;
using Dassie.Configuration;
using Dassie.Messages;
using Dassie.Parser;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dassie.CodeAnalysis.Default;

internal class AnalyzingListener : DassieParserBaseListener
{
    public List<MessageInfo> Messages { get; } = [];
    public AnalysisRuleSet RuleSet { get; }
    public CodeAnalysisConfiguration Configuration { get; }

    public AnalyzingListener(AnalysisRuleSet ruleSet = null, CodeAnalysisConfiguration config = null)
    {
        RuleSet = ruleSet ?? AnalysisRuleSet.Default;
        Configuration = config;
    }

    private string _exportedNamespace = null;
    private bool _isInModule = false;

    public override void EnterExport_directive([NotNull] DassieParser.Export_directiveContext context)
    {
        _exportedNamespace = context.full_identifier().GetText();
    }

    public override void EnterLocal_declaration_or_assignment([NotNull] DassieParser.Local_declaration_or_assignmentContext context)
    {
        if (context.Var() != null && char.IsUpper(context.Identifier().GetIdentifier()[0]))
        {
            Messages.Add(ErrorHelper.GetError(
                AnalysisErrorKind.DS2001_NamingConvention,
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetIdentifier().Length,
                string.Format(StringHelper.AnalyzingListener_NamingConventionViolationMutableLocalVariable, context.Identifier().GetIdentifier()),
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
                AnalysisErrorKind.DS2001_NamingConvention,
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetIdentifier().Length,
                string.Format(StringHelper.AnalyzingListener_NamingConventionViolationInstanceMethod, context.Identifier().GetIdentifier()),
                config: Configuration));
        }

        if (context.attribute().Any(a => a.type_name().GetText() == "EntryPoint") && id != "Main")
        {
            Messages.Add(ErrorHelper.GetError(
                AnalysisErrorKind.DS2002_EntryPointWrongName,
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetIdentifier().Length,
                string.Format(StringHelper.AnalyzingListener_EntryPointName, context.Identifier().GetText()),
                config: Configuration));
        }
    }

    public override void EnterType([NotNull] DassieParser.TypeContext context)
    {
        if (context.type_kind().Module() != null)
            _isInModule = true;

        if (char.IsLower(context.Identifier().GetIdentifier()[0]))
        {
            Messages.Add(ErrorHelper.GetError(
                AnalysisErrorKind.DS2001_NamingConvention,
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetIdentifier().Length,
                string.Format(StringHelper.AnalyzingListener_NamingConventionViolationType, context.Identifier().GetIdentifier()),
                config: Configuration));
        }

        TypeAttributes attribs = AttributeHelpers.GetTypeAttributes(
            context.type_kind(),
            context.type_access_modifier(),
            context.nested_type_access_modifier(),
            context.type_special_modifier(),
            false,
            context.type_block() != null && context.type_block().type_name() != null);

        if (string.IsNullOrEmpty(_exportedNamespace) && attribs.HasFlag(TypeAttributes.Public))
        {
            Messages.Add(ErrorHelper.GetError(
                AnalysisErrorKind.DS2003_TypeOutsideNamespace,
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetIdentifier().Length,
                string.Format(StringHelper.AnalyzingListener_MissingNamespace, context.Identifier().GetIdentifier()),
                severity: Severity.Warning,
                tip: StringHelper.AnalyzingListener_MissingNamespaceTip,
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
            if (char.IsLower(context.Identifier().GetIdentifier()[0]))
            {
                Messages.Add(ErrorHelper.GetError(
                    AnalysisErrorKind.DS2001_NamingConvention,
                    context.Identifier().Symbol.Line,
                    context.Identifier().Symbol.Column,
                    context.Identifier().GetIdentifier().Length,
                    string.Format(StringHelper.AnalyzingListener_NamingConventionViolationGlobalProperty, context.Identifier().GetIdentifier()),
                    config: Configuration));
            }

            return;
        }

        if (char.IsUpper(context.Identifier().GetIdentifier()[0]))
        {
            Messages.Add(ErrorHelper.GetError(
                AnalysisErrorKind.DS2001_NamingConvention,
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetIdentifier().Length,
                string.Format(StringHelper.AnalyzingListener_NamingConventionViolationParameter, context.Identifier().GetIdentifier()),
                config: Configuration));
        }
    }
}