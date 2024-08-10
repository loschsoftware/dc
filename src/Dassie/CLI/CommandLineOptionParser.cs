using Dassie.Configuration;
using Dassie.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Dassie.CLI;

/// <summary>
/// Parses command-line options for the '&lt;FileName&gt; [FileNames] [Options]' command of dc.
/// </summary>
internal static class CommandLineOptionParser
{
    public static void ParseOptions(string[] args, DassieConfig config)
    {
        //args = args.Where(a => a.StartsWith("--") && a.Count(c => c == '=') == 1).Select(a => a[2..]);
        args = args.Where(a => a.StartsWith("--")).ToArray();

        if (!args.Any())
            return;

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

        foreach (string arg in args)
        {
            if (arg.Contains("::")) // Accessing a property of a more complex element (e.g. version info)
            {
                ParseObjectOption(arg, config);
                continue;
            }

            if (arg.Count(c => c == '=') == 1) // Normal option of type bool, string or enum -> no array or complex object
            {
                ParseRegularOption(arg, propNames, config);
                continue;
            }

            if (arg.Contains('+')) // Adding an element to an array
            {
                ParseArrayOption(arg, config);
                continue;
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
        }
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