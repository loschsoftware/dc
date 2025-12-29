using Dassie.Messages.Devices;
using Dassie.Extensions;
using Dassie.Messages;
using Dassie.Validation;
using System;
using System.IO;
using System.Linq;
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
    
    public static DassieConfig Deserialize(string path, bool handleImports = true)
    {
        if (!File.Exists(path))
            return null;

        Path = System.IO.Path.GetFullPath(path);

        XmlSerializer xmls = new(typeof(DassieConfig));
        using StreamReader sr = new(path);
        DassieConfig config = null;

        try
        {
            config = (DassieConfig)xmls.Deserialize(sr);
        }
        catch (Exception ex)
        {
            int row = 0, col = 0;

            if (ex.Message.Contains('('))
            {
                row = int.Parse(ex.Message.Split('(')[1].Split(',')[0]);
                col = int.Parse(ex.Message.Split('(')[1].Split(',')[1][1..^2]);
            }

            EmitErrorMessage(
                row, col, 0,
                DS0091_MalformedConfigurationFile,
                $"Invalid project file.{string.Join(':', ex.InnerException.Message.Split(':')[1..])}",
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