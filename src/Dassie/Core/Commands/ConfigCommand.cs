using Dassie.Configuration;
using Dassie.Configuration.Global;
using Dassie.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Dassie.Core.Commands;

internal class ConfigCommand : CompilerCommand
{
    private static ConfigCommand _instance;
    public static ConfigCommand Instance => _instance ??= new();

    public override string Command => "config";

    public override string Description => StringHelper.ConfigCommand_Description;

    public override CommandHelpDetails HelpDetails => new()
    {
        Description = Description,
        Usage =
        [
            "dc config [<Property>=[Value]]...",
            "dc config --global [--reset] [--import <Path>] [<Property>=[Value]]...",
            "dc config --macros [--no-predefined]"
        ],
        Options =
        [
            ("Property=[Value]", StringHelper.ConfigCommand_PropertyOption),
            ("--global", StringHelper.ConfigCommand_GlobalOption),
            ("    --reset", StringHelper.ConfigCommand_ResetOption),
            ("    --import <Path>", StringHelper.ConfigCommand_ImportOption),
            ("--macros", StringHelper.ConfigCommand_MacrosOption),
            ("    --no-predefined", StringHelper.ConfigCommand_NoPredefinedOption),
        ],
        Remarks = StringHelper.ConfigCommand_Remarks,
        Examples =
        [
            ("dc config", StringHelper.ConfigCommand_Example1),
            ("dc config MeasureElapsedTime=true Verbosity=2", StringHelper.ConfigCommand_Example2),
            ("dc config --global", StringHelper.ConfigCommand_Example3),
            ("dc config --global core.scratchpad.editor=vim", StringHelper.ConfigCommand_Example4)
        ]
    };

