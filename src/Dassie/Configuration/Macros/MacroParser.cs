using Dassie.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dassie.Configuration.Macros;

internal partial class MacroParser
{
    public MacroParser(bool addDefaults = true)
    {
        if (addDefaults)
            AddDefaultMacros();
    }

    [GeneratedRegex(@"\$\([^$\)\(\r\n]+?\)")]
    private static partial Regex MacroRegex();

    private static readonly int _maxDepth = 100;
    private readonly Dictionary<string, string> _macros = [];

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
            { "Time", DateTime.Now.ToShortTimeString() },
            { "TimeExact", DateTime.Now.ToString("HH:mm:ss.ffff") },
            { "Date", DateTime.Now.ToShortDateString() },
            { "Year", DateTime.Now.Year.ToString() },
            { "CompilerDir", compilerDir },
            { "CompilerPath", compilerPath }
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

            if (prop.PropertyType.Name == "List`1")
            {
                object list = prop.GetValue(obj);

                if (list == null)
                    continue;

                foreach (object item in (IList)list)
                    Normalize(item);

                continue;
            }

            if (prop.PropertyType != typeof(string))
                continue;

            string val;

            try
            {
                val = (string)prop.GetValue(obj);
            }
            catch (ArgumentException)
            {
                continue;
            }

            if (val == null)
                continue;

            val = Normalize(val);

            if (prop.SetMethod != null)
                prop.SetValue(obj, val);
        }
    }

    public string Normalize(string str)
    {
        string result = str;
        string previous;
        int depth = 0;
        Regex macroRegex = MacroRegex();

        do
        {
            string macroName = "";
            previous = result;
            result = macroRegex.Replace(result, match =>
            {
                macroName = match.Value[2..^1];

                if (_macros.TryGetValue(macroName, out string value))
                    return value;

                if (ExtensionLoader.Macros.Any(m => m.Macro == macroName))
                    return ExtensionLoader.Macros.First(m => m.Macro == macroName).Expand();

                EmitErrorMessage(
                    0, 0, 0,
                    DS0083_InvalidDSConfigMacro,
                    $"Failed to expand macro '{macroName}': Macro is not defined.",
                    ProjectConfigurationFileName);

                return "";
            });

            if (depth++ >= _maxDepth)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0266_MacroRecursionLimitReached,
                    $"Failed to expand macro '{macroName}': Maximum recursion depth reached.",
                    ProjectConfigurationFileName);

                return "";
            }
        }
        while (result != previous && macroRegex.IsMatch(result));

        return result;
    }
}