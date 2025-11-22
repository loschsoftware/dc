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

    public override int Invoke(string[] args)
    {
        StringBuilder output = new();

        Version v = Assembly.GetExecutingAssembly().GetName().Version;
        // 8517 -> Days between 01/01/2000 and 27/04/2023, on which development on dc was started
        Version version = new(v.Major, v.Minor, v.Build - 8517);
        DateTime buildDate = new DateTime(2000, 1, 1).AddDays(v.Build);

        int padding = 35;

        output.AppendLine();
        output.AppendLine(ProductName);
        output.AppendLine($"(C) 2023-{buildDate.Year} Losch");

        output.AppendLine();
        output.AppendLine($"Build ID: {IdCommand.GetBuildID(version)}");

        output.AppendLine();
        output.AppendLine("Environment:");
        output.AppendLine($"{"    - Compiler version:".PadRight(padding)}{v.ToString(2)}");
        output.AppendLine($"{"    - Build number:".PadRight(padding)}{version.Build}");
        output.AppendLine($"{"    - Compilation date:".PadRight(padding)}{buildDate.ToShortDateString()}");
        output.Append("    - Compiler architecture:".PadRight(padding));

#if STANDALONE
output.AppendLine("AOT, statically linked");
#else
        output.AppendLine("JIT, dynamically linked");
#endif
        output.AppendLine($"{"    - .NET version:".PadRight(padding)}{typeof(object).Assembly.GetName().Version}");
        output.AppendLine($"{"    - Operating system:".PadRight(padding)}{RuntimeInformation.OSDescription}");
        output.AppendLine($"{"    - OS architecture:".PadRight(padding)}{RuntimeInformation.OSArchitecture}");

        output.AppendLine();
        output.AppendLine("Locations:");
        output.AppendLine($"{"    - Extension storage:".PadRight(padding)}\"{ExtensionLocationProperty.Instance.GetValue()}\"");
        output.AppendLine($"{"    - Global tools storage:".PadRight(padding)}\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Tools")}\"");
        output.AppendLine($"{"    - NuGet package storage:".PadRight(padding)}\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages")}\"");
        output.AppendLine($"{"    - Tool paths file:".PadRight(padding)}\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "tools.xml")}\"");

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
        Version v = Assembly.GetExecutingAssembly().GetName().Version;
        Version version = new(v.Major, v.Minor, v.Build - 8517);
        WriteString($"{GetBuildID(version)}{Environment.NewLine}");
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