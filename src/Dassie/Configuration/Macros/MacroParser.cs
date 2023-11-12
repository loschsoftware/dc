using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dassie.Configuration.Macros;

internal class MacroParser
{
    public MacroParser(bool addDefaults = true)
    {
        if (addDefaults)
            AddDefaultMacros();
    }

    private readonly Dictionary<string, string> _macros = new();

    public void DeclareMacro(string name, string expansion) => _macros.Add(name, expansion);

    public void AddDefaultMacros()
    {
        Dictionary<string, string> macros = new()
        {
            { "time", DateTime.Now.ToShortTimeString() },
            { "date", DateTime.Now.ToShortDateString() },
            { "year", DateTime.Now.Year.ToString() }
        };

        foreach (var macro in macros)
            DeclareMacro(macro.Key, macro.Value);
    }

    public void Normalize(DassieConfig config)
    {
        foreach (PropertyInfo prop in config.GetType().GetProperties().Where(p => p.PropertyType == typeof(string)))
        {
            string val = (string)prop.GetValue(config);

            if (val == null)
                continue;

            Regex macroRegex = new(@"\$\(.+\)");
            foreach (Match match in macroRegex.Matches(val))
            {
                if (!_macros.Any(k => k.Key == match.Value[2..^1].ToLowerInvariant()))
                {
                    EmitWarningMessage(
                        0, 0, 0,
                        DS0082_InvalidDSConfigMacro,
                        $"The macro '{match.Value[2..^1]}' does not exist and will be ignored.",
                        "dsconfig.xml");

                    val = val.Replace(match.Value, "");

                    break;
                }

                val = val.Replace(match.Value, _macros[match.Value[2..^1].ToLowerInvariant()]);
            }

            prop.SetValue(config, val);
        }
    }
}