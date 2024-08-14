using Dassie.Configuration;
using Dassie.Meta;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Dassie.Unmanaged;

internal static class WinSdkHelper
{
    public static string GetFrameworkToolPath(string toolName, string reason)
    {
        GlobalConfig.ExternalToolPaths.Tools ??= [];
        if (GlobalConfig.ExternalToolPaths.Tools.Any(t => t.Name == toolName))
            return GlobalConfig.ExternalToolPaths.Tools.First(t => t.Name == toolName).Path;

        string path = ToolLocationHelper.GetPathToDotNetFrameworkSdkFile(toolName);
        if (File.Exists(path))
            return path;

        Console.Write($"The operation '{reason}' requires the external tool '{toolName}'. To enable this feature, locate this file on your system and enter its file path: ");
        path = Console.ReadLine().Replace("\"", "");

        if (!File.Exists(path))
        {
            Console.WriteLine();
            Console.WriteLine("Operation failed: File does not exist or insufficient permissions.");
            return null;
        }

        GlobalConfig.ExternalToolPaths.Tools = GlobalConfig.ExternalToolPaths.Tools.Append(new()
        {
            Name = toolName,
            Path = path
        }).ToArray();

        StreamWriter sw = new(ToolPaths.ToolPathsFile);
        XmlSerializer xmls = new(typeof(ToolPaths));
        xmls.Serialize(sw, GlobalConfig.ExternalToolPaths);
        sw.Dispose();

        return GetFrameworkToolPath(toolName, reason);
    }

    public static string GetDirectoryPath(string toolName, string prompt)
    {
        GlobalConfig.ExternalToolPaths.Tools ??= [];
        if (GlobalConfig.ExternalToolPaths.Tools.Any(t => t.Name == toolName))
            return GlobalConfig.ExternalToolPaths.Tools.First(t => t.Name == toolName).Path;

        string path = "";

        Console.Write(prompt);
        path = Console.ReadLine();

        GlobalConfig.ExternalToolPaths.Tools = GlobalConfig.ExternalToolPaths.Tools.Append(new()
        {
            Name = toolName,
            Path = path
        }).ToArray();

        StreamWriter sw = new(ToolPaths.ToolPathsFile);
        XmlSerializer xmls = new(typeof(ToolPaths));
        xmls.Serialize(sw, GlobalConfig.ExternalToolPaths);
        sw.Dispose();

        return GetFrameworkToolPath(toolName, prompt);
    }

    public static string GetToolPath(string tool)
    {
        string winSdkBaseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Kits", "10", "bin");

        if (!Directory.Exists(winSdkBaseDir))
            return "";

        string[] files = Directory.GetFiles(winSdkBaseDir, tool, SearchOption.AllDirectories);

        // TODO: This is an exceptionally shitty implementation

        string file = "";
        if (files.Length > 1)
            file = files[1]; // Skips ARM version

        return file;
    }
}