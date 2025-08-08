using Antlr4.Runtime.Tree;
using Dassie.Cli.Commands;
using Dassie.CodeAnalysis;
using Dassie.Errors.Devices;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Dassie.Extensions;

internal static class ExtensionLoader
{
    static ExtensionLoader()
    {
        InstalledExtensions = [];
        InstalledExtensions.AddRange(LoadInstalledExtensions());
        InstalledExtensions.CollectionChanged += Update;
    }

    public static readonly string DefaultExtensionSource = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Extensions")).FullName;
    public static readonly string GlobalToolsPath = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Tools")).FullName;

    public static ObservableCollection<IPackage> InstalledExtensions
    {
        get => field;
        set
        {
            if (field != null)
                field.CollectionChanged -= Update;

            field = value;
            field.CollectionChanged += Update;
            Update(null, null);
        }
    }

    private static IEnumerable<ICompilerCommand> _commands = [];
    public static IEnumerable<ICompilerCommand> Commands => _commands;

    private static IEnumerable<IConfigurationProvider> _configurationProviders = [];
    public static IEnumerable<IConfigurationProvider> ConfigurationProviders => _configurationProviders;

    private static IEnumerable<IAnalyzer<IParseTree>> _codeAnalyzers = [];
    public static IEnumerable<IAnalyzer<IParseTree>> CodeAnalyzers => _codeAnalyzers;

    private static IEnumerable<IBuildLogDevice> _buildLogDevices = [];
    public static IEnumerable<IBuildLogDevice> BuildLogDevices => _buildLogDevices;

    private static IEnumerable<IProjectTemplate> _projectTemplates = [];
    public static IEnumerable<IProjectTemplate> ProjectTemplates => _projectTemplates;

    private static IEnumerable<ICompilerDirective> _compilerDirectives = [];
    public static IEnumerable<ICompilerDirective> CompilerDirectives => _compilerDirectives;

    private static IEnumerable<IDocumentSource> _documentSources = [];
    public static IEnumerable<IDocumentSource> DocumentSources => _documentSources;

    private static void Update(object sender, NotifyCollectionChangedEventArgs e)
    {
        _commands = InstalledExtensions.SelectMany(p => p.Commands());
        _configurationProviders = InstalledExtensions.SelectMany(p => p.ConfigurationProviders());
        _codeAnalyzers = InstalledExtensions.SelectMany(p => p.CodeAnalyzers());
        _buildLogDevices = InstalledExtensions.SelectMany(p => p.BuildLogDevices());
        _projectTemplates = InstalledExtensions.SelectMany(p => p.ProjectTemplates());
        _compilerDirectives = InstalledExtensions.SelectMany(p => p.CompilerDirectives());
        _documentSources = InstalledExtensions.SelectMany(p => p.DocumentSources());
    }

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

        packages.Add(CorePackage.Instance);
        ActivateBuildLogWriters(packages);
        return packages;
    }

    public static void LoadTransientExtensions(IEnumerable<(string, List<XmlAttribute>, List<XmlElement>)> paths)
    {
        foreach ((string path, List<XmlAttribute> attribs, List<XmlElement> elems) in paths)
        {
            EmitBuildLogMessage($"Loaded transient extension '{path}'.", 2, true);
            InstalledExtensions.AddRange(LoadInstalledExtensions(path, true, attribs, elems));
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
                CompilerExecutableName);

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
                        CompilerExecutableName);

                    continue;
                }

                if (!loadTransient && !package.Modes().HasFlag(ExtensionModes.Global))
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0222_ExtensionUnsupportedMode,
                        $"The extension '{package.Metadata.Name}' cannot be loaded in global mode.",
                        CompilerExecutableName);

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
                        CompilerExecutableName);

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
                        CompilerExecutableName);

                    continue;
                }

                if (loadTransient && InstalledExtensions.Any(p => p.Metadata.Id == package.Metadata.Id))
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0224_ExtensionDuplicateMode,
                        $"Extension '{package.Metadata.Name}' was loaded twice in different modes. Global mode will be unloaded.",
                        CompilerExecutableName);

                    IPackage duplicate = InstalledExtensions.First(p => p.Metadata.Id == package.Metadata.Id);
                    Unload(duplicate);
                    InstalledExtensions.Remove(duplicate);
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
                CompilerExecutableName);
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
                CompilerExecutableName);

            if (Context.Configuration.PrintExceptionInfo)
                TextWriterBuildLogDevice.ErrorOut.WriteLine(ex.ToString());
        }

        EmitBuildLogMessage($"Unloaded extension '{package.Metadata.Name}'.", 2);
    }

    public static void UnloadAll()
    {
        foreach (IPackage package in InstalledExtensions)
            Unload(package);

        InstalledExtensions.Clear();
    }

    //public static List<ICompilerCommand> GetAllCommands(List<IPackage> packages)
    //{
    //    List<ICompilerCommand> commands = [];

    //    foreach (IPackage package in packages)
    //    {
    //        var cmds = GetCommands(package);

    //        foreach (ICompilerCommand cmd in cmds)
    //        {
    //            if (commands.Any(c => c.Command == cmd.Command || c.Aliases().Intersect(cmd.Aliases()).Any()))
    //            {
    //                StringBuilder errMsg = new();
    //                errMsg.Append($"Ambiguous command: The command '{cmd.Command}' is defined by multiple extensions. ");
    //                errMsg.AppendLine($"The command defined in '{package.Metadata.Name}, version {package.Metadata.Version}' will be used.");

    //                EmitWarningMessage(
    //                    0, 0, 0,
    //                    DS0099_DuplicateCompilerCommand,
    //                    errMsg.ToString(),
    //                    CompilerExecutableName);
    //            }

    //            commands.Add(cmd);
    //        }
    //    }

    //    return commands;
    //}

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