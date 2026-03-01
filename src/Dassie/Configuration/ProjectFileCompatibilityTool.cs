using System;

namespace Dassie.Configuration;

// Once dsconfig.xml uses a format version greater than 1.0, this tool is supposed to upgrade an older format to the newest one.
internal static class ProjectFileCompatibilityTool
{
    public static void VerifyFormatVersionCompatibility(DassieConfig config)
    {
        config.FormatVersion ??= DassieConfig.CurrentFormatVersion;
        Version current = Version.Parse(DassieConfig.CurrentFormatVersion);

        if (!Version.TryParse(config.FormatVersion, out Version formatVersion))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0091_MalformedConfigurationFile,
                nameof(StringHelper.ProjectFileCompatibilityTool_InvalidFormatVersion), [],
                ProjectConfigurationFileName);

            return;
        }

        if (formatVersion.Major > current.Major)
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0092_ConfigurationFormatVersionTooNew,
                nameof(StringHelper.ProjectFileCompatibilityTool_FormatVersionTooNew), [current.ToString(1)],
                ProjectConfigurationFileName);
        }

        if (formatVersion.Major < current.Major)
        {
            EmitWarningMessageFormatted(
                0, 0, 0,
                DS0093_ConfigurationFormatVersionTooOld,
                nameof(StringHelper.ProjectFileCompatibilityTool_FormatVersionTooOld), [current.ToString(2)],
                ProjectConfigurationFileName);
        }
    }
}