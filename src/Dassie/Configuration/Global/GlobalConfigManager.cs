using Dassie.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Dassie.Configuration.Global;

internal static class GlobalConfigManager
{
    public static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", GlobalConfigurationFileName);
    public static Dictionary<string, (GlobalConfigDataType Type, object Value)> Properties { get; private set; } = [];

    public static void Set(string key, GlobalConfigDataType type, object value)
    {
        if (!Properties.TryGetValue(key, out _))
        {
            Properties.Add(key, (type, value));
            Serialize();
            return;
        }

        Properties[key] = (type, value);
        Serialize();
    }

    public static void Initialize()
    {
        if (!File.Exists(ConfigPath))
        {
            StreamWriter sw = new(ConfigPath);
            sw.WriteLine("""
                <?xml version="1.0" encoding="utf-8"?>
                <Config/>
                """);
            sw.Dispose();
        }

        Properties = [];

        XDocument doc;
        List<XElement> modules;

        try
        {
            doc = XDocument.Load(ConfigPath);
            modules = doc.Root.Elements().ToList();
        }
        catch (XmlException xex)
        {
            EmitWarningMessage(
                0, 0, 0,
                DS0257_GlobalConfigFileMalformed,
                $"The global configuration file is malformed: {xex.Message} The default property values will be used.",
                ConfigPath);

            modules = [];
        }

        HashSet<string> extNames = [.. ExtensionLoader.InstalledExtensions.Select(p => p.Metadata.Name)];
        List<string> unknownModules = [];

        if (modules.Any(m => !extNames.Contains(m.Name.LocalName, StringComparer.OrdinalIgnoreCase)))
        {
            foreach (string unknownNs in modules.Where(m => !extNames.Contains(m.Name.LocalName, StringComparer.OrdinalIgnoreCase)).Select(e => e.Name.LocalName))
            {
                EmitWarningMessage(
                    0, 0, 0,
                    DS0256_GlobalConfigInvalidElement,
                    $"The configuration namespace '{unknownNs}' does not match any installed package.",
                    ConfigPath);

                unknownModules.Add(unknownNs);
            }
        }

        if (modules.Count < extNames.Count)
        {
            foreach (string ext in extNames.Where(e => !modules.Any(m => m.Name.LocalName.Equals(e, StringComparison.OrdinalIgnoreCase))))
            {
                // TODO: Support extensions with spaces in their name. Space (0x20) is not a valid character inside an XML identifier.
                // Unless someone complains, this is is not a high priority feature.

                if (ext.Contains(' '))
                    continue;

                modules.Add(new(ext, null));
            }
        }

        foreach (XElement module in modules)
        {
            if (unknownModules.Contains(module.Name.LocalName))
                continue;

            IPackage package = ExtensionLoader.InstalledExtensions.First(p => p.Metadata.Name.Equals(module.Name.LocalName, StringComparison.OrdinalIgnoreCase));
            GlobalConfigProperty[] properties = package.GlobalProperties();

            foreach (GlobalConfigProperty prop in properties)
                prop.ExtensionIdentifier = package.Metadata.Name;

            List<GlobalConfigProperty> foundProps = [];
            foreach (XElement property in module.Elements())
            {
                if (!properties.Any(p => p.Name.Equals(property.Name.LocalName, StringComparison.OrdinalIgnoreCase)))
                {
                    EmitWarningMessage(
                        0, 0, 0,
                        DS0256_GlobalConfigInvalidElement,
                        $"The package '{package.Metadata.Name}' does not define a global configuration property named '{property.Name.LocalName}'.",
                        ConfigPath);

                    continue;
                }

                GlobalConfigProperty prop = properties.First(p => p.Name.Equals(property.Name.LocalName, StringComparison.OrdinalIgnoreCase));
                Set($"{package.Metadata.Name}.{prop.Name}", prop.Type, GetValue(property.Value, prop.Type, out _));
                foundProps.Add(prop);
            }

            if (foundProps.Count < properties.Length)
            {
                foreach (GlobalConfigProperty prop in properties.Except(foundProps))
                    Set($"{package.Metadata.Name}.{prop.Name}", prop.Type, prop.DefaultValue);
            }
        }
    }

    public static string TypeName(GlobalConfigDataType type)
    {
        if (type.IsList)
            return $"List[{type.BaseType}]";

        return type.BaseType.ToString();
    }

    public static object GetValue(string format, GlobalConfigDataType type, out bool error)
    {
        error = false;
        void Error()
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0258_GlobalConfigPropertyValueMalformed,
                $"Malformed property value: '{format}' is not a valid value for a property of type '{TypeName(type)}'.",
                ConfigPath);
        }

        if (string.IsNullOrEmpty(format))
        {
            if (type.IsList)
                return null;

            return type.BaseType switch
            {
                GlobalConfigBaseType.Boolean => false,
                GlobalConfigBaseType.String => "",
                GlobalConfigBaseType.Integer => 0,
                GlobalConfigBaseType.Real => 0.0,
                _ => null
            };
        }

        if (type.IsList && (!format.StartsWith('[') || !format.EndsWith(']')))
        {
            Error();
            return null;
        }

        if (type.IsList)
        {
            format = format[1..^1];
            string[] values = format.Split(',');
            return new List<object>(values.Select(v => GetValue(v, type with { IsList = false }, out _)));
        }

        if (type.BaseType == GlobalConfigBaseType.Boolean)
        {
            if (!bool.TryParse(format, out bool val))
            {
                Error();
                error = true;
                return false;
            }

            return val;
        }

        if (type.BaseType == GlobalConfigBaseType.Real)
        {
            if (!double.TryParse(format, CultureInfo.InvariantCulture, out double val))
            {
                Error();
                error = true;
                return 0d;
            }

            return val;
        }

        if (type.BaseType == GlobalConfigBaseType.Integer)
        {
            if (!int.TryParse(format, CultureInfo.InvariantCulture, out int val))
            {
                Error();
                error = true;
                return 0;
            }

            return val;
        }

        return format;
    }

    public static string Format(object obj, GlobalConfigDataType type)
    {
        if (obj == null)
            return "";

        if (type.IsList)
        {
            StringBuilder sb = new("[");

            foreach (object elem in (IEnumerable)obj)
            {
                sb.Append(Format(elem, type with { IsList = false }));
                sb.Append(',');
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');
            return sb.ToString();
        }

        if (type.BaseType == GlobalConfigBaseType.Boolean)
            return obj.ToString().ToLowerInvariant();

        if (type.BaseType == GlobalConfigBaseType.Real)
            return Convert.ToString(obj, CultureInfo.InvariantCulture);

        return obj.ToString();
    }

    public static void Serialize()
    {
        IEnumerable<string> ns = Properties.DistinctBy(p => p.Key.Split('.')[0]).Select(p => p.Key.Split('.')[0]);
        XDocument doc = new(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("Config",
                ns.Select(n =>
                    new XElement(n,
                        Properties.Where(p => p.Key.StartsWith(n + "."))
                                  .Select(p => new XElement(string.Join('.', p.Key.Split('.')[1..]), Format(p.Value.Value, p.Value.Type)))))));

        doc.Save(ConfigPath);
    }
}