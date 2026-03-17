using Antlr4.Runtime;
using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

using GeneratedMacroLexer = Dassie.Configuration.Macros.Parser.MacroLexer;
using GeneratedMacroParser = Dassie.Configuration.Macros.Parser.MacroParser;

namespace Dassie.Configuration.Macros;

internal partial class MacroParser2
{
    private const int MaxDepth = 100;

    private record MacroData(
        List<MacroParameter> Parameters,
        string Body);

    private readonly XDocument _inputDocument;
    private readonly string _path;
    private readonly Dictionary<string, MacroData> _definedMacros = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _constantMacros = new(StringComparer.Ordinal);
    private bool _hasErrors;

    public MacroParser2(XDocument doc, string symbolicName)
    {
        _inputDocument = doc;
        _path = symbolicName;

        AddDefaultMacros();
        ParseMacroDefinitions();
    }

    private void ParseMacroDefinitions()
    {
        XElement root = _inputDocument.Root;
        if (root == null)
            return;

        XElement macroDefs = root.Element("MacroDefinitions");
        if (macroDefs == null)
            return;

        IEnumerable<XElement> definitions = macroDefs.Elements("Define");
        foreach (XElement definition in definitions)
        {
            (string key, MacroData data) = ParseMacroDefinition(definition);

            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (!_definedMacros.TryAdd(key, data))
            {
                IXmlLineInfo lineInfo = definition;

                EmitWarningMessageFormatted(
                    lineInfo.LineNumber,
                    lineInfo.LinePosition,
                    definition.ToString().Length,
                    DS0272_DuplicateMacro,
                    $"Macro '{key}' is defined multiple times.", [],
                    _path);

                _definedMacros[key] = data;
            }
        }
    }

    private (string Name, MacroData Data) ParseMacroDefinition(XElement define)
    {
        IXmlLineInfo lineInfo = define;
        string name = "";
        bool trim = false;

        XAttribute macroAttribute = define.Attribute("Macro");
        if (macroAttribute == null)
        {
            EmitError(lineInfo, define.ToString().Length, DS0271_MissingMacroName, "Macro definition is missing required attribute 'Macro'.");
        }
        else
            name = macroAttribute.Value;

        if (define.Attribute("Trim") is XAttribute trimAttrib && bool.TryParse(trimAttrib.Value, out bool trimVal) && trimVal)
            trim = true;

        List<MacroParameter> paramList = [];
        XAttribute paramsAttribute = define.Attribute("Parameters");
        if (paramsAttribute != null)
        {
            IXmlLineInfo paramListLineInfo = paramsAttribute;
            string paramListStr = paramsAttribute.Value?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(paramListStr)
                && !(paramListStr.StartsWith('[') && paramListStr.EndsWith(']')))
            {
                EmitError(
                    paramListLineInfo,
                    paramsAttribute.ToString().Length,
                    DS0273_MacroSyntaxError,
                    "Syntax error. Macro parameter list must be enclosed by '[]'.");
            }

            paramListStr = paramListStr.Trim('[', ']');

            if (!string.IsNullOrWhiteSpace(paramListStr))
            {
                foreach (string rawParam in paramListStr.Split(','))
                {
                    string p = rawParam.Trim();

                    if (string.IsNullOrWhiteSpace(p))
                    {
                        EmitError(
                            paramListLineInfo,
                            paramsAttribute.ToString().Length,
                            DS0273_MacroSyntaxError,
                            "Syntax error. Macro parameter name cannot be empty.");

                        continue;
                    }

                    bool eager = p.StartsWith('!');
                    if (eager)
                        p = p[1..].Trim();

                    if (string.IsNullOrWhiteSpace(p))
                    {
                        EmitError(
                            paramListLineInfo,
                            paramsAttribute.ToString().Length,
                            DS0273_MacroSyntaxError,
                            "Syntax error. Macro parameter name cannot be empty.");

                        continue;
                    }

                    paramList.Add(new(p, eager));
                }
            }
        }

        string value = define.Value ?? "";
        if (trim)
            value = value.Trim();

