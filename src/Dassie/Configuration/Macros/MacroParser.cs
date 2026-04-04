using Antlr4.Runtime;
using Dassie.Configuration.Macros.Parser;
using Dassie.Core.Macros;
using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dassie.Configuration.Macros;

internal partial class MacroParser
{
    private class AdHocMacro(string name, MacroOptions options, Func<string> expansion) : IMacro
    {
        public string Name => name;
        public List<MacroParameter> Parameters => [];
        public MacroOptions Options => options;
        public string Expand(Dictionary<string, string> arguments) => expansion();
    }

    private record ExpansionResult(string Result, bool CanBeCached);

    private int _macroDefinitionResolutionDepth;
    private List<Define> _macroDefinitions = [];
    private Func<string, object> _propertyResolver = static _ => null;
    private readonly Dictionary<string, string> _cachedMacros = [];
    private readonly List<IMacro> _additionalMacros = [];

    public MacroParser()
    {
        PopulateIntrinsicMacros();
    }

    public MacroParser(DassieConfig config) : this()
    {
        BindPropertyResolver(p => config[p]);
        AddMacro(new PropMacro(config));
    }

    public void SetMacroDefinitions(IEnumerable<Define> definitions)
    {
        _macroDefinitions = definitions?
            .Where(d => d != null)
            .ToList() ?? [];
    }

    private void PopulateIntrinsicMacros()
    {
#if STANDALONE
        string compilerPath = Environment.GetCommandLineArgs()[0];
        string compilerDir = Path.GetDirectoryName(compilerPath) + Path.DirectorySeparatorChar;
#else
        string compilerPath = typeof(MacroParser).Assembly.Location;
        string compilerDir = Path.GetDirectoryName(compilerPath) + Path.DirectorySeparatorChar;
#endif

        List<AdHocMacro> macros =
        [
            // General
            new("Time", MacroOptions.None, () => DateTime.Now.ToShortTimeString()),
            new("TimeExact", MacroOptions.None, () => DateTime.Now.ToString("HH:mm:ss.ffff")),
            new("Date", MacroOptions.None, () => DateTime.Now.ToShortDateString()),
            // Probably good to cache this, might be unexpected for the year to change midway through a build
            new("Year", MacroOptions.AllowCaching, () => DateTime.Now.Year.ToString()),
            new("CompilerDir", MacroOptions.AllowCaching, () => compilerDir),
            new("CompilerPath", MacroOptions.AllowCaching, () => compilerPath),
            new("Guid", MacroOptions.AllowCaching, () => Guid.NewGuid().ToString()),
            new("NewGuid", MacroOptions.None, () => Guid.NewGuid().ToString()),
            
            // Project-specific
            new("ProjectFilePath", MacroOptions.AllowCaching, () => Path.GetFullPath(ProjectConfigurationFileName)),
            new("ProjectName", MacroOptions.AllowCaching, () => Path.GetDirectoryName(Path.GetFullPath(ProjectConfigurationFileName)).Split(Path.DirectorySeparatorChar).Last()),
            new("ProjectDir", MacroOptions.AllowCaching, () => Path.GetDirectoryName(Path.GetFullPath(ProjectConfigurationFileName)) + Path.DirectorySeparatorChar),
            new("OutputDir", MacroOptions.AllowCaching, () => Path.GetFullPath(_propertyResolver("BuildDirectory")?.ToString() ?? Directory.GetCurrentDirectory()) + Path.DirectorySeparatorChar),
            new("TargetPath", MacroOptions.AllowCaching, () => Path.GetFullPath(Path.Combine(_propertyResolver("BuildDirectory").ToString(), $"{_propertyResolver("AssemblyFileName")}.dll")))
        ];

        _additionalMacros.AddRange(macros);
    }

    private IMacro GetRuntimeMacro(string name)
    {
        return ExtensionLoader.Macros.FirstOrDefault(m => m.Name == name)
            ?? _additionalMacros.FirstOrDefault(m => m.Name == name);
    }

    public void AddMacro(IMacro macro) => _additionalMacros.Add(macro);
    public void AddMacros(IEnumerable<IMacro> macros) => _additionalMacros.AddRange(macros);

    public void BindPropertyResolver(Func<string, object> resolver)
    {
        _propertyResolver = resolver;
    }

    public (string Value, bool CanBeCached) Expand(string input)
    {
        ExpansionResult result = Expand(input, [], []);
        return (result.Result, result.CanBeCached);
    }

    private ExpansionResult Expand(string input, Dictionary<string, string> paramScope, HashSet<string> stack)
    {
        if (string.IsNullOrEmpty(input))
            return new(input ?? "", true);

        if (!input.Contains('$') && !input.Contains('^'))
            return new(input, true);

        ICharStream charStream = CharStreams.fromString(input);
        MacroLexer lexer = new(charStream);
        CommonTokenStream tokenStream = new(lexer);
        Parser.MacroParser parser = new(tokenStream);
        MacroVisitor visitor = new(this, paramScope, stack);

        return visitor.Visit(parser.document());
    }

    private bool TryExpandDefinedMacro(
        string macroName,
        List<string> argumentTexts,
        Dictionary<string, string> outerScope,
        HashSet<string> stack,
        out ExpansionResult result)
    {
        result = default;

        IEnumerable<Define> definitions = GetMacroDefinitions();
        Define definition = definitions.FirstOrDefault(d => d?.Macro == macroName);

        if (definition == null)
            return false;

        List<MacroParameter> parameters = ParseDefineParameters(definition.Parameters);
        Dictionary<string, string> localScope = new(StringComparer.Ordinal);
        bool canBeCached = true;

        for (int i = 0; i < parameters.Count; i++)
        {
            MacroParameter p = parameters[i];
            string argText = i < argumentTexts.Count ? argumentTexts[i] : string.Empty;

            if (p.IsEager)
            {
                ExpansionResult expandedArg = Expand(argText, outerScope, stack);
                localScope[p.Name] = expandedArg.Result;
                canBeCached &= expandedArg.CanBeCached;
            }
            else
            {
                localScope[p.Name] = argText;
            }
        }

        string body = definition.Value ?? string.Empty;
        ExpansionResult expandedBody = Expand(body, localScope, stack);
        canBeCached &= expandedBody.CanBeCached;

        string value = definition.Trim ? expandedBody.Result.Trim() : expandedBody.Result;
        result = new(value, canBeCached);
        return true;
    }

    private IEnumerable<Define> GetMacroDefinitions()
    {
        if (_macroDefinitions.Count > 0)
            return _macroDefinitions;

        if (_macroDefinitionResolutionDepth > 0)
            return [];

        _macroDefinitionResolutionDepth++;

        try
        {
            object raw = _propertyResolver(nameof(DassieConfig.MacroDefinitions));

            return raw switch
            {
                Define[] arr => arr,
                IEnumerable<Define> seq => seq,
                _ => []
            };
        }
        finally
        {
            _macroDefinitionResolutionDepth--;
        }
    }

    private static List<MacroParameter> ParseDefineParameters(string paramList)
    {
        paramList = paramList?.TrimStart('[').TrimEnd(']');

        if (string.IsNullOrWhiteSpace(paramList))
            return [];

        List<MacroParameter> parsed = [];

        foreach (string token in paramList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string t = token.Trim();
            bool eager = false;

            if (t.StartsWith('!'))
            {
                eager = true;
                t = t[1..].Trim();
            }

            if (t.Length > 0)
                parsed.Add(new(t, eager));
        }

        return parsed;
    }
}