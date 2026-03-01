using Dassie.Cli;
using Dassie.Extensions;
using Dassie.Extensions.Web;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Converters;
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

    public override string Description => StringHelper.PackageCommand_Description;

    public override CommandHelpDetails HelpDetails => GetHelpDetails();
    private static CommandHelpDetails GetHelpDetails()
    {
        StringBuilder commandsSb = new();
        commandsSb.Append($"{"    list",-35}{HelpCommand.FormatLines(StringHelper.PackageCommand_ListDescription, indentWidth: 35)}");
        commandsSb.Append($"{"    info <Name>",-35}{HelpCommand.FormatLines(StringHelper.PackageCommand_InfoDescription, indentWidth: 35)}");
        commandsSb.Append($"{"    install <Name> [-g]",-35}{HelpCommand.FormatLines(StringHelper.PackageCommand_InstallDescription, indentWidth: 35)}");
        commandsSb.Append($"{"    import <Path> [-o] [-g]",-35}{HelpCommand.FormatLines(StringHelper.PackageCommand_ImportDescription, indentWidth: 35)}");
        commandsSb.Append($"{"    remove <Name>",-35}{HelpCommand.FormatLines(StringHelper.PackageCommand_RemoveDescription, indentWidth: 35)}");
        commandsSb.Append($"{"    update <Name>",-35}{HelpCommand.FormatLines(StringHelper.PackageCommand_UpdateDescription, indentWidth: 35)}");

        return new()
        {
            Description = StringHelper.PackageCommand_HelpDetailsDescription,
            Usage = ["dc package [Command] [Options]"],
            Options =
            [
                ("Command", StringHelper.PackageCommand_CommandOption),
                ("Options", StringHelper.PackageCommand_OptionsOption)
            ],
            CustomSections =
            [
                (StringHelper.PackageCommand_AvailableCommands, commandsSb.ToString())
            ],
            Examples =
            [
                ("dc package list", StringHelper.PackageCommand_Example1),
                ("dc package info MyExtension", StringHelper.PackageCommand_Example2),
                ("dc package install MyExtension", StringHelper.PackageCommand_Example3),
                ("dc package import ./extension.dll", StringHelper.PackageCommand_Example4),
                ("dc package remove MyExtension", StringHelper.PackageCommand_Example5)
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

        int corePackageIndex = packages.IndexOf(CorePackage.Instance);
        packages.RemoveAt(corePackageIndex);
        packages.Insert(0, CorePackage.Instance);

        if (packages.Count == 0)
        {
            Console.WriteLine(StringHelper.PackageCommand_NoExtensionsInstalled);
            return 0;
        }

        HelpCommand.DisplayLogo();
        Console.WriteLine();
        Console.WriteLine(StringHelper.PackageCommand_InstalledModulesAndExtensions);
        Console.WriteLine();

        string header = $"{$"\e[1m{StringHelper.PackageCommand_Name}\e[22m",-59}\e[1m{StringHelper.PackageCommand_Version}\e[22m";
        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length - 18));

        foreach (IPackage package in packages)
        {
            string packageDisplay = $"{(package == CorePackage.Instance ? $"\e[1;31m[{StringHelper.PackageCommand_BuiltIn}]\e[0m " : "")}{package.Metadata.Name}";
            if (packageDisplay.Length > 45)
                packageDisplay = packageDisplay[0..45] + "...";

            Console.WriteLine($"{packageDisplay,-50}{(package == CorePackage.Instance ? new string(' ', 11) : "")}{package.Metadata.Version}");
        }

        return 0;
    }

    private static int Info(string name)
    {
        StringBuilder sb = new();
        List<IPackage> packages = ExtensionLoader.InstalledExtensions.Where(p => !p.Hidden()).ToList();

        if (!packages.Any(p => p.Metadata.Name == name))
        {
            Console.WriteLine(StringHelper.PackageCommand_SpecifiedExtensionNotFound);
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
        WriteHeading(StringHelper.PackageCommand_Details);

        sb.AppendLine($"{$"    {StringHelper.PackageCommand_NameColon}",-50}{package.Metadata.Name}");
        sb.AppendLine($"{$"    {StringHelper.PackageCommand_DescriptionColon}",-50}{package.Metadata.Description}");
        sb.AppendLine($"{$"    {StringHelper.PackageCommand_Author}",-50}{package.Metadata.Author}");
        sb.AppendLine($"{$"    {StringHelper.PackageCommand_VersionColon}",-50}{package.Metadata.Version}");
        sb.AppendLine($"{$"    {StringHelper.PackageCommand_File}",-50}{package.GetType().Assembly.Location}");

        IEnumerable<ICompilerCommand> definedCommands = ExtensionLoader.Commands.Where(c => !c.Options.HasFlag(CommandOptions.Hidden) && c.GetType().Assembly == package.GetType().Assembly);
        PrintFeatures(StringHelper.PackageCommand_Commands, definedCommands.Select(cmd => $"{$"{cmd.Command}",-46}{cmd.Description}"));
        PrintFeatures(StringHelper.PackageCommand_CodeAnalyzers, package.CodeAnalyzers().Select(a => a.Name));
        PrintFeatures(StringHelper.PackageCommand_ConfigurationProviders, package.ConfigurationProviders().Select(p => p.Name));
        PrintFeatures(StringHelper.PackageCommand_ProjectTemplates, package.ProjectTemplates().Select(t => t.Name));
        PrintFeatures(StringHelper.PackageCommand_BuildLogDevices, package.BuildLogDevices().Select(d => d.Name));
        PrintFeatures(StringHelper.PackageCommand_CompilerDirectives, package.CompilerDirectives().Select(d => d.Identifier));
        PrintFeatures(StringHelper.PackageCommand_DocumentSources, package.DocumentSources().Select(s => $"{s.Name}: '{s.DocumentName}'"));
        PrintFeatures(StringHelper.PackageCommand_DeploymentTargets, package.DeploymentTargets().Select(t => t.Name));
        PrintFeatures(StringHelper.PackageCommand_Subsystems, package.Subsystems().Select(s => s.Name));
        PrintFeatures(StringHelper.PackageCommand_BuildActions, package.BuildActions().Select(b => b.Name));
        PrintFeatures(StringHelper.PackageCommand_Macros, package.Macros().Select(b => b.Macro));
        PrintFeatures(StringHelper.PackageCommand_LocalizationResourceProviders, package.LocalizationResourceProviders().Select(b => b.Culture));

        HelpCommand.DisplayLogo();
        Console.WriteLine(sb.ToString());
        return 0;
    }

    private static int Install(string name)
    {
        List<ExtensionMetadata> extensions = ExtensionDownloader.GetPackagesAsync(name).Result;

        if (extensions == null || extensions.Count == 0)
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0227_PackageInstallNotFound,
                nameof(StringHelper.PackageCommand_ExtensionNotFound), [name],
                CompilerExecutableName);

            return -1;
        }

        (byte[] dataBytes, string fileName) = ExtensionDownloader.DownloadExtension(extensions[0]).Result;
        string path = Path.Combine(ExtensionLoader.DefaultExtensionSource, fileName);

        if (File.Exists(path))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0229_PackageInstallAlreadyInstalled,
                nameof(StringHelper.PackageCommand_ExtensionAlreadyInstalled), [name],
                CompilerExecutableName);

            return -1;
        }

        File.WriteAllBytes(path, dataBytes);
        EmitMessageFormatted(0, 0, 0, DS0000_Success, nameof(StringHelper.PackageCommand_InstallationSuccessful), [name, extensions[0].Metadata.Version], CompilerExecutableName);
        return 0;
    }

    private static int Import(string path, bool overwrite = false, bool globalTool = false)
    {
        if (globalTool)
            return InstallGlobalTool(path);

        if (!File.Exists(path))
        {
            Console.WriteLine(StringHelper.PackageCommand_SpecifiedExtensionFileNotFound);
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
                Console.WriteLine(StringHelper.PackageCommand_ExtensionFileAlreadyInstalled);
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
            Console.WriteLine(StringHelper.PackageCommand_ToolPathNotFound);
            return -1;
        }

        string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

        if (!currentPath.Split(Path.PathSeparator).Contains(toolDir))
            Environment.SetEnvironmentVariable("PATH", $"{currentPath}{Path.PathSeparator}{toolDir}", EnvironmentVariableTarget.User);

        return 0;
    }

    private static int Remove(string name)
    {
        if (name == CorePackage.Instance.Metadata.Name)
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0265_CorePackageRemoved,
                nameof(StringHelper.PackageCommand_BuiltInModuleCannotBeUninstalled), [CorePackage.Instance.Metadata.Name],
                CompilerExecutableName);

            return -1;
        }

        List<IPackage> installed = ExtensionLoader.InstalledExtensions.Where(p => !p.Hidden()).ToList();

        if (!installed.Any(p => p.Metadata.Name == name))
        {
            Console.WriteLine(StringHelper.PackageCommand_SpecifiedExtensionNotFound);
            return -1;
        }

        Assembly packageAssembly = installed.First(p => p.Metadata.Name == name).GetType().Assembly;
        string path = packageAssembly.Location;

        if (packageAssembly.DefinedTypes.Where(t => t.GetInterfaces().Contains(typeof(IPackage))).Count() > 1)
        {
            Console.WriteLine(StringHelper.Format(nameof(StringHelper.PackageCommand_ExtensionRemoveWarningLine1), name));

            foreach (var ext in ExtensionLoader.LoadInstalledExtensions(path))
                Console.WriteLine($"    - {ext.Metadata.Name}");

            Console.WriteLine();
            Console.WriteLine(StringHelper.PackageCommand_ExtensionRemoveWarningLine2);

            if (char.ToLower(Console.ReadKey().KeyChar) == 'n')
                return 0;
        }

        File.AppendAllText(Path.Combine(ExtensionLoader.DefaultExtensionSource, "RemovalList.txt"), $"{path}{Environment.NewLine}");
        return 0;
    }

    private static int Update(string name)
    {
        EmitErrorMessageFormatted(
            0, 0, 0,
            DS0064_UnsupportedFeature,
            nameof(StringHelper.PackageCommand_CommandNotImplemented), [],
            CompilerExecutableName);

        return -1;
    }

    private static int ShowUsage() => HelpCommand.Instance.Invoke(["package"]);
}