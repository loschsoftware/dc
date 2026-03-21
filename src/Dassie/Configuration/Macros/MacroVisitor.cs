using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Dassie.Configuration.Macros.Parser;
using System.Text;

namespace Dassie.Configuration.Macros;

internal partial class MacroParser
{
    private sealed class MacroVisitor : MacroParserBaseVisitor<ExpansionResult>
    {
        private readonly MacroParser _owner;

        public MacroVisitor(MacroParser owner)
        {
            _owner = owner;
        }

        public override ExpansionResult VisitDocument([NotNull] Parser.MacroParser.DocumentContext context)
        {
            StringBuilder sb = new();
            bool canBeCached = true;

            foreach (IParseTree part in context.part())
            {
                ExpansionResult res = Visit(part);
                sb.Append(res.Result);
                canBeCached &= res.CanBeCached;
            }

            return new(sb.ToString(), canBeCached);
        }

        public override ExpansionResult VisitParam_ref([NotNull] Parser.MacroParser.Param_refContext context)
        {
            return base.VisitParam_ref(context);
        }

        public override ExpansionResult VisitLiteral([NotNull] Parser.MacroParser.LiteralContext context)
        {
            StringBuilder sb = new();

            foreach (IParseTree part in context.children)
            {
                string text = part.GetText();

                if (text.Length == 2 && text.StartsWith('^'))
                {
                    sb.Append(text[1]);
                    continue;
                }

                sb.Append(text);
            }

            return new(sb.ToString(), true);
        }

        public override ExpansionResult VisitMacro_call([NotNull] Parser.MacroParser.Macro_callContext context)
        {
            return base.VisitMacro_call(context);
        }
    }
}