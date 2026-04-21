using Dassie.Cli;
using Dassie.Core.Properties;
using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Dassie.Core.Commands;

internal class VersionCommand : CompilerCommand
{
    private static VersionCommand _instance;
    public static VersionCommand Instance => _instance ??= new();

    public override string Command => "--version";
    public override List<string> Aliases => ["-v", "--env", "--environment"];
    public override CommandOptions Options => CommandOptions.Hidden | CommandOptions.NoHelpRouting;
    public override string Description => "";

    /// <summary>
    /// Days between 01/01/2000 and 27/04/2023, on which development on dc was started.
    /// </summary>
    private const int DateOffset = 8517;

    public static Version GetFriendlyVersion(Version version)
    {
        return new(version.Major, version.Minor, version.Build - DateOffset);
    }

    public static Version FrameworkVersion => typeof(object).Assembly.GetName().Version;
    public static Version AssemblyFriendlyVersion => GetFriendlyVersion(Assembly.GetExecutingAssembly().GetName().Version);
    public static DateOnly AssemblyBuildDate => new DateOnly(2000, 1, 1).AddDays(AssemblyFriendlyVersion.Build + DateOffset);

#pragma warning disable IL3000
    public static string AssemblyDirectory
    {
        get
        {
            string loc = typeof(Program).Assembly.Location;
            if (string.IsNullOrWhiteSpace(loc))
                loc = AppContext.BaseDirectory;

            return loc;
        }
    }

    public static bool AssemblyIsManagedDll
    {
        get
        {
#if STANDALONE
            return false;
#else
            return true;
#endif
        }
    }

    public static string AssemblyFile
    {
        get
        {
            string loc = typeof(Program).Assembly.Location;
            if (!string.IsNullOrWhiteSpace(loc))
                return loc;

            string dir = AssemblyDirectory;
            string ext = ".dll";

            // Heuristics that are hopefully accurate enough
            if (!AssemblyIsManagedDll)
            {
                if (OperatingSystem.IsWindows())
                    ext = ".exe";
                else
                    ext = "";
            }

            return Path.Combine(dir, $"{CompilerExecutableName}{ext}");
        }
    }

    public override int Invoke(string[] args)
    {
        StringBuilder output = new();
        int padding = 35;

        output.AppendLine();
        output.AppendLine(StringHelper.ProductNameFull);
        output.AppendLine(StringHelper.Format(nameof(StringHelper.VersionCommand_Copyright), AssemblyBuildDate.Year));

        output.AppendLine();
        output.AppendLine(StringHelper.Format(nameof(StringHelper.VersionCommand_BuildID), IdCommand.GetBuildID(AssemblyFriendlyVersion)));

        output.AppendLine();
        output.AppendLine(StringHelper.VersionCommand_Environment);
        output.AppendLine($"{StringHelper.VersionCommand_CompilerVersion.PadRight(padding)}{AssemblyFriendlyVersion.ToString(2)}");
        output.AppendLine($"{StringHelper.VersionCommand_BuildNumber.PadRight(padding)}{AssemblyFriendlyVersion.Build}");
        output.AppendLine($"{StringHelper.VersionCommand_CompilationDate.PadRight(padding)}{AssemblyBuildDate.ToShortDateString()}");
        output.Append(StringHelper.VersionCommand_CompilerArchitecture.PadRight(padding));

#if STANDALONE
output.AppendLine(StringHelper.VersionCommand_CompilerArchitectureAot);
#else
        output.AppendLine(StringHelper.VersionCommand_CompilerArchitectureJit);
#endif
        output.AppendLine($"{StringHelper.VersionCommand_DotNetVersion.PadRight(padding)}{typeof(object).Assembly.GetName().Version}");
        output.AppendLine($"{StringHelper.VersionCommand_OperatingSystem.PadRight(padding)}{RuntimeInformation.OSDescription}");
        output.AppendLine($"{StringHelper.VersionCommand_OSArchitecture.PadRight(padding)}{RuntimeInformation.OSArchitecture}");

        output.AppendLine();
        output.AppendLine(StringHelper.VersionCommand_Locations);
        output.AppendLine($"{StringHelper.VersionCommand_ExtensionStorage.PadRight(padding)}\"{ExtensionLocationProperty.Instance.GetValue()}\"");
        output.AppendLine($"{StringHelper.VersionCommand_GlobalToolsStorage.PadRight(padding)}\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Tools")}\"");
        output.AppendLine($"{StringHelper.VersionCommand_NuGetPackageStorage.PadRight(padding)}\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages")}\"");
        output.AppendLine($"{StringHelper.VersionCommand_ToolPathsFile.PadRight(padding)}\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "tools.xml")}\"");

        WriteString(output.ToString());
        return 0;
    }
}

internal class IdCommand : CompilerCommand
{
    private static IdCommand _instance;
    public static IdCommand Instance => _instance ??= new();

    public override string Command => "--build-id";
    public override string Description => "";
    public override CommandOptions Options => CommandOptions.Hidden | CommandOptions.NoHelpRouting;

    public override int Invoke(string[] args)
    {
        WriteString($"{GetBuildID(VersionCommand.AssemblyFriendlyVersion)}{Environment.NewLine}");
        return 0;
    }

    public static string GetBuildID(Version version)
    {
        StringBuilder output = new();
        output.Append($"{version.ToString(2)}.{version.Build}");

#if STANDALONE
output.Append('a');
#else
        output.Append('j');
#endif

        string osShortName = RuntimeInformation.OSDescription.Replace(' ', '_');
        if (OperatingSystem.IsWindows())
            osShortName = "win";
        else if (OperatingSystem.IsLinux())
            osShortName = "linux";
        else if (OperatingSystem.IsMacOS())
            osShortName = "macOS";

        output.Append($"_{osShortName}-{RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()}");
        return output.ToString();
    }
}