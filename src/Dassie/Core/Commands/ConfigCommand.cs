using Dassie.Configuration;
using Dassie.Configuration.Global;
using Dassie.Extensions;
using System;
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
            "dc config --global [--reset] [--import <Path>] [<Property>=[Value]]..."
        ],
        Options =
        [
            ("Property=[Value]", StringHelper.ConfigCommand_PropertyOption),
            ("--global", StringHelper.ConfigCommand_GlobalOption),
            ("    --reset", StringHelper.ConfigCommand_ResetOption),
            ("    --import <Path>", StringHelper.ConfigCommand_ImportOption)
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

        if (args != null)
        {
            properties = args.Where(a => a.Contains('=')).Select(a =>
            {
                string[] parts = a.Split('=');
                string key = parts[0];
                string value = string.Join('=', parts[1..]);
                return new KeyValuePair<string, string>(key, value);
            }).ToDictionary();
        }

        foreach (string arg in args.Where(a => !a.StartsWith('-') && !a.Contains('=')))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0013_InvalidArgument,
                nameof(StringHelper.ConfigCommand_UnexpectedArgument), [arg],
                CompilerExecutableName);
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
            DassieConfig config = ProjectFileDeserializer.DassieConfig;

            if (properties.Count == 0)
            {
                List<(string Key, string Type, string Value)> props = [];

                foreach (PropertyInfo prop in typeof(DassieConfig).GetProperties())
                {
                    object defaultVal = null;
                    if (prop.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute dva)
                        defaultVal = dva.Value;

                    object val = prop.GetValue(config);

                    bool IsEqual()
                    {
                        if (val == null && prop.PropertyType == typeof(string) && (string)defaultVal == "")
                            return true;

                        if (val == null)
                            return val == defaultVal;

                        if (val is string s && s == "" && prop.PropertyType == typeof(string) && defaultVal == null)
                            return true;

                        return val.Equals(defaultVal);
                    }

                    if (!IsEqual())
                        props.Add((prop.Name, HelpCommand.GetPropertyTypeName(prop.PropertyType), val?.ToString() ?? ""));
                }

                PrintProperties(props);
                return 0;
            }

            XDocument doc = XDocument.Load(ProjectConfigurationFileName, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            PropertyInfo[] dsconfigProps = typeof(DassieConfig).GetProperties();

            foreach (KeyValuePair<string, string> prop in properties)
            {
                PropertyInfo dsconfigProperty = null;

                if (dsconfigProps.Any(p => p.Name.Equals(prop.Key, StringComparison.OrdinalIgnoreCase)))
                    dsconfigProperty = dsconfigProps.First(p => p.Name.Equals(prop.Key, StringComparison.OrdinalIgnoreCase));

                if (dsconfigProperty == null)
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0253_DCConfigInvalidProperty,
                        nameof(StringHelper.ConfigCommand_InvalidProjectFileProperty), [prop.Key],
                        CompilerExecutableName);

                    continue;
                }

                Type propertyType = dsconfigProperty.PropertyType;
                object defaultVal = null;
                if (dsconfigProperty.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute dva)
                    defaultVal = dva.Value;

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
                    doc.Root.Add(new XElement(dsconfigProperty.Name, format));

                removed.ForEach(e => e.Remove());
            }

            doc.Save(ProjectConfigurationFileName, SaveOptions.DisableFormatting);
            return 0;
        }

        using StreamWriter configWriter = new(ProjectConfigurationFileName);

        XmlSerializerNamespaces ns = new();
        ns.Add("", "");

        XmlSerializer xmls = new(typeof(DassieConfig));
        xmls.Serialize(configWriter, new DassieConfig(), ns);

        configWriter.Dispose();

        if (properties.Count == 0)
        {
            LogOut.WriteLine(StringHelper.Format(nameof(StringHelper.ConfigCommand_CreatedNewProjectFileDefaultSettings), ProjectConfigurationFileName));
            return 0;
        }

        Invoke(args);
        LogOut.WriteLine(StringHelper.Format(nameof(StringHelper.ConfigCommand_CreatedNewProjectFileSpecifiedSettings), ProjectConfigurationFileName));
        return 0;
    }

    private static void PrintProperties(List<(string Key, string TypeName, string Value)> properties)
    {
        if (properties == null || properties.Count == 0)
        {
            LogOut.WriteLine(StringHelper.ConfigCommand_NoPropertiesToDisplay);
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
        int typeColumnWidth = Math.Max(typeColumn.Length, types.MaxBy(t => t.Length).Length);
        int valueColumnWidth = Math.Max(valueColumn.Length, values.MaxBy(v => v.Length).Length);

        StringBuilder sb = new();
        sb.AppendLine($"{nameColumn.PadRight(nameColumnWidth)}{space}{typeColumn.PadRight(typeColumnWidth)}{space}{valueColumn}");
        sb.AppendLine(new string('-', nameColumnWidth + space.Length + typeColumnWidth + space.Length + valueColumnWidth));

        foreach ((string key, string type, string val) in properties)
            sb.AppendLine($"{key.PadRight(nameColumnWidth)}{space}{type.PadRight(typeColumnWidth)}{space}{val}");

        Console.Write(sb.ToString());
    }
}