        return new(name, new(paramList, value));
    }

    public bool Normalize()
    {
        if (_inputDocument.Root == null)
            return !_hasErrors;

        NormalizeElement(_inputDocument.Root, inMacroDefinitions: false);
        return !_hasErrors;
    }

    private void NormalizeElement(XElement element, bool inMacroDefinitions)
    {
        bool isMacroDefinitions = element.Name.LocalName == "MacroDefinitions";
        bool skipExpansion = inMacroDefinitions || isMacroDefinitions;

        if (!skipExpansion)
        {
            foreach (XAttribute attr in element.Attributes())
            {
                if (attr.IsNamespaceDeclaration)
                    continue;

                attr.Value = Expand(attr.Value, [], attr, 0, expandMacroCalls: true, strictParameterReferences: true);
            }

            foreach (XText text in element.Nodes().OfType<XText>())
                text.Value = Expand(text.Value, [], text, 0, expandMacroCalls: true, strictParameterReferences: true);
        }

        foreach (XElement child in element.Elements())
            NormalizeElement(child, skipExpansion);
    }

    private void AddDefaultMacros()
    {
#if STANDALONE
        string compilerPath = Environment.GetCommandLineArgs()[0];
#else
        string compilerPath = typeof(MacroParser2).Assembly.Location;
#endif

        string compilerDir = Path.GetDirectoryName(compilerPath) + Path.DirectorySeparatorChar;

        _constantMacros["Time"] = DateTime.Now.ToShortTimeString();
        _constantMacros["TimeExact"] = DateTime.Now.ToString("HH:mm:ss.ffff");
        _constantMacros["Date"] = DateTime.Now.ToShortDateString();
        _constantMacros["Year"] = DateTime.Now.Year.ToString();
        _constantMacros["CompilerDir"] = compilerDir;
        _constantMacros["CompilerDirectory"] = compilerDir;
        _constantMacros["CompilerPath"] = compilerPath;
    }

    private string Expand(
        string input,
        Dictionary<string, string> arguments,
        IXmlLineInfo lineInfo,
        int depth,
        bool expandMacroCalls,
        bool strictParameterReferences)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? "";

        if (!input.Contains("$(", StringComparison.Ordinal))
            return expandMacroCalls ? Unescape(input) : input;

        if (depth > MaxDepth)
        {
            EmitError(lineInfo, input.Length, DS0266_MacroRecursionLimitReached, "Macro expansion exceeded the maximum recursion depth.");
            return "";
        }

        GeneratedMacroLexer lexer = new(CharStreams.fromString(input));
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new MacroSyntaxErrorListener(this, lineInfo));

        GeneratedMacroParser parser = new(new CommonTokenStream(lexer));
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new MacroSyntaxErrorListener(this, lineInfo));

        GeneratedMacroParser.DocumentContext doc = parser.document();

        ExpansionVisitor visitor = new(
            this,
            lineInfo,
            arguments,
            depth,
            expandMacroCalls,
            strictParameterReferences);

        return visitor.VisitDocument(doc);
    }

    private string ExpandMacroCall(
        string macroName,
        List<string> rawArgs,
        Dictionary<string, string> currentArgs,
        IXmlLineInfo lineInfo,
        int depth)
    {
        if (depth > MaxDepth)
        {
            EmitError(lineInfo, macroName.Length, DS0266_MacroRecursionLimitReached, "Macro expansion exceeded the maximum recursion depth.");
            return "";
        }

        if (_constantMacros.TryGetValue(macroName, out string constantValue))
        {
            if (rawArgs.Count > 0)
                EmitError(lineInfo, macroName.Length, DS0273_MacroSyntaxError, $"Macro '{macroName}' does not accept arguments.");

            return constantValue;
        }

        if (_definedMacros.TryGetValue(macroName, out MacroData userMacro))
            return ExpandUserMacro(macroName, userMacro, rawArgs, currentArgs, lineInfo, depth);

        if (ExtensionLoader.Macros.Any(m => m.Name == macroName))
        {
            IMacro macro = ExtensionLoader.Macros.First(m => m.Name == macroName);
            return ExpandExtensionMacro(macro, rawArgs, currentArgs, lineInfo, depth);
        }

        EmitError(lineInfo, macroName.Length, DS0083_InvalidDSConfigMacro, $"Macro '{macroName}' is not defined.");
        return "";
    }

    private string ExpandUserMacro(
        string macroName,
        MacroData macro,
        List<string> rawArgs,
        Dictionary<string, string> currentArgs,
        IXmlLineInfo lineInfo,
        int depth)
    {
        if (rawArgs.Count != macro.Parameters.Count)
            EmitError(lineInfo, macroName.Length, DS0273_MacroSyntaxError, $"Macro '{macroName}' expects {macro.Parameters.Count} argument(s), but got {rawArgs.Count}.");

        Dictionary<string, string> localArgs = new(StringComparer.Ordinal);

        for (int i = 0; i < macro.Parameters.Count; i++)
        {
            MacroParameter parameter = macro.Parameters[i];
            string value = i < rawArgs.Count ? rawArgs[i] : "";

            value = parameter.IsEager
                ? Expand(value, currentArgs, lineInfo, depth + 1, expandMacroCalls: true, strictParameterReferences: true)
                : Expand(value, currentArgs, lineInfo, depth + 1, expandMacroCalls: false, strictParameterReferences: false);

            localArgs[parameter.Name] = value;
        }

        string body = ReplaceParameterReferences(macro.Body, localArgs, lineInfo, strict: true);
        return Expand(body, localArgs, lineInfo, depth + 1, expandMacroCalls: true, strictParameterReferences: false);
    }

    [GeneratedRegex(@"\$\(@([a-zA-Z_][a-zA-Z0-9_]*)\)")]
    private static partial Regex ParameterReferenceRegex();

    private string ReplaceParameterReferences(string input, Dictionary<string, string> arguments, IXmlLineInfo lineInfo, bool strict)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? "";

        return ParameterReferenceRegex().Replace(input, match =>
        {
            string parameterName = match.Groups[1].Value;

            if (arguments.TryGetValue(parameterName, out string value))
                return value;

            if (strict)
            {
                EmitError(lineInfo, parameterName.Length, DS0273_MacroSyntaxError, $"Macro parameter '{parameterName}' is not defined in this context.");
                return "";
            }

            return match.Value;
        });
    }

    private string ExpandExtensionMacro(
        IMacro macro,
        List<string> rawArgs,
        Dictionary<string, string> currentArgs,
        IXmlLineInfo lineInfo,
        int depth)
    {
        List<MacroParameter> parameters = macro.Parameters ?? [];

        if (rawArgs.Count != parameters.Count)
            EmitError(lineInfo, macro.Name.Length, DS0273_MacroSyntaxError, $"Macro '{macro.Name}' expects {parameters.Count} argument(s), but got {rawArgs.Count}.");

        Dictionary<string, string> args = new(StringComparer.Ordinal);

        for (int i = 0; i < parameters.Count; i++)
        {
            MacroParameter parameter = parameters[i];
            string value = i < rawArgs.Count ? rawArgs[i] : "";

            value = parameter.IsEager
                ? Expand(value, currentArgs, lineInfo, depth + 1, expandMacroCalls: true, strictParameterReferences: true)
                : Expand(value, currentArgs, lineInfo, depth + 1, expandMacroCalls: false, strictParameterReferences: false);

            args[parameter.Name] = value;
        }

        try
        {
            return macro.Expand(args) ?? "";
        }
        catch (Exception ex)
        {
            EmitError(lineInfo, macro.Name.Length, DS0273_MacroSyntaxError, $"Expansion of macro '{macro.Name}' failed: {ex.Message}");
            return "";
        }
    }

    private void EmitError(IXmlLineInfo lineInfo, int length, Dassie.Messages.MessageCode code, string message)
    {
        _hasErrors = true;
        EmitErrorMessageFormatted(
            lineInfo?.LineNumber ?? 0,
            lineInfo?.LinePosition ?? 0,
            Math.Max(0, length),
            code,
            message,
            [],
            _path);
    }

    private static string Unescape(string text)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains('^'))
            return text ?? "";

        Span<char> chars = text.ToCharArray();
        char[] buffer = new char[chars.Length];
        int n = 0;

        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == '^' && i + 1 < chars.Length)
            {
                buffer[n++] = chars[++i];
                continue;
            }

            buffer[n++] = chars[i];
        }

        return new(buffer, 0, n);
    }

    private sealed class MacroSyntaxErrorListener(MacroParser2 owner, IXmlLineInfo lineInfo) : BaseErrorListener, IAntlrErrorListener<int>
    {
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            owner.EmitError(
                lineInfo,
                offendingSymbol?.Text?.Length ?? 0,
                DS0273_MacroSyntaxError,
                $"Syntax error in macro expression: {msg}");
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            owner.EmitError(
                lineInfo,
                1,
                DS0273_MacroSyntaxError,
                $"Syntax error in macro expression: {msg}");
        }
    }
}
