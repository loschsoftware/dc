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

    public override string Description => "Manages compiler settings and project configurations.";

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
            ("Property=[Value]", "The property to modify. Multiple can be specified, separated by spaces. The value is optional, if omitted, the default value is used. Note that the equals sign (=) is still required."),
            ("--global", "Indicates that the operation displays or modifies the global configuration, as opposed to a project file."),
            ("    --reset", "Resets all global properties to their default value."),
            ("    --import <Path>", "Imports the global configuration from the specified file.")
        ],
        Remarks = $"This command is used to display or change global or project-specific compiler settings. If this command is called without arguments in a directory containing a project file, it will display the current project configuration. Similarly, the '--global' flag is used to change or show the global configuration.{Environment.NewLine}{Environment.NewLine}"
        + "If 'dc config' is called in a directory not containing a project file, a new project file will be created. This is useful for initializing a Dassie project in an existing directory structure, as opposed to the 'dc new' command which creates a new project structure based on a template. If property values are supplied to the command as arguments, they will be applied to the generated project file.",
        Examples =
        [
            ("dc config", "Creates a new project file with default values in the current directory, or displays the current project configuration if one already exists."),
            ("dc config MeasureElapsedTime=true Verbosity=2", "Changes two settings of the project configuration."),
            ("dc config --global", "Displays the global compiler configuration."),
            ("dc config --global core.scratchpad.editor=vim", "Changes a setting of the global configuration.")
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
            EmitErrorMessage(
                0, 0, 0,
                DS0013_InvalidArgument,
                $"Unexpected argument '{arg}'.",
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
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0013_InvalidArgument,
                        $"File path to import from required.",
                        CompilerExecutableName);

                    return -1;
                }

                string path = args[args.IndexOf("--import") + 1];
                if (!File.Exists(path))
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0198_ImportedConfigFileNotFound,
                        $"The specified file '{path}' could not be found.",
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
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0253_DCConfigInvalidProperty,
                        $"Invalid global property '{prop.Key}'. Use 'dc config --global' to display all global properties.",
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
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0253_DCConfigInvalidProperty,
                        $"Invalid project file property '{prop.Key}'.",
                        CompilerExecutableName);

                    continue;
                }

                Type propertyType = dsconfigProperty.PropertyType;
                object defaultVal = null;
                if (dsconfigProperty.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute dva)
                    defaultVal = dva.Value;

                if (propertyType != typeof(string) && !propertyType.IsPrimitive && !propertyType.IsEnum)
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0254_DCConfigUnsupportedDataType,
                        $"The property '{dsconfigProperty.Name}' has an unsupported data type ({TypeHelpers.TypeName(propertyType)}). 'dc config' only supports properties of type 'string' or of enum or primitive types.",
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
                        EmitErrorMessage(
                            0, 0, 0,
                            DS0255_DCConfigInvalidValue,
                            $"'{format}' is not a valid value for properties of type 'bool'. Allowed values are 'true' and 'false'.",
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
                        EmitErrorMessage(
                            0, 0, 0,
                            DS0255_DCConfigInvalidValue,
                            $"'{format}' is not a valid value for properties of type '{propertyType}'. Allowed values are [{string.Join(", ", names)}].",
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
            LogOut.WriteLine($"Created new {ProjectConfigurationFileName} with default values.");
            return 0;
        }

        Invoke(args);
        LogOut.WriteLine($"Created new {ProjectConfigurationFileName} with specified settings.");
        return 0;
    }

    private static void PrintProperties(List<(string Key, string TypeName, string Value)> properties)
    {
        if (properties == null || properties.Count == 0)
        {
            Console.WriteLine("No properties to display.");
            return;
        }

        IEnumerable<string> keys = properties.Select(p => p.Key);
        IEnumerable<string> types = properties.Select(p => p.TypeName);
        IEnumerable<string> values = properties.Select(p => p.Value);

        string nameColumn = "Name";
        string typeColumn = "Type";
        string valueColumn = "Value";
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