using Dassie.Errors;
using Dassie.Validation;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Dassie.Configuration;

internal static class ProjectFileDeserializer
{
    private static DassieConfig _config;
    public static DassieConfig DassieConfig => _config ??= Deserialize();

    private static DassieConfig Deserialize()
    {
        if (!File.Exists(ProjectConfigurationFileName))
            return null;

        XmlSerializer xmls = new(typeof(DassieConfig));
        using StreamReader sr = new(ProjectConfigurationFileName);
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
                DS0090_MalformedConfigurationFile,
                $"Invalid project file.{string.Join(':', ex.InnerException.Message.Split(':')[1..])}",
                ProjectConfigurationFileName);
        }

        foreach (ErrorInfo error in ConfigValidation.Validate(ProjectConfigurationFileName))
            EmitGeneric(error);

        return config;
    }
}