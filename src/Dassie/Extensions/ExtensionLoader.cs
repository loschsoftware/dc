using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dassie.Extensions;

internal static class ExtensionLoader
{
    public static readonly string DefaultExtensionSource = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Extensions")).FullName;

    public static List<IPackage> LoadInstalledExtensions()
    {
        if (File.Exists(Path.Combine(DefaultExtensionSource, "RemovalList.txt")))
        {
            foreach (string file in File.ReadAllLines(Path.Combine(DefaultExtensionSource, "RemovalList.txt")))
            {
                if (File.Exists(file))
                    File.Delete(file);
            }

            File.Delete(Path.Combine(DefaultExtensionSource, "RemovalList.txt"));
        }

        List<IPackage> packages = [];

        foreach (string file in Directory.EnumerateFiles(DefaultExtensionSource, "*.dll", SearchOption.AllDirectories))
            packages.AddRange(LoadInstalledExtensions(file));

        return packages;
    }

    public static List<IPackage> LoadInstalledExtensions(string assembly)
    {
        List<IPackage> packages = [];

        Assembly extensionAssembly = Assembly.LoadFile(assembly);
        Type[] packageTypes = extensionAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IPackage))).ToArray();

        foreach (Type t in packageTypes)
            packages.Add((IPackage)Activator.CreateInstance(t));

        return packages;
    }

    public static Dictionary<string, string> GetCommandDescriptions(List<IPackage> packages)
    {
        Dictionary<string, string> commands = [];

        foreach (IPackage package in packages)
        {
            foreach (Type commandType in package.Commands)
            {
                ICompilerCommand cmd = (ICompilerCommand)Activator.CreateInstance(commandType);

                if (!commands.ContainsKey(cmd.UsageString))
                    commands.Add(cmd.UsageString, cmd.Description);
            }
        }

        return commands;
    }

    public static Dictionary<string, Func<string[], int>> GetAllCommands(List<IPackage> packages)
    {
        Dictionary<string, (IPackage package, Func<string[], int> func)> commands = [];

        foreach (IPackage package in packages)
        {
            var cmds = GetCommands(package);

            foreach (KeyValuePair<string, Func<string[], int>> cmd in cmds)
            {
                if (commands.TryGetValue(cmd.Key, out (IPackage package, Func<string[], int> func) value))
                {
                    IPackage prevPackage = value.package;

                    StringBuilder errMsg = new();
                    errMsg.AppendLine($"Ambiguous command: The command '{cmd.Key}' is defined by multiple extensions:");
                    errMsg.AppendLine($"    - {prevPackage.Metadata.Name}, version {prevPackage.Metadata.Version}");
                    errMsg.AppendLine($"    - {package.Metadata.Name}, version {package.Metadata.Version}");
                    errMsg.AppendLine($"The command defined in '{prevPackage.Metadata.Name}, version {prevPackage.Metadata.Version}' will be used.");

                    EmitWarningMessage(
                        0, 0, 0,
                        DS0099_DuplicateCompilerCommand,
                        errMsg.ToString(),
                        "dc");
                }

                else
                    commands.Add(cmd.Key, (package, cmd.Value));
            }
        }

        return commands.Select(c => new KeyValuePair<string, Func<string[], int>>(c.Key, c.Value.func)).ToDictionary();
    }

    public static Dictionary<string, Func<string[], int>> GetCommands(IPackage package)
    {
        Dictionary<string, Func<string[], int>> commands = [];

        foreach (var command in package.Commands)
        {
            ICompilerCommand instance = (ICompilerCommand)Activator.CreateInstance(command);
            commands.Add(instance.Command, instance.Invoke);
        }

        return commands;
    }
}