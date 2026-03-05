using Antlr4.Runtime.Tree;
using Dassie.CodeAnalysis;
using Dassie.Core;
using Dassie.Messages.Devices;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Dassie.Extensions;

internal static class ExtensionLoader
{
    public static void Initialize()
    {
        InstalledExtensions = [];
        InstalledExtensions.AddRange(LoadInstalledExtensions());
        InstalledExtensions.CollectionChanged += Update;
        InitializeGlobalExtensions();
    }

    private static readonly IEnvironmentInfo _env = new CompilerEnvironmentInfo()
    {
        ConfigurationFunc = () => Context.Configuration,
        ExtensionsFunc = () => InstalledExtensions
    };

    private static readonly string _extensionsDefaultPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Dassie",
        "Extensions");

    public static string DefaultExtensionSource => field ??= Directory.CreateDirectory(GetProperty("Locations.Extensions") ?? _extensionsDefaultPath).FullName;
    public static string GlobalToolsPath => Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Tools")).FullName;

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

    private static IEnumerable<GlobalConfigProperty> _gloablConfigProperties = [];
    public static IEnumerable<GlobalConfigProperty> GlobalConfigProperties => _gloablConfigProperties;

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

    private static IEnumerable<IDeploymentTarget> _deploymentTargets = [];
    public static IEnumerable<IDeploymentTarget> DeploymentTargets => _deploymentTargets;

    private static IEnumerable<ISubsystem> _subsystems = [];
    public static IEnumerable<ISubsystem> Subsystems => _subsystems;

    private static IEnumerable<IBuildAction> _buildActions = [];
    public static IEnumerable<IBuildAction> BuildActions => _buildActions;

    private static IEnumerable<IMacro> _macros = [];
    public static IEnumerable<IMacro> Macros => _macros;

    private static IEnumerable<IResourceProvider<string>> _localizationResourceProviders = [];
    public static IEnumerable<IResourceProvider<string>> LocalizationResourceProviders => _localizationResourceProviders;

    private static void Update()
    {
        _gloablConfigProperties = InstalledExtensions.SelectMany(p => p.GlobalProperties());
        _commands = InstalledExtensions.SelectMany(p => p.Commands());
        _configurationProviders = InstalledExtensions.SelectMany(p => p.ConfigurationProviders());
        _codeAnalyzers = InstalledExtensions.SelectMany(p => p.CodeAnalyzers());
        _buildLogDevices = InstalledExtensions.SelectMany(p => p.BuildLogDevices());
        _projectTemplates = InstalledExtensions.SelectMany(p => p.ProjectTemplates());
        _compilerDirectives = InstalledExtensions.SelectMany(p => p.CompilerDirectives());
        _documentSources = InstalledExtensions.SelectMany(p => p.DocumentSources());
        _deploymentTargets = InstalledExtensions.SelectMany(p => p.DeploymentTargets());
        _subsystems = InstalledExtensions.SelectMany(p => p.Subsystems());
        _buildActions = InstalledExtensions.SelectMany(p => p.BuildActions());
        _macros = InstalledExtensions.SelectMany(p => p.Macros());
        _localizationResourceProviders = InstalledExtensions.SelectMany(p => p.LocalizationResourceProviders());
    }

    private static void Update(object sender, NotifyCollectionChangedEventArgs e)
    {
        Update();
        Verify();
    }

    private static void Verify()
    {
        if (_commands == null || !_commands.Any())
            return;

        Dictionary<string, (ICompilerCommand command, IPackage package)> seenCommands = [];

        foreach (IPackage package in InstalledExtensions)
        {
            foreach (ICompilerCommand cmd in package.Commands())
            {
                if (seenCommands.TryGetValue(cmd.Command, out var existing))
                {
                    EmitWarningMessageFormatted(
                        0, 0, 0,
                        DS0100_DuplicateCompilerCommand,
                        nameof(StringHelper.ExtensionLoader_AmbiguousCommand), [cmd.Command, existing.package.Metadata.Name],
                        CompilerExecutableName);
                }
                else
                    seenCommands[cmd.Command] = (cmd, package);

                foreach (string alias in cmd.Aliases ?? [])
                {
                    if (seenCommands.TryGetValue(alias, out existing))
                    {
                        EmitWarningMessageFormatted(
                            0, 0, 0,
                            DS0100_DuplicateCompilerCommand,
                            nameof(StringHelper.ExtensionLoader_AmbiguousAlias), [alias, existing.package.Metadata.Name],
                            CompilerExecutableName);
                    }
                    else
                        seenCommands[alias] = (cmd, package);
                }
            }
        }
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

        string enableExtensionsStr = GetProperty("EnableExtensions");
        string enableCorePackageStr = GetProperty("EnableCorePackage");

        if (enableExtensionsStr == null || (bool.TryParse(enableExtensionsStr, out bool enableExtensions) && enableExtensions))
        {
            foreach (string file in Directory.EnumerateFiles(DefaultExtensionSource, "*.dll", SearchOption.AllDirectories))
                packages.AddRange(LoadInstalledExtensions(file));
        }

        if (enableCorePackageStr == null || (bool.TryParse(enableCorePackageStr, out bool enableCorePackage) && enableCorePackage))
            packages.Add(CorePackage.Instance);

        ActivateBuildLogWriters(packages);
        return packages;
    }

    private static void InitializeGlobalExtensions()
    {
        foreach (IPackage package in InstalledExtensions)
        {
            int ret;

            if (!package.Modes().HasFlag(ExtensionModes.Global))
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0223_ExtensionUnsupportedMode,
                    nameof(StringHelper.ExtensionLoader_CannotLoadGlobal), [package.Metadata.Name],
                    CompilerExecutableName);

                continue;
            }

            try
            {
                ret = package.InitializeGlobal(_env);
            }
            catch (Exception ex)
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0224_ExtensionInitializerFailed,
                    nameof(StringHelper.ExtensionLoader_InitializerException), [package.Metadata.Name],
                    CompilerExecutableName);

                if (Context.Configuration.PrintExceptionInfo)
                    TextWriterBuildLogDevice.ErrorOut.WriteLine(ex.ToString());

                continue;
            }

            if (ret != 0)
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0224_ExtensionInitializerFailed,
                    nameof(StringHelper.ExtensionLoader_InitializerNonzeroExit), [package.Metadata.Name],
                    CompilerExecutableName);

                continue;
            }

            EmitBuildLogMessageFormatted(nameof(StringHelper.ExtensionLoader_ExtensionLoaded), [package.Metadata.Name], 2, true);
        }
    }

    public static void LoadTransientExtensions(IEnumerable<(string, List<XmlAttribute>, List<XmlElement>)> paths)
    {
        foreach ((string path, List<XmlAttribute> attribs, List<XmlElement> elems) in paths)
        {
            EmitBuildLogMessageFormatted(nameof(StringHelper.ExtensionLoader_TransientExtensionLoaded), [path], 2, true);
            List<IPackage> packages = LoadInstalledExtensions(path);

            foreach (IPackage package in packages)
            {
                int ret = -1;

                if (!package.Modes().HasFlag(ExtensionModes.Transient))
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0223_ExtensionUnsupportedMode,
                        nameof(StringHelper.ExtensionLoader_CannotLoadTransient), [package.Metadata.Name],
                        CompilerExecutableName);

                    continue;
                }

                try
                {
                    ret = package.InitializeTransient(_env, attribs, elems);
                }
                catch (Exception ex)
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0224_ExtensionInitializerFailed,
                        nameof(StringHelper.ExtensionLoader_InitializerException), [package.Metadata.Name],
                        CompilerExecutableName);

                    if (Context.Configuration.PrintExceptionInfo)
                        TextWriterBuildLogDevice.ErrorOut.WriteLine(ex.ToString());

                    continue;
                }

                if (ret != 0)
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0224_ExtensionInitializerFailed,
                        nameof(StringHelper.ExtensionLoader_InitializerNonzeroExit), [package.Metadata.Name],
                        CompilerExecutableName);

                    continue;
                }

                if (InstalledExtensions.Any(p => p.Metadata.Id == package.Metadata.Id))
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0225_ExtensionDuplicateMode,
                        nameof(StringHelper.ExtensionLoader_DuplicateMode), [package.Metadata.Name],
                        CompilerExecutableName);

                    IPackage duplicate = InstalledExtensions.First(p => p.Metadata.Id == package.Metadata.Id);
                    Unload(duplicate);
                    InstalledExtensions.Remove(duplicate);
                }
            }

            InstalledExtensions.AddRange(packages);
        }
    }

    public static List<IPackage> LoadInstalledExtensions(string assembly)
    {
        if (!File.Exists(assembly))
        {
            EmitErrorMessageFormatted(0, 0, 0,
                DS0222_ExtensionFileNotFound,
                nameof(StringHelper.ExtensionLoader_PackageNotFound), [assembly],
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
                packages.Add(package);
            }
        }
        catch (ReflectionTypeLoadException)
        {
            EmitWarningMessageFormatted(0, 0, 0,
                DS0124_InvalidExtensionPackage,
                nameof(StringHelper.ExtensionLoader_MalformedPackage), [extensionAssembly.GetName().Name],
                CompilerExecutableName);
        }

        return packages;
    }

    public static void Unload(IPackage package)
    {
        if (!InstalledExtensions.Contains(package))
            return;

        try
        {
            package?.Unload();
        }
        catch (Exception ex)
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0224_ExtensionInitializerFailed,
                nameof(StringHelper.ExtensionLoader_FinalizerException), [package.Metadata.Name],
                CompilerExecutableName);

            if (Context.Configuration.PrintExceptionInfo)
                TextWriterBuildLogDevice.ErrorOut.WriteLine(ex.ToString());
        }

        InstalledExtensions.Remove(package);
        EmitBuildLogMessageFormatted(nameof(StringHelper.ExtensionLoader_ExtensionUnloaded), [package.Metadata.Name], 2);
    }

    public static void UnloadAll()
    {
        if (InstalledExtensions == null)
            return;

        foreach (IPackage package in InstalledExtensions.ToArray())
            Unload(package);
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

    public static ISubsystem GetSubsystem(string name)
    {
        if (string.IsNullOrEmpty(name))
            return Configuration.Subsystems.Console.Instance;

        if (Subsystems.Any(s => s.Name == name))
            return Subsystems.First(s => s.Name == name);

        EmitErrorMessageFormatted(
            0, 0, 0,
            DS0251_InvalidSubsystem,
            nameof(StringHelper.ExtensionLoader_SubsystemNotResolved), [name],
            ProjectConfigurationFileName);

        return Configuration.Subsystems.Console.Instance;
    }

    private static string GetProperty(string name)
    {
        string configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Dassie",
            GlobalConfigurationFileName);

        if (!File.Exists(configPath))
            return null;

        try
        {
            XDocument doc = XDocument.Load(configPath);
            XElement coreModule = doc.Root?.Elements()
                .FirstOrDefault(e => e.Name.LocalName.Equals("Core", StringComparison.OrdinalIgnoreCase));

            if (coreModule == null)
                return null;

            XElement extensionLocationElement = coreModule.Elements()
                .FirstOrDefault(e => e.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (extensionLocationElement != null && !string.IsNullOrWhiteSpace(extensionLocationElement.Value))
                return extensionLocationElement.Value.Trim();
        }
        catch (XmlException) { }

        return null;
    }
}