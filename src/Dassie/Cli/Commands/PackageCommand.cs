﻿using Antlr4.Runtime.Tree;
using Dassie.CodeAnalysis;
using Dassie.Extensions;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

#pragma warning disable IDE0079
#pragma warning disable IL3000

namespace Dassie.Cli.Commands;

internal class PackageCommand : ICompilerCommand
{
    private static PackageCommand _instance;
    public static PackageCommand Instance => _instance ??= new();

    public string Command => "package";

    public string UsageString => "package [Command] [Options]";

    public string Description => "Used to install and manage compiler extensions. Use 'dc package help' to display available commands.";

    public int Invoke(string[] args)
    {
        args ??= [];

        if (args.Length == 0)
            args = ["help"];

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

        if (command == "source")
            return ExtensionSourceManager.HandleArgs(args);

        return ShowUsage();
    }

    private static int List()
    {
        List<IPackage> packages = ExtensionLoader.InstalledExtensions;

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
        List<IPackage> packages = ExtensionLoader.InstalledExtensions;

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

        sb.AppendLine();
        WriteHeading("Details");

        sb.AppendLine($"{"    Name:",-50}{package.Metadata.Name}");
        sb.AppendLine($"{"    Description:",-50}{package.Metadata.Description}");
        sb.AppendLine($"{"    Author:",-50}{package.Metadata.Author}");
        sb.AppendLine($"{"    Version:",-50}{package.Metadata.Version}");
        sb.AppendLine($"{"    File:",-50}{package.GetType().Assembly.Location}");

        IEnumerable<ICompilerCommand> definedCommands = CommandRegistry.Commands.Where(c => c.GetType().Assembly == package.GetType().Assembly);

        if (definedCommands.Any())
        {
            sb.AppendLine();
            WriteHeading("Commands");

            foreach (ICompilerCommand cmd in definedCommands)
                sb.AppendLine($"{$"    {cmd.UsageString}",-50}{cmd.Description}");
        }

        if (package.CodeAnalyzers().Length != 0)
        {
            sb.AppendLine();
            WriteHeading("Code analyzers");

            foreach (IAnalyzer<IParseTree> analyzer in package.CodeAnalyzers())
                sb.AppendLine($"    {analyzer.Name}");
        }

        if (package.ConfigurationProviders().Length != 0)
        {
            sb.AppendLine();
            WriteHeading("Configuration providers");

            foreach (IConfigurationProvider provider in package.ConfigurationProviders())
                sb.AppendLine($"    {provider.Name}");
        }

        if (package.ProjectTemplates().Length != 0)
        {
            sb.AppendLine();
            WriteHeading("Project templates");

            foreach (IProjectTemplate template in package.ProjectTemplates())
                sb.AppendLine($"    {template.Name}");
        }

        if (package.BuildLogDevices().Length != 0)
        {
            sb.AppendLine();
            WriteHeading("Build log devices");

            foreach (IBuildLogDevice device in package.BuildLogDevices())
                sb.AppendLine($"    {device.Name}");
        }

        HelpCommand.DisplayLogo();
        Console.WriteLine(sb.ToString());
        return 0;
    }

    private static int Install(string name)
    {
        EmitErrorMessage(
            0, 0, 0,
            DS0063_UnsupportedFeature,
            "Installing and updating packages from the internet is not yet supported.",
            "dc");

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
        List<IPackage> installed = ExtensionLoader.InstalledExtensions;

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
        throw new NotImplementedException("Installing and updating packages from the internet is not yet implemented.");
    }

    private static int ShowUsage()
    {
        StringBuilder sb = new();

        sb.AppendLine();
        sb.AppendLine("Usage: dc package [Command] [Options]");

        sb.AppendLine();
        sb.AppendLine("Available commands:");
        sb.Append($"{"    list",-35}{HelpCommand.FormatLines("Displays a list of all installed extensions.", indentWidth: 35)}");
        sb.Append($"{"    info <Name>",-35}{HelpCommand.FormatLines("Displays advanced information about the specified extension.", indentWidth: 35)}");
        sb.Append($"{"    install <Name> [-g]",-35}{HelpCommand.FormatLines("Installs the specified extension from the package repository. Use the -g flag to install the package as a globally accessible tool.", indentWidth: 35)}");
        sb.Append($"{"    import <Path> [-o] [-g]",-35}{HelpCommand.FormatLines("Installs an extension from the specified file path. Use the -o flag to overwrite existing extensions.", indentWidth: 35)}");
        sb.Append($"{"    remove <Name>",-35}{HelpCommand.FormatLines("Uninstalls the specified extension package.", indentWidth: 35)}");
        sb.Append($"{"    update <Name>",-35}{HelpCommand.FormatLines("Updates the specified extension to the newest version.", indentWidth: 35)}");
        sb.Append($"{"    source [Command] [Options]",-35}{HelpCommand.FormatLines("Manages extension sources. Use 'dc package source help' for a list of commands.", indentWidth: 35)}");
        sb.Append($"{"    help",-35}{HelpCommand.FormatLines("Shows this list.", indentWidth: 35)}");

        HelpCommand.DisplayLogo();
        Console.Write(sb.ToString());
        return 0;
    }
}