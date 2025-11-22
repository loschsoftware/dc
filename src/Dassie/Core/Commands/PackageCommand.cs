using Antlr4.Runtime.Tree;
using Dassie.Cli;
using Dassie.CodeAnalysis;
using Dassie.Extensions;
using Dassie.Extensions.Web;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

#pragma warning disable IDE0079
#pragma warning disable IL3000

namespace Dassie.Core.Commands;

internal class PackageCommand : CompilerCommand
{
    private static PackageCommand _instance;
    public static PackageCommand Instance => _instance ??= new();

    public override string Command => "package";

    public override string Description => "Manages compiler extensions.";

    public override CommandHelpDetails HelpDetails => GetHelpDetails();
    private static CommandHelpDetails GetHelpDetails()
    {
        StringBuilder commandsSb = new();
        commandsSb.Append($"{"    list",-35}{HelpCommand.FormatLines("Displays a list of all installed extensions.", indentWidth: 35)}");
        commandsSb.Append($"{"    info <Name>",-35}{HelpCommand.FormatLines("Displays advanced information about the specified extension.", indentWidth: 35)}");
        commandsSb.Append($"{"    install <Name> [-g]",-35}{HelpCommand.FormatLines("Installs the specified extension from the package repository. Use the -g flag to install the package as a globally accessible tool.", indentWidth: 35)}");
        commandsSb.Append($"{"    import <Path> [-o] [-g]",-35}{HelpCommand.FormatLines("Installs an extension from the specified file path. Use the -o flag to overwrite existing extensions.", indentWidth: 35)}");
        commandsSb.Append($"{"    remove <Name>",-35}{HelpCommand.FormatLines("Uninstalls the specified extension package.", indentWidth: 35)}");
        commandsSb.Append($"{"    update <Name>",-35}{HelpCommand.FormatLines("Updates the specified extension to the newest version.", indentWidth: 35)}");

        return new()
        {
            Description = "Used to install and manage compiler extensions.",
            Usage = ["dc package [Command] [Options]"],
            Options =
            [
                ("Command", "The subcommand to execute."),
                ("Options", "Additional options passed to the subcommand.")
            ],
            CustomSections =
            [
                ("Available commands", commandsSb.ToString())
            ],
            Examples =
            [
                ("dc package list", "Displays a list of installed extensions."),
                ("dc package info MyExtension", "Displays information about the extension 'MyExtension'."),
                ("dc package install MyExtension", "Installs the extension 'MyExtension' from the package repository."),
                ("dc package import ./extension.dll", "Installs an extension from the specified file path."),
                ("dc package remove MyExtension", "Uninstalls the extension 'MyExtension'.")
            ]
        };
    }

    public override int Invoke(string[] args)
    {
        args ??= [];

        if (args.Length == 0)
            args = ["help"];

        if (args.Any(a => a.StartsWith("--source=")))
            ExtensionDownloader.Source = args.First(a => a.StartsWith("--source="))[9..];

        string command = args[0];

        if (command == "list")
            return List();

        if (command == "info" && args.Length > 1)
            return Info(args[1]);

        if (command == "install" && args.Length > 1)
            return Install(args[1]);

        if (command == "import" && args.Length > 1)
        {
            bool overwrite = false, globalTool = false;

            if (args.Length > 2 && args.Contains("-o"))
                overwrite = true;

            if (args.Length > 2 && args.Contains("-g"))
                globalTool = true;

            return Import(args[1], overwrite, globalTool);
        }

        if (command == "remove" && args.Length > 1)
            return Remove(args[1]);

        if (command == "update" && args.Length > 1)
            return Update(args[1]);

        return ShowUsage();
    }

