using Dassie.Core.Macros;
using Dassie.Extensions;
using Dassie.Messages;
using Dassie.Messages.Devices;
using Dassie.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using IOPath = System.IO.Path;

namespace Dassie.Configuration;

internal static class ProjectFileDeserializer
{
    private static DassieConfig _config;
    public static DassieConfig DassieConfig => _config ??= Deserialize();

    public static string Path { get; private set; }

    public static void Reload() => _config = Deserialize();
    public static void Set(DassieConfig cfg) => _config = cfg;

    private static DassieConfig Deserialize()
        => Deserialize(ProjectConfigurationFileName);

    // Lookup paths for referenced configuration files
    private static readonly List<string> _lookupDirs =
    [
        IOPath.Combine(IOPath.GetDirectoryName(typeof(ProjectFileDeserializer).Assembly.Location), SdkDirectoryName), // Application directory
        IOPath.Combine(ApplicationDataDirectoryPath, SdkDirectoryName), // Application data directory
        IOPath.Combine(IOPath.GetDirectoryName(IOPath.GetDirectoryName(typeof(ProjectFileDeserializer).Assembly.Location)), SdkDirectoryName), // Application binaries
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

    private static object GetRawValue(Property prop, IEnumerable<XElement> matchingElements, bool getArrayElement = false)
    {
        Type type = prop.Type;

        if (type.IsArray && !getArrayElement)
        {
            if (!matchingElements.Any())
            {
                return prop.Default;
            }

            if (matchingElements.Count() == 1 && matchingElements.Single() is XElement array && array.Name.LocalName == prop.Name)
                return GetRawValue(prop, array.Elements());

            return matchingElements.Select(e => GetRawValue(prop, [e], true)).ToArray();
        }

        if (!matchingElements.Any())
            return prop.Default;

        if (matchingElements.Count() > 1)
        {
            // ERROR: Property specified multiple times
            return null;
        }

        XElement elem = matchingElements.Single();

        if (type == typeof(string) || type.IsPrimitive || type.IsEnum)
            return elem.Value;

        return elem;
    }

    public static DassieConfig Deserialize(string path, bool handleImports = true)
    {
        if (!File.Exists(path))
            return null;

        Path = System.IO.Path.GetFullPath(path);

        XDocument doc = XDocument.Load(path, LoadOptions.SetLineInfo);

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

        PropertyStore ps = new(props, rawValues);
        DassieConfig cfg = new(ps);

        PropMacro propMacro = new(cfg); // TODO: Register this with the macro parser

        string asmName = cfg.GetProperty<string>("AssemblyFileName");
        string asmName2 = cfg.GetProperty<string>("AssemblyFileName");

        //ConfigImportManager.ImportMacroDefinitions(doc);
        //MacroParser2 parser = new(doc, path);
        //bool result = parser.Normalize();

        XmlSerializer xmls = new(typeof(DassieConfig));
        DassieConfig config = null;

        try
        {
            config = (DassieConfig)xmls.Deserialize(doc.Root.CreateReader());
        }
        catch (Exception ex)
        {
            /* TODO: Parsing will fail if a property of a type other than string is constructed from a macro, like in this example:
             * 
             * <DassieConfig>
             *      <MacroDefinitions>
             *          <Define Macro="V">2</Define>
             *      </MacroDefinitions>
             *      <Verbosity>$(V)</Verbosity>
             * </DassieConfig>
            */

            int row = 0, col = 0;

            if (ex.Message.Contains('('))
            {
                row = int.Parse(ex.Message.Split('(')[1].Split(',')[0]);
                col = int.Parse(ex.Message.Split('(')[1].Split(',')[1][1..^2]);
            }

            EmitErrorMessageFormatted(
                row, col, 0,
                DS0091_MalformedConfigurationFile,
                nameof(StringHelper.ProjectFileDeserializer_InvalidProjectFile), [string.Join(':', ex.InnerException.Message.Split(':')[1..])],
                path);
        }

        if (config.Extensions != null && config.Extensions.Count > 0)
            ExtensionLoader.LoadTransientExtensions(config.Extensions.Select(e => (IOPath.GetFullPath(e.Path), e.Attributes, e.Elements)));

        if (handleImports)
            ConfigImportManager.Merge(config);

        foreach (MessageInfo error in ConfigValidation.Validate(path))
            Emit(error);

        BuildLogDeviceContextBuilder.RegisterBuildLogDevices(config, path);
        return config;
    }
}