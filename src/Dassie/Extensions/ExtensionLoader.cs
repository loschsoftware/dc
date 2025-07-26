using Antlr4.Runtime.Tree;
using Dassie.CodeAnalysis;
using Dassie.Errors.Devices;
using Dassie.Meta.Directives;
using Dassie.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Dassie.Extensions;

internal static class ExtensionLoader
{
    public static readonly string DefaultExtensionSource = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Extensions")).FullName;
    public static readonly string GlobalToolsPath = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Tools")).FullName;

    private static readonly List<IPackage> _installedExtensions = LoadInstalledExtensions();
    public static List<IPackage> InstalledExtensions => _installedExtensions;

    public static IEnumerable<IConfigurationProvider> ConfigurationProviders => InstalledExtensions.Select(p => p.ConfigurationProviders()).SelectMany(p => p);
    public static IEnumerable<IAnalyzer<IParseTree>> CodeAnalyzers => InstalledExtensions.Select(a => a.CodeAnalyzers()).SelectMany(a => a);

    public static IEnumerable<IBuildLogDevice> BuildLogDevices => InstalledExtensions.Select(a => a.BuildLogDevices()).SelectMany(a => a).Append(TextWriterBuildLogDevice.Instance).Append(new FileBuildLogDevice());

    public static IEnumerable<IProjectTemplate> ProjectTemplates => InstalledExtensions.Select(p => p.ProjectTemplates()).SelectMany(a => a).Append(LibraryProject.Instance).Append(ConsoleProject.Instance);

    public static IEnumerable<ICompilerDirective> CompilerDirectives => InstalledExtensions.Select(p => p.CompilerDirectives()).SelectMany(a => a)
        .Append(LineDirective.Instance)
        .Append(SourceDirective.Instance)
        .Append(TodoDirective.Instance)
        .Append(ILDirective.Instance)
        .Append(ImportDirective.Instance);

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

        ActivateBuildLogWriters(packages);
        return packages;
    }

    public static void LoadTransientExtensions(IEnumerable<(string, List<XmlAttribute>, List<XmlElement>)> paths)
    {
        foreach ((string path, List<XmlAttribute> attribs, List<XmlElement> elems) in paths)
        {
            EmitBuildLogMessage($"Loaded transient extension '{path}'.", 2, true);
            _installedExtensions.AddRange(LoadInstalledExtensions(path, true, attribs, elems));
        }
    }

    public static List<IPackage> LoadInstalledExtensions(string assembly)
        => LoadInstalledExtensions(assembly, false, null, null);

    private static List<IPackage> LoadInstalledExtensions(string assembly, bool loadTransient, List<XmlAttribute> xmlAttributes, List<XmlElement> xmlElements)
    {
        if (!File.Exists(assembly))
        {
            EmitErrorMessage(0, 0, 0,
                DS0221_ExtensionFileNotFound,
                $"Extension package '{assembly}' could not be found.",
                "dc");

            return [];
        }

        List<IPackage> packages = [];
        Assembly extensionAssembly = Assembly.LoadFile(assembly);

        try
        {
            Type[] packageTypes = extensionAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IPackage))).ToArray();

            foreach (Type t in packageTypes)
            {
                IPackage package = (IPackage)Activator.CreateInstance(t);
                int ret = -1;

                if (loadTransient && !package.Modes().HasFlag(ExtensionModes.Transient))
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0222_ExtensionUnsupportedMode,
                        $"The extension '{package.Metadata.Name}' cannot be loaded in transient mode.",
                        "dc");

                    continue;
                }

                if (!loadTransient && !package.Modes().HasFlag(ExtensionModes.Global))
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0222_ExtensionUnsupportedMode,
                        $"The extension '{package.Metadata.Name}' cannot be loaded in global mode.",
                        "dc");

                    continue;
                }

                try
                {
                    if (loadTransient)
                        ret = (int)typeof(IPackage).GetMethod("InitializeTransient").Invoke(package, [xmlAttributes, xmlElements]);
                    else
                        ret = (int)typeof(IPackage).GetMethod("InitializeGlobal").Invoke(package, []);
                }
                catch (Exception ex)
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0223_ExtensionInitializerFailed,
                        $"The extension initializer of '{package.Metadata.Name}' threw an exception.",
                        "dc");

                    if (Context.Configuration.PrintExceptionInfo)
                        TextWriterBuildLogDevice.ErrorOut.WriteLine(ex.ToString());

                    continue;
                }

                if (ret != 0)
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0223_ExtensionInitializerFailed,
                        $"The extension initializer of '{package.Metadata.Name}' exited with a nonzero status code.",
                        "dc");

                    continue;
                }

                if (loadTransient && _installedExtensions.Any(p => p.Metadata.Id == package.Metadata.Id))
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0224_ExtensionDuplicateMode,
                        $"Extension '{package.Metadata.Name}' was loaded twice in different modes. Global mode will be unloaded.",
                        "dc");

                    IPackage duplicate = _installedExtensions.First(p => p.Metadata.Id == package.Metadata.Id);
                    Unload(duplicate);
                    _installedExtensions.Remove(duplicate);
                }

                if (!loadTransient)
                    EmitBuildLogMessage($"Loaded extension '{package.Metadata.Name}'.", 2, true);

                packages.Add(package);
            }
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

    public static void Unload(IPackage package)
    {
        try
        {
            package?.Unload();
        }
        catch (Exception ex)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0223_ExtensionInitializerFailed,
                $"Finalizer of extension '{package.Metadata.Name}' threw an exception.",
                "dc");

            if (Context.Configuration.PrintExceptionInfo)
                TextWriterBuildLogDevice.ErrorOut.WriteLine(ex.ToString());
        }

        EmitBuildLogMessage($"Unloaded extension '{package.Metadata.Name}'.", 2);
    }

    public static void UnloadAll()
    {
        foreach (IPackage package in InstalledExtensions)
            Unload(package);

        _installedExtensions.Clear();
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

    public static void ActivateBuildLogWriters(List<IPackage> packages)
    {
        if (packages == null)
            return;

        foreach (IBuildLogWriter writers in packages.Select(p => p.BuildLogWriters()).SelectMany(b => b))
        {
            if (writers.Severity.HasFlag(BuildLogSeverity.Message))
                TextWriterBuildLogDevice.InfoOut.AddWriters(writers.Writers);

            if (writers.Severity.HasFlag(BuildLogSeverity.Warning))
                TextWriterBuildLogDevice.InfoOut.AddWriters(writers.Writers);

            if (writers.Severity.HasFlag(BuildLogSeverity.Error))
                TextWriterBuildLogDevice.ErrorOut.AddWriters(writers.Writers);
        }
    }
}