    private static int List()
    {
        List<IPackage> packages = ExtensionLoader.InstalledExtensions.Where(p => !p.Hidden()).ToList();

        if (packages.Count == 0)
        {
            Console.WriteLine("No extensions installed.");
            return 0;
        }

        HelpCommand.DisplayLogo();
        Console.WriteLine();
        Console.WriteLine("Installed extensions:");
        Console.WriteLine();

        string header = $"{"\e[1mName\e[22m",-59}\e[1mVersion\e[22m";
        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length - 18));

        foreach (IPackage package in packages.Where(p => p.Metadata.Name != "Default"))
        {
            string packageDisplay = package.Metadata.Name;
            if (packageDisplay.Length > 45)
                packageDisplay = packageDisplay[0..45] + "...";

            Console.WriteLine($"{packageDisplay,-50}{package.Metadata.Version}");
        }

        return 0;
    }

    private static int Info(string name)
    {
        StringBuilder sb = new();
        List<IPackage> packages = ExtensionLoader.InstalledExtensions.Where(p => !p.Hidden()).ToList();

        if (!packages.Any(p => p.Metadata.Name == name))
        {
            Console.WriteLine("The specified extension could not be found.");
            return -1;
        }

        IPackage package = packages.First(p => p.Metadata.Name == name);

        void SetUnderline()
        {
            if (!ConsoleHelper.AnsiEscapeSequenceSupported)
                return;

            sb.Append("\e[4m\e[1m");
        }

        void ResetUnderline()
        {
            if (!ConsoleHelper.AnsiEscapeSequenceSupported)
                return;

            sb.Append("\e[24m\e[22m");
        }

        void WriteHeading(string text)
        {
            SetUnderline();
            sb.Append(text);
            ResetUnderline();
            sb.AppendLine(":");
        }

        void PrintFeatures(string category, IEnumerable<string> values)
        {
            if (values == null || values.Count() == 0)
                return;

            sb.AppendLine();
            WriteHeading(category);

            foreach (string feature in values)
                sb.AppendLine($"    {feature}");
        }

        sb.AppendLine();
        WriteHeading("Details");

        sb.AppendLine($"{"    Name:",-50}{package.Metadata.Name}");
        sb.AppendLine($"{"    Description:",-50}{package.Metadata.Description}");
        sb.AppendLine($"{"    Author:",-50}{package.Metadata.Author}");
        sb.AppendLine($"{"    Version:",-50}{package.Metadata.Version}");
        sb.AppendLine($"{"    File:",-50}{package.GetType().Assembly.Location}");

        IEnumerable<ICompilerCommand> definedCommands = ExtensionLoader.Commands.Where(c => c.GetType().Assembly == package.GetType().Assembly);
        PrintFeatures("Commands", definedCommands.Select(cmd => $"{$"{cmd.Command}",-46}{cmd.Description}"));
        PrintFeatures("Code analyzers", package.CodeAnalyzers().Select(a => a.Name));
        PrintFeatures("Configuration providers", package.ConfigurationProviders().Select(p => p.Name));
        PrintFeatures("Project templates", package.ProjectTemplates().Select(t => t.Name));
        PrintFeatures("Build log devices", package.BuildLogDevices().Select(d => d.Name));
        PrintFeatures("Compiler directives", package.CompilerDirectives().Select(d => d.Identifier));
        PrintFeatures("Document sources", package.DocumentSources().Select(s => $"{s.Name}: '{s.DocumentName}'"));
        PrintFeatures("Deployment targets", package.DeploymentTargets().Select(t => t.Name));
        PrintFeatures("Subsystems", package.Subsystems().Select(s => s.Name));
        PrintFeatures("Build actions", package.BuildActions().Select(b => b.Name));

        HelpCommand.DisplayLogo();
        Console.WriteLine(sb.ToString());
        return 0;
    }

    private static int Install(string name)
    {
        List<ExtensionMetadata> extensions = ExtensionDownloader.GetPackagesAsync(name).Result;

        if (extensions == null || extensions.Count == 0)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0227_PackageInstallNotFound,
                $"The extension '{name}' could not be found.",
                CompilerExecutableName);