    public override int Invoke(string[] args)
    {
        Dictionary<string, string> properties = [];
        Dictionary<string, string> macros = [];

        if (args != null)
        {
            foreach (string arg in args.Where(a => a.Contains('=')))
            {
                string[] parts = arg.Split('=');
                string key = parts[0];
                string value = string.Join('=', parts[1..]);

                if (key.StartsWith("$(") && key.EndsWith(')'))
                    macros.Add(key[2..^1], value);
                else
                    properties.TryAdd(key, value);
            }
        }

        foreach (string arg in args.Where(a => !a.StartsWith('-') && !a.Contains('=')))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0013_InvalidArgument,
                nameof(StringHelper.ConfigCommand_UnexpectedArgument), [arg],
                CompilerExecutableName);
        }

        if (args.Contains("--macros"))
        {
            if (!File.Exists(ProjectConfigurationFileName))
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0270_DCConfigMacrosOptionNoProjectFile,
                    nameof(StringHelper.ConfigCommand_MacrosOptionNoProjectFile), [],
                    CompilerExecutableName);

                return -1;
            }

            _ = ProjectFileSerializer.DassieConfig;
            Dictionary<string, string> definedMacros = ProjectFileSerializer.MacroDefinitions.Select(d => new KeyValuePair<string, string>(d.Name, d.Value)).ToDictionary();
            PrintProperties(definedMacros.Select(k => (k.Key, "", k.Value)).OrderBy(k => k.Key).ToList(), true);
            return 0;
        }

        if (args.Contains("--global"))
        {
            if (args.Contains("--reset"))
            {
                File.Delete(GlobalConfigManager.ConfigPath);
                GlobalConfigManager.Initialize();
                return 0;
            }

            if (args.Contains("--import"))
            {
                if (args.Length <= args.IndexOf("--import") + 1)
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0013_InvalidArgument,
                        nameof(StringHelper.ConfigCommand_ImportFilePathRequired), [],
                        CompilerExecutableName);

                    return -1;
                }

                string path = args[args.IndexOf("--import") + 1];
                if (!File.Exists(path))
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0198_ImportedConfigFileNotFound,
                        nameof(StringHelper.ConfigCommand_FileNotFound), [path],
                        CompilerExecutableName);

                    return -1;
                }

                File.Copy(path, GlobalConfigManager.ConfigPath, true);
                GlobalConfigManager.Initialize();
                return 0;
            }

            if (properties.Count == 0)
            {
                PrintProperties(GlobalConfigManager.Properties.Select(p => (p.Key, GlobalConfigManager.TypeName(p.Value.Type), GlobalConfigManager.Format(p.Value.Value, p.Value.Type))).ToList());
                return 0;
            }

            if (macros.Count >= 1)
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0269_DCConfigMacroForGlobalConfig,
                    nameof(StringHelper.ConfigCommand_MacroForGlobalConfig), [],
                    CompilerExecutableName);
            }

            foreach (KeyValuePair<string, string> prop in properties)
            {
                if (!GlobalConfigManager.Properties.Any(p => p.Key.Equals(prop.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0253_DCConfigInvalidProperty,
                        nameof(StringHelper.ConfigCommand_InvalidGlobalProperty), [prop.Key],
                        CompilerExecutableName);

                    continue;
                }

                (GlobalConfigDataType Type, object Value) value = GlobalConfigManager.Properties.First(p => p.Key.Equals(prop.Key, StringComparison.OrdinalIgnoreCase)).Value;
                object val = GlobalConfigManager.GetValue(prop.Value, value.Type, out bool error);

                if (!error)
                    GlobalConfigManager.Set(prop.Key, value.Type, val);
            }

            return 0;
        }

        if (File.Exists(ProjectConfigurationFileName))
        {
            DassieConfig config = ProjectFileSerializer.DassieConfig;

            if (properties.Count == 0 && macros.Count == 0)
            {
                List<(string Key, string Type, string Value)> props = [];

                foreach (Property prop in config.Store.PropertyScope)
                {
                    if (prop.Name == nameof(DassieConfig.MacroDefinitions))
                        continue;

                    object defaultVal = prop.Default;
                    object val = config[prop.Name];

                    bool IsEqual()
                    {
                        if (val == null && prop.Type == typeof(string) && (string)defaultVal == "")
                            return true;

                        if (val == null)
                            return val == defaultVal;

                        if (val is string s && s == "" && prop.Type == typeof(string) && defaultVal == null)
                            return true;

                        return val.Equals(defaultVal);
                    }

                    if (!IsEqual())
                        props.Add((prop.Name, HelpCommand.GetPropertyTypeName(prop.Type), FormatObject(val)));
                }

                PrintProperties(props);
                return 0;
            }

            XDocument doc = XDocument.Load(ProjectConfigurationFileName, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            IEnumerable<Property> dsconfigProps = config.Store.Properties;

            if (macros.Count >= 1)
                config.MacroDefinitions ??= [];

            foreach (KeyValuePair<string, string> macro in macros)
            {
                XElement macroDefs = doc.Root.Element("MacroDefinitions");
                if (macroDefs == null)
                {
                    macroDefs = new("MacroDefinitions");
                    doc.Root.AddFirst(macroDefs);
                }

                XElement macroElement = macroDefs.Elements("Define")
                         .FirstOrDefault(e => (string)e.Attribute("Macro") == macro.Key);

                if (macroElement != null)
                {
                    if (string.IsNullOrEmpty(macro.Value))
                        macroElement.Remove();
                    else
                        macroElement.Value = macro.Value;
                }
                else
                    macroDefs.Add(new XElement("Define", new XAttribute("Macro", macro.Key), macro.Value));
            }

            foreach (KeyValuePair<string, string> prop in properties)
            {
                Property dsconfigProperty = null;

                if (dsconfigProps.Any(p => p.Name.Equals(prop.Key, StringComparison.OrdinalIgnoreCase)))
                    dsconfigProperty = dsconfigProps.First(p => p.Name.Equals(prop.Key, StringComparison.OrdinalIgnoreCase));

                dsconfigProperty ??= new(prop.Key, typeof(string), "");

                Type propertyType = dsconfigProperty.Type;
                object defaultVal = dsconfigProperty.Default;

                if (propertyType != typeof(string) && !propertyType.IsPrimitive && !propertyType.IsEnum)
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0254_DCConfigUnsupportedDataType,
                        nameof(StringHelper.ConfigCommand_UnsupportedPropertyType), [dsconfigProperty.Name, TypeHelpers.TypeName(propertyType)],
                        CompilerExecutableName);

                    continue;
                }

                string format = prop.Value;

                if (string.IsNullOrEmpty(format))
                    format = defaultVal?.ToString() ?? "";

                if (propertyType == typeof(bool))
                {
                    if (!bool.TryParse(format.ToLowerInvariant(), out _))
                    {
                        EmitErrorMessageFormatted(
                            0, 0, 0,
                            DS0255_DCConfigInvalidValue,
                            nameof(StringHelper.ConfigCommand_InvalidValueForBool), [format],
                            CompilerExecutableName);

                        continue;
                    }

                    format = format.ToLowerInvariant();
                }

                if (propertyType.IsEnum)
                {
                    string[] names = Enum.GetNames(propertyType);
                    if (!names.Contains(format, StringComparer.OrdinalIgnoreCase))
                    {
                        EmitErrorMessageFormatted(
                            0, 0, 0,
                            DS0255_DCConfigInvalidValue,
                            nameof(StringHelper.ConfigCommand_InvalidValueForEnum), [format, propertyType, string.Join(", ", names)],
                            CompilerExecutableName);

                        continue;
                    }

                    if (!names.Contains(format))
                        format = names.First(n => n.Equals(format, StringComparison.OrdinalIgnoreCase));
                }

                List<XElement> removed = [];
                foreach (XElement elem in doc.Descendants(dsconfigProperty.Name))
                {
                    if (string.IsNullOrEmpty(format))
                        removed.Add(elem);

                    elem.Value = format;
                }

                if (!doc.Descendants().Any(d => d.Name == dsconfigProperty.Name))
                {
                    doc.Root.Add(new XText("    "));
                    doc.Root.Add(new XElement(dsconfigProperty.Name, format));
                    doc.Root.Add(new XText(Environment.NewLine));
                }

                removed.ForEach(e => e.Remove());
            }

            doc.Save(ProjectConfigurationFileName, SaveOptions.DisableFormatting);
            return 0;
        }

        File.WriteAllText(ProjectConfigurationFileName, ProjectFileSerializer.SerializeEmpty());

        if (properties.Count == 0)
        {
            LogOut.WriteLine(StringHelper.Format(nameof(StringHelper.ConfigCommand_CreatedNewProjectFileDefaultSettings), ProjectConfigurationFileName));
            return 0;
        }

        Invoke(args);
        LogOut.WriteLine(StringHelper.Format(nameof(StringHelper.ConfigCommand_CreatedNewProjectFileSpecifiedSettings), ProjectConfigurationFileName));
        return 0;
    }

    private static string FormatObject(object obj)
    {
        if (obj == null)
            return "";

        if (obj is string s)
            return s;

        if (obj is Define[] macroDefinitions)
        {
            return $"[{string.Join(", ", macroDefinitions.Select(m => $"$({m.Name})={m.Value}"))}]";
        }

        if (obj is IEnumerable collection)
        {
            List<string> elems = [];
            foreach (object elem in collection)
                elems.Add(FormatObject(elem));

            return $"[{string.Join(", ", elems)}]";
        }

        return obj.ToString();
    }

    private static void PrintProperties(List<(string Key, string TypeName, string Value)> properties, bool displayMacros = false)
    {
        if (properties == null || properties.Count == 0)
        {
            string msg = StringHelper.ConfigCommand_NoPropertiesToDisplay;
            if (displayMacros)
                msg = StringHelper.ConfigCommand_NoMacrosToDisplay;

            LogOut.WriteLine(msg);
            return;
        }

        IEnumerable<string> keys = properties.Select(p => p.Key);
        IEnumerable<string> types = properties.Select(p => p.TypeName);
        IEnumerable<string> values = properties.Select(p => p.Value);

        string nameColumn = StringHelper.ConfigCommand_Name;
        string typeColumn = StringHelper.ConfigCommand_Type;
        string valueColumn = StringHelper.ConfigCommand_Value;
        string space = "    ";

        int nameColumnWidth = Math.Max(nameColumn.Length, keys.MaxBy(k => k.Length).Length);
        int typeColumnWidth = displayMacros ? 0 : Math.Max(typeColumn.Length, types.MaxBy(t => t.Length).Length);
        int valueColumnWidth = Math.Max(valueColumn.Length, values.MaxBy(v => v.Length).Length);

        StringBuilder sb = new();
        sb.AppendLine($"{nameColumn.PadRight(nameColumnWidth)}{(displayMacros ? "" : $"{space}{typeColumn.PadRight(typeColumnWidth)}")}{space}{valueColumn}");
        sb.AppendLine(new string('-', nameColumnWidth + space.Length + typeColumnWidth + space.Length + valueColumnWidth));

        foreach ((string key, string type, string val) in properties)
            sb.AppendLine($"{key.PadRight(nameColumnWidth)}{(displayMacros ? "" : $"{space}{type.PadRight(typeColumnWidth)}")}{space}{val}");

        LogOut.Write(sb.ToString());
    }
}