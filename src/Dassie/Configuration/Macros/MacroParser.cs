using Dassie.Extensions;
using System;
using System.Collections.Generic;

namespace Dassie.Configuration.Macros;

internal partial class MacroParser
{
    private record ExpansionResult(string Result, bool CanBeCached);

    private Func<string, object> _propertyResolver = static _ => null;
    private readonly Dictionary<string, string> _cachedMacros = [];
    private readonly List<IMacro> _additionalMacros = [];

    public void AddMacro(IMacro macro) => _additionalMacros.Add(macro);
    public void AddMacros(IEnumerable<IMacro> macros) => _additionalMacros.AddRange(macros);

    public void BindPropertyResolver(Func<string, object> resolver)
    {
        _propertyResolver = resolver;
    }

    public (string Value, bool CanBeCached) Expand(string input)
    {
        if (string.IsNullOrEmpty(input))
            return (input ?? "", true);

        if (!input.Contains('$') && !input.Contains('^'))
            return (input, true);

        // TODO
        return (input, false);
    }
}