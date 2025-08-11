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
            EmitErrorMessage(
                0, 0, 0,
                DS0091_MalformedConfigurationFile,
                "Invalid format version.",
                ProjectConfigurationFileName);

            return;
        }

        if (formatVersion.Major > current.Major)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0092_ConfigurationFormatVersionTooNew,
                $"Project configuration file uses a newer format than supported by this compiler. This compiler only supports project files up to format version {current.ToString(1)}.",
                ProjectConfigurationFileName);
        }

        if (formatVersion.Major < current.Major)
        {
            EmitWarningMessage(
                0, 0, 0,
                DS0093_ConfigurationFormatVersionTooOld,
                $"Project configuration file uses an outdated format. For best compatibility, the project file should be updated to version {current.ToString(2)}. Use the 'dc config update' command to perform this action automatically.",
                ProjectConfigurationFileName);
        }
    }
}