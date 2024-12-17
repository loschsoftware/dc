using Antlr4.Runtime.Tree;
using Dassie.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dassie.Extensions;

internal static class ExtensionLoader
{
    public static readonly string DefaultExtensionSource = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Extensions")).FullName;
    public static readonly string GlobalToolsPath = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Tools")).FullName;

    private static readonly List<IPackage> _installedExtensions = LoadInstalledExtensions();
    public static List<IPackage> InstalledExtensions => _installedExtensions;

    public static IEnumerable<IConfigurationProvider> ConfigurationProviders => InstalledExtensions.Select(p => p.ConfigurationProviders()).SelectMany(p => p);
    public static IEnumerable<IAnalyzer<IParseTree>> CodeAnalyzers => InstalledExtensions.Select(a => a.CodeAnalyzers()).SelectMany(a => a);

    private static List<IPackage> LoadInstalledExtensions()
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

        if (File.Exists(Path.Combine(DefaultExtensionSource, "RenameList.txt")))
        {
            foreach (string renamePattern in File.ReadAllLines(Path.Combine(DefaultExtensionSource, "RenameList.txt")))
            {
                string oldPath = renamePattern.Split("==>")[0];
                string newPath = renamePattern.Split("==>")[1];

                if (File.Exists(oldPath))
                    File.Move(oldPath, newPath);
            }

            File.Delete(Path.Combine(DefaultExtensionSource, "RenameList.txt"));
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

        try
        {
            Type[] packageTypes = extensionAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IPackage))).ToArray();

            foreach (Type t in packageTypes)
                packages.Add((IPackage)Activator.CreateInstance(t));
        }
        catch (ReflectionTypeLoadException)
        {
            EmitWarningMessage(0, 0, 0,
                DS0123_InvalidExtensionPackage,
                $"Extension package '{extensionAssembly.GetName().Name}' is malformed and will be ignored.",
                "dc");
        }

        return packages;
    }

    public static List<ICompilerCommand> GetAllCommands(List<IPackage> packages)
    {
        List<ICompilerCommand> commands = [];

        foreach (IPackage package in packages)
        {
            var cmds = GetCommands(package);

            foreach (ICompilerCommand cmd in cmds)
            {
                if (commands.Any(c => c.Command == cmd.Command || c.Aliases().Intersect(cmd.Aliases()).Any()))
                {
                    StringBuilder errMsg = new();
                    errMsg.Append($"Ambiguous command: The command '{cmd.Command}' is defined by multiple extensions. ");
                    errMsg.AppendLine($"The command defined in '{package.Metadata.Name}, version {package.Metadata.Version}' will be used.");

                    EmitWarningMessage(
                        0, 0, 0,
                        DS0099_DuplicateCompilerCommand,
                        errMsg.ToString(),
                        "dc");
                }

                commands.Add(cmd);
            }
        }

        return commands;
    }

    public static List<ICompilerCommand> GetCommands(IPackage package)
    {
        List<ICompilerCommand> commands = [];

        foreach (var command in package.Commands)
        {
            PropertyInfo instanceProp = command.GetProperty("Instance");
            ICompilerCommand instance = null;

            if (instanceProp != null)
                instance = (ICompilerCommand)instanceProp.GetValue(null);

            commands.Add(instance ?? (ICompilerCommand)Activator.CreateInstance(command));
        }

        return commands;
    }

    public static bool TryGetAnalyzer(string name, out IAnalyzer<IParseTree> analyzer)
    {
        if (CodeAnalyzers.Any(a => a.Name == name))
        {
            analyzer = CodeAnalyzers.First(a => a.Name == name);
            return true;
        }

        analyzer = null;
        return false;
    }
}