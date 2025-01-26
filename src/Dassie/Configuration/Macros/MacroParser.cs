using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Dassie.Configuration.Macros;

internal class MacroParser
{
    public MacroParser(bool addDefaults = true)
    {
        if (addDefaults)
            AddDefaultMacros();
    }

    private Dictionary<string, string> _macros = [];

    public void ImportMacros(Dictionary<string, string> macros)
    {
        foreach (KeyValuePair<string, string> macro in macros)
            _macros.Add(macro.Key, macro.Value);
    }

    public void DeclareMacro(string name, string expansion) => _macros.Add(name, expansion);

    public void AddDefaultMacros()
    {
#if STANDALONE
        string compilerDir = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + Path.DirectorySeparatorChar;
        string compilerPath = Environment.GetCommandLineArgs()[0];
#else
        string compilerDir = Path.GetDirectoryName(typeof(MacroParser).Assembly.Location) + Path.DirectorySeparatorChar;
        string compilerPath = typeof(MacroParser).Assembly.Location;
#endif

        Dictionary<string, string> macros = new()
        {
            { "time", DateTime.Now.ToShortTimeString() },
            { "timeexact", DateTime.Now.ToString("HH:mm:ss.ffff") },
            { "date", DateTime.Now.ToShortDateString() },
            { "year", DateTime.Now.Year.ToString() },
            { "compilerdirectory", compilerDir },
            { "compilerpath", compilerPath }
        };

        foreach (var macro in macros)
            DeclareMacro(macro.Key, macro.Value);
    }

    public void Normalize(DassieConfig config)
    {
        Normalize((object)config);
    }

    private void Normalize(object obj)
    {
        if (obj == null)
            return;

        foreach (PropertyInfo prop in obj.GetType().GetProperties())
        {
            if (prop.PropertyType != typeof(string) && prop.PropertyType.IsClass && !prop.PropertyType.Namespace.StartsWith("System"))
            {
                if (prop.PropertyType.IsArray)
                {
                    object array = prop.GetValue(obj);

                    if (array == null)
                        continue;

                    foreach (object item in (Array)array)
                        Normalize(item);

                    continue;
                }

                Normalize(prop.GetValue(obj));
                continue;
            }

            if (prop.PropertyType != typeof(string))
                continue;

            string val = (string)prop.GetValue(obj);

            if (val == null)
                continue;

            Regex macroRegex = new(@"\$\(.+?\)");
            foreach (Match match in macroRegex.Matches(val))
            {
                if (!_macros.Any(k => k.Key == match.Value[2..^1].ToLowerInvariant()))
                {
                    EmitWarningMessage(
                        0, 0, 0,
                        DS0082_InvalidDSConfigMacro,
                        $"The macro '{match.Value[2..^1]}' does not exist and will be ignored.",
                        ProjectConfigurationFileName);

                    val = val.Replace(match.Value, "");

                    break;
                }

                val = val.Replace(match.Value, _macros[match.Value[2..^1].ToLowerInvariant()], StringComparison.InvariantCultureIgnoreCase);
            }

            prop.SetValue(obj, val);
        }
    }

    public string Normalize(string str)
    {
        StringReader sr = new(str);
        StringBuilder result = new();

        while (sr.Peek() != -1)
        {
            char c = (char)sr.Read();

            switch (c)
            {
                case '$':
                    if ((char)sr.Peek() == '(')
                    {
                        sr.Read();
                        StringBuilder macroNameBuilder = new();
                        while ((char)sr.Peek() != ')')
                            macroNameBuilder.Append((char)sr.Read());

                        if (_macros.TryGetValue(macroNameBuilder.ToString(), out string value))
                            result.Append(value);

                        sr.Read();
                        break;
                    }

                    goto default;

                default:
                    result.Append(c);
                    break;
            }
        }

        return result.ToString();
    }
}