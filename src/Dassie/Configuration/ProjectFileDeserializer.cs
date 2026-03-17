using Dassie.Configuration.Macros;
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

    public static DassieConfig Deserialize(string path, bool handleImports = true)
    {
        if (!File.Exists(path))
            return null;

        Path = System.IO.Path.GetFullPath(path);

        XDocument doc = XDocument.Load(path, LoadOptions.SetLineInfo);
        ConfigImportManager.ImportMacroDefinitions(doc);

        MacroParser2 parser = new(doc, path);
        bool result = parser.Normalize();

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