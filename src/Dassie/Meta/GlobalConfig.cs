using Dassie.Configuration;

namespace Dassie.Meta;

internal static class GlobalConfig
{
    public static bool AdvancedDiagnostics { get; set; }

    public static ToolPaths ExternalToolPaths { get; set; }

    public static bool DisableDebugInfo { get; set; }

    public static string RelativePathResolverDirectory { get; set; }

    public static bool BuildDirectoryCreated { get; set; }
}