            return -1;
        }

        (byte[] dataBytes, string fileName) = ExtensionDownloader.DownloadExtension(extensions[0]).Result;
        string path = Path.Combine(ExtensionLoader.DefaultExtensionSource, fileName);

        if (File.Exists(path))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0229_PackageInstallAlreadyInstalled,
                $"The extension '{name}' is already installed. Use the 'dc package update' command to update it to the newest version.",
                CompilerExecutableName);

            return -1;
        }

        File.WriteAllBytes(path, dataBytes);
        EmitMessage(0, 0, 0, DS0000_Success, $"Successfully installed extension '{name}', version {extensions[0].Metadata.Version}.", CompilerExecutableName);
        return 0;
    }

    private static int Import(string path, bool overwrite = false, bool globalTool = false)
    {
        if (globalTool)
            return InstallGlobalTool(path);

        if (!File.Exists(path))
        {
            Console.WriteLine("The specified extension file could not be found.");
            return -1;
        }

        string dest = Path.Combine(ExtensionLoader.DefaultExtensionSource, Path.GetFileName(path));

        if (File.Exists(dest))
        {
            if (overwrite)
            {
                Guid tempNameGuid = Guid.NewGuid();
                string tempFileName = Path.Combine(Path.GetDirectoryName(dest), $"{tempNameGuid:N}{Path.GetExtension(dest)}");

                File.AppendAllText(Path.Combine(ExtensionLoader.DefaultExtensionSource, "RemovalList.txt"), $"{dest}{Environment.NewLine}");
                File.AppendAllText(Path.Combine(ExtensionLoader.DefaultExtensionSource, "RenameList.txt"), $"{tempFileName}==>{dest}{Environment.NewLine}");
                File.Copy(path, tempFileName);
                return 0;
            }
            else
            {
                Console.WriteLine("The specified extension is already installed. Use the -o flag to overwrite existing extensions.");
                return -1;
            }
        }

        File.Copy(path, dest);
        return 0;
    }

    private static int InstallGlobalTool(string toolPath)
    {
        string toolDir = Directory.CreateDirectory(Path.Combine(ExtensionLoader.GlobalToolsPath, Path.GetFileNameWithoutExtension(toolPath))).FullName;

        if (File.Exists(toolPath))
            File.Copy(toolPath, Path.Combine(toolDir, Path.GetFileName(toolPath)));
        else if (Directory.Exists(toolPath))
            FileSystem.CopyDirectory(toolPath, toolDir, true);
        else
        {
            Console.WriteLine("The specified tool path could not be found.");
            return -1;
        }

        string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

        if (!currentPath.Split(Path.PathSeparator).Contains(toolDir))
            Environment.SetEnvironmentVariable("PATH", $"{currentPath}{Path.PathSeparator}{toolDir}", EnvironmentVariableTarget.User);

        return 0;
    }

    private static int Remove(string name)
    {
        List<IPackage> installed = ExtensionLoader.InstalledExtensions.Where(p => !p.Hidden()).ToList();

        if (!installed.Any(p => p.Metadata.Name == name))
        {
            Console.WriteLine("The specified extension could not be found.");
            return -1;
        }

        Assembly packageAssembly = installed.First(p => p.Metadata.Name == name).GetType().Assembly;
        string path = packageAssembly.Location;

        if (packageAssembly.DefinedTypes.Where(t => t.GetInterfaces().Contains(typeof(IPackage))).Count() > 1)
        {
            Console.WriteLine($"Warning: The extension {name} is located inside of an assembly containing multiple extensions. If you proceed with the removal, the following extensions will be removed:");

            foreach (var ext in ExtensionLoader.LoadInstalledExtensions(path))
                Console.WriteLine($"    - {ext.Metadata.Name}");

            Console.WriteLine();
            Console.WriteLine("Remove above extensions? [Y/N] ");

            if (char.ToLower(Console.ReadKey().KeyChar) == 'n')
                return 0;
        }

        File.AppendAllText(Path.Combine(ExtensionLoader.DefaultExtensionSource, "RemovalList.txt"), $"{path}{Environment.NewLine}");
        return 0;
    }

    private static int Update(string name)
    {
        EmitErrorMessage(
            0, 0, 0,
            DS0064_UnsupportedFeature,
            "This command is not yet implemented.",
            CompilerExecutableName);

        return -1;
    }

    private static int ShowUsage() => HelpCommand.Instance.Invoke(["package"]);
}