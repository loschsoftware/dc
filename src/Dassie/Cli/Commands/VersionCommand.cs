using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Dassie.Cli.Commands;

internal class VersionCommand : ICompilerCommand
{
    private static VersionCommand _instance;
    public static VersionCommand Instance => _instance ??= new();

    public string Command => "--version";

    public List<string> Aliases() => ["-v", "--env", "--environment"];

    public bool Hidden() => true;

    public string UsageString => "";
    public string Description => "";

    public int Invoke(string[] args)
    {
        StringBuilder output = new();

        Version v = Assembly.GetExecutingAssembly().GetName().Version;
        // 8517 -> Days between 01/01/2000 and 27/04/2023, on which development on dc was started
        Version version = new(v.Major, v.Minor, v.Build - 8517);
        DateTime buildDate = new DateTime(2000, 1, 1).AddDays(v.Build);

        int padding = 35;

        output.AppendLine();
        output.AppendLine("Dassie Compiler Command Line");
        output.AppendLine($"(C) 2023-{buildDate.Year} Losch");

        output.AppendLine();
        output.AppendLine("Environment:");
        output.AppendLine($"{"    - Compiler version:".PadRight(padding)}{v}");
        output.AppendLine($"{"    - Build number:".PadRight(padding)}{version.Build}");
        output.AppendLine($"{"    - Compilation date:".PadRight(padding)}{buildDate.ToShortDateString()}");
        output.AppendLine($"{"    - .NET version:".PadRight(padding)}{typeof(object).Assembly.GetName().Version}");
        output.AppendLine($"{"    - Operating system:".PadRight(padding)}{RuntimeInformation.OSDescription}");
        output.AppendLine($"{"    - OS architecture:".PadRight(padding)}{RuntimeInformation.OSArchitecture}");

        output.AppendLine();
        output.AppendLine("Locations:");
        output.AppendLine($"{"    - Extension storage:".PadRight(padding)}\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Extensions")}\"");
        output.AppendLine($"{"    - Global tools storage:".PadRight(padding)}\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Tools")}\"");
        output.AppendLine($"{"    - NuGet package storage:".PadRight(padding)}\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages")}\"");
        output.AppendLine($"{"    - Tool paths file:".PadRight(padding)}\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "tools.xml")}\"");

        InfoOut.Write(output.ToString());
        return 0;
    }
}