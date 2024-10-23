using Dassie.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Dassie.Cli;

/// <summary>
/// Parses command-line options for the '&lt;FileName&gt; [FileNames] [Options]' command of dc.
/// </summary>
internal static class CommandLineOptionParser
{
    public static readonly Dictionary<string, string> Aliases = new()
    {
        ["O"] = "BuildDirectory",
        ["T"] = "ApplicationType",
        ["A"] = "AssemblyFileName",
        ["R"] = "Runtime",
        ["M"] = "ILOptimizations",
        ["L"] = "MeasureElapsedTime",
        ["I"] = "GenerateILFiles",
        ["H"] = "GenerateNativeAppHost",
        ["C"] = "CacheSourceFiles"
    };

    private static readonly List<string> BooleanAliases = ["M", "L", "I", "H", "C"];

    public static void ParseOptions(ref string[] args, DassieConfig config)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith('-'))
                continue;

            string arg = args[i][1..].ToUpperInvariant();

            if (Aliases.TryGetValue(arg, out string option))
            {
                if (BooleanAliases.Contains(arg))
                {
                    args = [
                        .. args[..i],
                        $"--{option}=1",
                        .. args[(i + 1)..]
                    ];

                    continue;
                }

                else if (args[i] == args.Last() || args[i + 1].StartsWith('-'))
                {
                    EmitErrorMessage(0, 0, 0,
                        DS0089_InvalidDSConfigProperty,
                        $"Expected argument for option '{arg.ToLowerInvariant()}'.",
                        "dc");

                    continue;
                }

                args = [
                    .. args[..i],
                    $"--{option}={args[i+1]}",
                    .. args[(i + 2)..]
                    ];
            }
        }

        string[] options = args.Where(a => a.StartsWith("--")).ToArray();

        if (options.Length == 0)
        {
            foreach (string flag in args.Where(a => a.StartsWith('-')))
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0089_InvalidDSConfigProperty,
                    $"Invalid flag '{flag[1..]}'.",
                    "dc");
            }

            return;
        }

        Dictionary<string, PropertyInfo> propNames = [];
        foreach (PropertyInfo prop in typeof(DassieConfig).GetProperties())
        {
            XmlElementAttribute element = prop.GetCustomAttribute<XmlElementAttribute>();
            if (element != null && !string.IsNullOrEmpty(element.ElementName))
            {
                propNames.Add(element.ElementName.ToLowerInvariant(), prop);
                continue;
            }

            XmlAttributeAttribute attrib = prop.GetCustomAttribute<XmlAttributeAttribute>();
            if (attrib != null && !string.IsNullOrEmpty(attrib.AttributeName))
            {
                propNames.Add(attrib.AttributeName.ToLowerInvariant(), prop);
                continue;
            }

            propNames.Add(prop.Name.ToLowerInvariant(), prop);
        }

        foreach (string arg in options)
        {
            if (arg.Contains("::")) // Accessing a property of a more complex element (e.g. version info)
                ParseObjectOption(arg, config);

            else if (arg.Count(c => c == '=') == 1) // Normal option of type bool, string or enum -> no array or complex object
                ParseRegularOption(arg, propNames, config);

            else if (arg.Contains('+')) // Adding an element to an array
                ParseArrayOption(arg, config);

            else
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0089_InvalidDSConfigProperty,
                    $"Invalid option '{arg[2..]}'.",
                    "dc");
            }
        }
    }

    public static void ParseRegularOption(string arg, Dictionary<string, PropertyInfo> propNames, DassieConfig config)
    {
        string identifier = arg.Split('=')[0].ToLowerInvariant()[2..];
        string value = arg.Split('=')[1];

        if (propNames.TryGetValue(identifier, out PropertyInfo prop))
        {
            object val = value;

            try
            {
                if (prop.PropertyType == typeof(bool))
                {
                    if (bool.TryParse(value.ToLowerInvariant(), out bool b))
                        val = b;

                    else if (int.TryParse(value, out int i))
                    {
                        if (i == 0) val = false;
                        else if (i == 1) val = true;
                        else throw new Exception();
                    }

                    else throw new Exception();
                }

                else if (prop.PropertyType == typeof(int))
                {
                    if (int.TryParse(value, out int i))
                        val = i;
                }

                else if (prop.PropertyType.IsEnum)
                    val = Enum.Parse(prop.PropertyType, value);
            }
            catch (Exception)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0089_InvalidDSConfigProperty,
                    $"Invalid property value for '{prop.Name}': '{value}' cannot be converted to '{prop.PropertyType.FullName}'.",
                    "dc");
            }

            prop.SetValue(config, val);
            return;
        }

        EmitErrorMessage(
            0, 0, 0,
            DS0089_InvalidDSConfigProperty,
            $"Invalid option '{identifier}'.",
            "dc");
    }

    public static void ParseArrayOption(string arg, DassieConfig config)
    {
        string identifier = arg.Split('+')[0].ToLowerInvariant()[2..];
        string value = arg.Split('+')[1];

        // A purely reflection-based approach isn't really feasible here

        if ("references".StartsWith(identifier))
        {
            config.References ??= [];

            if (!File.Exists(value))
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0022_InvalidAssemblyReference,
                    $"Reference '{value}' could not be resolved.",
                    "dc");

                return;
            }

            if (Path.GetFileName(value) == "dsconfig.xml")
            {
                config.References =
                [
                    .. config.References,
                    new ProjectReference()
                    {
                        CopyToOutput = true,
                        ProjectFile = value
                    },
                ];
            }
            else
            {
                config.References =
                [
                    .. config.References,
                    new AssemblyReference()
                    {
                        CopyToOutput = true,
                        ImportNamespacesImplicitly = false,
                        AssemblyPath = value
                    },
                ];
            }

            return;
        }

        if ("resources".StartsWith(identifier))
        {
            throw new NotImplementedException();
        }

        if ("ignoredmessages".StartsWith(identifier))
        {
            config.IgnoredMessages ??= [];
            config.IgnoredMessages =
            [
                .. config.IgnoredMessages,
                new Message()
                {
                    Code = value
                }
            ];

            return;
        }
    }

    public static void ParseObjectOption(string arg, DassieConfig config)
    {
        string obj = arg.Split("::")[0].ToLowerInvariant()[2..];
        string identifier = arg.Split("::")[1].Split('=')[0].ToLowerInvariant();
        string value = arg.Split('=')[1];

        Type t = null;

        if ("versioninfo".StartsWith(obj))
            t = typeof(VersionInfo);

        if (t == null)
            return;

        Dictionary<string, PropertyInfo> propNames = [];
        foreach (PropertyInfo prop in t.GetProperties())
        {
            XmlElementAttribute element = prop.GetCustomAttribute<XmlElementAttribute>();
            if (element != null && !string.IsNullOrEmpty(element.ElementName))
            {
                propNames.Add(element.ElementName.ToLowerInvariant(), prop);
                continue;
            }

            XmlAttributeAttribute attrib = prop.GetCustomAttribute<XmlAttributeAttribute>();
            if (attrib != null && !string.IsNullOrEmpty(attrib.AttributeName))
            {
                propNames.Add(attrib.AttributeName.ToLowerInvariant(), prop);
                continue;
            }

            propNames.Add(prop.Name.ToLowerInvariant(), prop);
        }

        if (propNames.TryGetValue(identifier, out PropertyInfo _prop))
        {
            object val = value;

            try
            {
                if (_prop.PropertyType == typeof(bool))
                {
                    if (bool.TryParse(value.ToLowerInvariant(), out bool b))
                        val = b;

                    else if (int.TryParse(value, out int i))
                    {
                        if (i == 0) val = false;
                        else if (i == 1) val = true;
                        else throw new Exception();
                    }

                    else throw new Exception();
                }

                else if (_prop.PropertyType.IsEnum)
                    val = Enum.Parse(_prop.PropertyType, value);
            }
            catch (Exception)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0089_InvalidDSConfigProperty,
                    $"Invalid property value for '{_prop.Name}': '{value}' cannot be converted to '{_prop.PropertyType.FullName}'.",
                    "dc");
            }

            if ("versioninfo".StartsWith(obj))
            {
                config.VersionInfo ??= new();
                _prop.SetValue(config.VersionInfo, val);
            }
        }
    }
}