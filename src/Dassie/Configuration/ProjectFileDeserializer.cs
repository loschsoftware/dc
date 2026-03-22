using Dassie.Configuration.Macros;
using Dassie.Core.Macros;
using Dassie.Extensions;
using Dassie.Messages.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using IOPath = System.IO.Path;

namespace Dassie.Configuration;

internal static class ProjectFileDeserializer
{
    private static DassieConfig _config;
    public static DassieConfig DassieConfig => _config ??= Deserialize();

    public static string Path { get; private set; }
    public static IReadOnlyList<Define> MacroDefinitions { get; private set; }

    public static void Reload() => _config = Deserialize();
    public static void Set(DassieConfig cfg) => _config = cfg;

    private static DassieConfig Deserialize()
        => Deserialize(ProjectConfigurationFileName);

    // Lookup paths for referenced configuration files
    private static readonly List<string> _lookupDirs =
    [
        IOPath.Combine(IOPath.GetDirectoryName(typeof(ProjectFileDeserializer).Assembly.Location), SdkDirectoryName),
        IOPath.Combine(ApplicationDataDirectoryPath, SdkDirectoryName),
        IOPath.Combine(IOPath.GetDirectoryName(IOPath.GetDirectoryName(typeof(ProjectFileDeserializer).Assembly.Location)), SdkDirectoryName)
    ];

    public static XDocument Load(string path)
    {
        if (path == null)
            return null;

        if (File.Exists(path))
            return XDocument.Load(path);

        if (Directory.Exists(path))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0198_ImportedConfigFileNotFound,
                $"The path '{path}' refers to a directory, not to a configuration file.", [path],
                path);

            return null;
        }

        foreach (string lookupDir in _lookupDirs)
        {
            string newPath = IOPath.GetFullPath(IOPath.Combine(lookupDir, path));

            if (File.Exists(newPath))
                return Load(newPath);
        }

        EmitErrorMessageFormatted(
            0, 0, 0,
            DS0198_ImportedConfigFileNotFound,
            $"The referenced configuration file '{path}' could not be found.", [path],
            path);

        return null;
    }

    private static List<Define> GetMacroDefinitions(Dictionary<string, object> rawValues)
    {
        if (!rawValues.TryGetValue(nameof(DassieConfig.MacroDefinitions), out object raw) || raw == null)
            return [];

        if (raw is IEnumerable<Define> alreadyTyped)
            return alreadyTyped.Where(d => d != null).ToList();

        IEnumerable<XElement> defineElements = raw switch
        {
            XElement single => [single],
            object[] arr => arr.OfType<XElement>(),
            IEnumerable<XElement> seq => seq,
            _ => []
        };

        List<Define> snapshot = [];

        foreach (XElement elem in defineElements.Where(e => e != null))
        {
            Define def = new(PropertyStore.Empty)
            {
                Name = (string)elem.Attribute("Macro"),
                Parameters = (string)elem.Attribute("Parameters"),
                Trim = (bool?)elem.Attribute("Trim") ?? false,
                Value = elem.Value
            };

            snapshot.Add(def);
        }

        return snapshot;
    }

    private static object GetRawValue(Property prop, IEnumerable<XElement> matchingElements, bool getArrayElement = false)
    {
        Type type = prop?.Type ??
            ((matchingElements.Count() > 1 && !getArrayElement) ? typeof(object[])
            : typeof(object));

        if ((type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) && !getArrayElement))
        {
            if (!matchingElements.Any())
            {
                return prop.Default;
            }

            if (matchingElements.Count() == 1 && matchingElements.Single() is XElement array && array.Name.LocalName == prop.Name)
                return GetRawValue(prop, array.Elements());

            return matchingElements.Select(e => GetRawValue(prop with { Type = PropertyStore.GetElementType(type) }, [e], true)).ToArray();
        }

        if (!matchingElements.Any())
            return prop.Default;

        if (matchingElements.Count() > 1)
        {
            // TODO: ERROR: Property specified multiple times
            return null;
        }

        XElement elem = matchingElements.Single();

        if (type == typeof(string) || type.IsPrimitive || type.IsEnum)
            return elem.Value;

        return elem;
    }

    private static Dictionary<string, object> GetRawValues(XDocument doc)
    {
        ConfigImportManager.ImportMacroDefinitions(doc);

        IEnumerable<Property> props = ExtensionLoader.Properties;
        IEnumerable<XAttribute> attributes = doc.Root.Attributes();
        IEnumerable<XElement> elements = doc.Root.Elements();

        if (attributes.Any(a => a.Name == "Base"))
        {
            string baseConfig = attributes.First(a => a.Name == "Base").Value;
        }

        Dictionary<string, object> rawValues = [];
        foreach (Property prop in props)
        {
            string name = prop.Name;
            Type type = prop.Type;
            IEnumerable<XElement> matchingElements = elements.Where(e => e.Name.LocalName == name);
            rawValues.Add(name, GetRawValue(prop, matchingElements));
        }

        foreach (IEnumerable<XElement> customProps in elements.Where(e => !rawValues.ContainsKey(e.Name.LocalName)).GroupBy(e => e.Name.LocalName))
        {
            XElement first = customProps.First();
            Type type = (first.HasElements || first.HasAttributes) ? typeof(object) : typeof(string);

            if (customProps.Count() > 1)
                type = type.MakeArrayType();

            Property prop = new(first.Name.LocalName, type);
            rawValues.Add(first.Name.LocalName, GetRawValue(prop, customProps));
        }

        return rawValues;
    }

    public static DassieConfig Deserialize(string path, bool handleImports = true)
    {
        if (!File.Exists(path))
            return DassieConfig.Default;

        Path = System.IO.Path.GetFullPath(path);

        XDocument doc = XDocument.Load(path, LoadOptions.SetLineInfo);
        Dictionary<string, object> rawValues = GetRawValues(doc);

        MacroDefinitions = GetMacroDefinitions(rawValues);

        MacroParser parser = new();
        parser.SetMacroDefinitions(MacroDefinitions);

        PropertyStore ps = new(ExtensionLoader.Properties, parser, rawValues);
        DassieConfig config = new(ps, doc);
        parser.BindPropertyResolver(key => config[key]);

        PropMacro propMacro = new(config);
        parser.AddMacro(propMacro);

        if (config.Extensions != null && config.Extensions.Count > 0)
            ExtensionLoader.LoadTransientExtensions(config.Extensions.Select(e => (IOPath.GetFullPath(e.Path), e.Attributes, e.Elements)));

        if (handleImports)
            ConfigImportManager.Merge(config);

        BuildLogDeviceContextBuilder.RegisterBuildLogDevices(config, path);
        return config;
    }
}