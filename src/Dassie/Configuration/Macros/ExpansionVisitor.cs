using Dassie.Configuration.Macros.Parser;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using GeneratedMacroParser = Dassie.Configuration.Macros.Parser.MacroParser;

namespace Dassie.Configuration.Macros;

internal partial class MacroParser2
{
    private sealed class ExpansionVisitor(
        MacroParser2 owner,
        IXmlLineInfo lineInfo,
        Dictionary<string, string> arguments,
        int depth,
        bool expandMacroCalls,
        bool strictParameterReferences) : MacroParserBaseVisitor<string>
    {
        public override string VisitDocument(GeneratedMacroParser.DocumentContext context)
            => string.Concat(context.part().Select(VisitPart));

        public override string VisitPart(GeneratedMacroParser.PartContext context)
        {
            if (context.literal() != null)
                return VisitLiteral(context.literal());

            if (context.param_ref() != null)
                return VisitParam_ref(context.param_ref());

            if (context.macro_call() != null)
                return VisitMacro_call(context.macro_call());

            return "";
        }

        public override string VisitLiteral(GeneratedMacroParser.LiteralContext context)
        {
            if (context.children == null || context.children.Count == 0)
                return "";

            return string.Concat(context.children
                .OfType<ITerminalNode>()
                .Select(t => t.Symbol.Type == GeneratedMacroParser.Text
                    ? (expandMacroCalls ? Unescape(t.GetText()) : t.GetText())
                    : t.GetText()));
        }

        public override string VisitArgument(GeneratedMacroParser.ArgumentContext context)
            => string.Concat(context.part().Select(VisitPart));

        public override string VisitParam_ref(GeneratedMacroParser.Param_refContext context)
        {
            string parameterName = context.Identifier().GetText();

            if (arguments.TryGetValue(parameterName, out string value))
                return value;

            if (!strictParameterReferences)
                return $"$(@{parameterName})";

            owner.EmitError(lineInfo, parameterName.Length, DS0273_MacroSyntaxError, $"Macro parameter '{parameterName}' is not defined in this context.");
            return "";
        }

        public override string VisitMacro_call(GeneratedMacroParser.Macro_callContext context)
        {
            string macroName = context.Identifier().GetText();
            List<string> args = context.arglist()?.argument().Select(VisitArgument).ToList() ?? [];

            if (!expandMacroCalls)
                return BuildMacroText(macroName, args);

            return owner.ExpandMacroCall(macroName, args, arguments, lineInfo, depth + 1);
        }

        private static string BuildMacroText(string macroName, List<string> args)
        {
            if (args.Count == 0)
                return $"$({macroName})";

            return $"$({macroName}: {string.Join(", ", args)})";
        }
    }
}
