﻿using Dassie.CLI;
using Dassie.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dassie.Extensions;

internal static class ExtensionManagerCommandLine
{
    public static int HandleArgs(string[] args)
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
            return Import(args[1]);

        if (command == "remove" && args.Length > 1)
            return Remove(args[1]);

        if (command == "update" && args.Length > 1)
            return Update(args[1]);

        return ShowUsage();
    }

    private static int List()
    {
        List<IPackage> packages = ExtensionLoader.LoadInstalledExtensions();

        if (packages.Count == 0)
        {
            Console.WriteLine("No extensions installed.");
            return 0;
        }

        Program.DisplayLogo();
        Console.WriteLine();
        Console.WriteLine("Installed extensions:");
        Console.WriteLine();

        Console.WriteLine($"{"Name",-50}Version");
        foreach (IPackage package in packages)
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
        List<IPackage> packages = ExtensionLoader.LoadInstalledExtensions();

        if (!packages.Any(p => p.Metadata.Name == name))
        {
            Console.WriteLine("The specified extension could not be found.");
            return -1;
        }

        IPackage package = packages.First(p => p.Metadata.Name == name);

        Program.DisplayLogo();
        Console.WriteLine();
        Console.WriteLine("Extension info:");
        Console.WriteLine();

        Console.WriteLine($"{"Name:",-30}{package.Metadata.Name}");
        Console.WriteLine($"{"Description:",-30}{package.Metadata.Description}");
        Console.WriteLine($"{"Author:",-30}{package.Metadata.Author}");
        Console.WriteLine($"{"Version:",-30}{package.Metadata.Version}");
        Console.WriteLine($"{"File:",-30}{package.GetType().Assembly.Location}");

        Console.WriteLine();
        Console.WriteLine("Defined commands:");

        foreach (KeyValuePair<string, string> command in ExtensionLoader.GetCommandDescriptions([package]))
            Console.WriteLine($"{$"{command.Key}",-30}{command.Value}");

        return 0;
    }

    private static int Install(string name)
    {
        throw new NotImplementedException("Installing and updating packages from the internet is not yet implemented.");
    }

    private static int Import(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine("The specified extension file could not be found.");
            return -1;
        }

        File.Copy(path, Path.Combine(ExtensionLoader.DefaultExtensionSource, Path.GetFileName(path)));
        return 0;
    }

    private static int Remove(string name)
    {
        List<IPackage> installed = ExtensionLoader.LoadInstalledExtensions();

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
        sb.Append($"{"    list",-35}{Program.FormatLines("Displays a list of all installed extensions.", indentWidth: 35)}");
        sb.Append($"{"    info <Name>",-35}{Program.FormatLines("Displays advanced information about the specified extension.", indentWidth: 35)}");
        sb.Append($"{"    install <Name>",-35}{Program.FormatLines("Installs the specified extension from the package repository.", indentWidth: 35)}");
        sb.Append($"{"    import <Path>",-35}{Program.FormatLines("Installs an extension from the specified file path.", indentWidth: 35)}");
        sb.Append($"{"    remove <Name>",-35}{Program.FormatLines("Uninstalls the specified extension package.", indentWidth: 35)}");
        sb.Append($"{"    update <Name>",-35}{Program.FormatLines("Updates the specified extension to the newest version.", indentWidth: 35)}");
        sb.Append($"{"    help",-35}{Program.FormatLines("Shows this list.", indentWidth: 35)}");

        Program.DisplayLogo();
        Console.Write(sb.ToString());
        return 0;
    }
}