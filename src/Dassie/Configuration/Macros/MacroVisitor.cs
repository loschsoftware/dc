using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Dassie.Configuration.Macros.Parser;
using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dassie.Configuration.Macros;

internal partial class MacroParser
{
    private sealed class MacroVisitor : MacroParserBaseVisitor<ExpansionResult>
    {
        private readonly MacroParser _owner;
        private readonly Dictionary<string, string> _paramScope;
        private readonly HashSet<string> _stack;

        public MacroVisitor(MacroParser owner, Dictionary<string, string> paramScope, HashSet<string> stack)
        {
            _owner = owner;
            _paramScope = paramScope;
            _stack = stack;
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

        public override ExpansionResult VisitParam_ref([NotNull] Parser.MacroParser.Param_refContext context)
        {
            string paramName = context.Identifier().GetText();

            if (!_paramScope.TryGetValue(paramName, out string value))
                return new(context.GetText(), false);

            return _owner.Expand(value, _paramScope, _stack);
        }

        public override ExpansionResult VisitMacro_call([NotNull] Parser.MacroParser.Macro_callContext context)
        {
            string macroName = context.Identifier().GetText();
            string invocationKey = context.GetText();

            if (_owner._cachedMacros.TryGetValue(invocationKey, out string cachedValue))
                return new(cachedValue, true);

            if (_stack.Contains(invocationKey))
                return new(invocationKey, false);

            _stack.Add(invocationKey);

            try
            {
                List<string> argumentTexts = context.arglist()?.argument().Select(a => a.GetText()).ToList() ?? [];

                if (_owner.TryExpandDefinedMacro(macroName, argumentTexts, _paramScope, _stack, out ExpansionResult defined))
                    return defined;

                IMacro runtimeMacro = _owner.GetRuntimeMacro(macroName);

                if (runtimeMacro != null)
                {
                    Dictionary<string, string> args = new(StringComparer.Ordinal);
                    bool canBeCached = true;

                    for (int i = 0; i < runtimeMacro.Parameters.Count; i++)
                    {
                        MacroParameter p = runtimeMacro.Parameters[i];
                        string rawArg = i < argumentTexts.Count ? argumentTexts[i] : string.Empty;

                        if (p.IsEager)
                        {
                            ExpansionResult eagerValue = _owner.Expand(rawArg, _paramScope, _stack);
                            args[p.Name] = eagerValue.Result;
                            canBeCached &= eagerValue.CanBeCached;
                        }
                        else
                        {
                            args[p.Name] = rawArg;
                        }
                    }

                    string expanded = "";

                    try
                    {
                        expanded = runtimeMacro.Expand(args) ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        EmitErrorMessageFormatted(
                            0, 0, 0,
                            DS0083_InvalidDSConfigMacro,
                            nameof(StringHelper.MacroParser_MacroThrewException), [invocationKey, ex.ToString()],
                            ProjectConfigurationFileName);
                    }

                    ExpansionResult nested = _owner.Expand(expanded, _paramScope, _stack);
                    canBeCached &= nested.CanBeCached;
                    canBeCached &= runtimeMacro.Options.HasFlag(MacroOptions.AllowCaching);

                    if (canBeCached)
                        _owner._cachedMacros[invocationKey] = nested.Result;

                    return new(nested.Result, canBeCached);
                }

                EmitWarningMessageFormatted(
                    0, 0, 0,
                    DS0083_InvalidDSConfigMacro,
                    nameof(StringHelper.MacroParser_MacroNotDefined), [invocationKey],
                    ProjectConfigurationFileName);

                return new(invocationKey, false);
            }
            finally
            {
                _stack.Remove(invocationKey);
            }
        }
    }
}