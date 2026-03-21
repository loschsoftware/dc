using Dassie.Configuration.ProjectGroups;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents the top-level configuration object of the Dassie project configuration system.
/// </summary>
[Serializable]
[XmlRoot("DassieConfig")]
public partial class DassieConfig : ConfigObject
{
    /// <summary>
    /// The current version of the configuration format.
    /// </summary>
    public static readonly string CurrentFormatVersion = "1.0";

    /// <summary>
    /// Initializes a new instance of the <see cref="DassieConfig"/> type.
    /// </summary>
    /// <param name="store">The <see cref="PropertyStore"/> backing the configuration.</param>
    public DassieConfig(PropertyStore store) : base(store) { }

    /// <summary>
    /// Creates an instance of <see cref="DassieConfig"/> with default values.
    /// </summary>
    public static DassieConfig Default => new(PropertyStore.Default);

    /// <summary>
    /// Retrieves the value of the specified property.
    /// </summary>
    /// <typeparam name="T">The type of the property to evaluate.</typeparam>
    /// <param name="name">The name of the property to evaluate.</param>
    /// <returns>The value of the specified property.</returns>
    public T GetProperty<T>(string name)
    {
        return (T)(this[name] ?? default(T));
    }

    /// <summary>
    /// Sets the specified property.
    /// </summary>
    /// <typeparam name="T">The type of the new value of the property.</typeparam>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The new value of the property.</param>
    public void SetProperty<T>(string name, T value)
    {
        this[name] = value;
    }

    /// <summary>
    /// Gets or sets the value of the specified property.
    /// </summary>
    /// <param name="key">The name of the property to operate on.</param>
    /// <returns>The value of the specified property.</returns>
    public object this[string key]
    {
        get
        {
            return Store.Get(key);
        }
        
        set
        {
            Store.Set(key, value);
        }
    }

    internal static List<Property> GetDefaultPropertyRegistrations()
    {
        List<Property> props = [];

        foreach (PropertyInfo propInfo in typeof(DassieConfig).GetProperties().Skip(1))
        {
            string name = propInfo.Name;

            if (propInfo.GetCustomAttribute<XmlElementAttribute>() is XmlElementAttribute elem && !string.IsNullOrWhiteSpace(elem.ElementName))
                name = elem.ElementName;
            else if (propInfo.GetCustomAttribute<XmlAttributeAttribute>() is XmlAttributeAttribute attrib && !string.IsNullOrWhiteSpace(attrib.AttributeName))
                name = attrib.AttributeName;

            object defaultVal = propInfo.GetCustomAttribute<DefaultValueAttribute>()?.Value;
            string description = propInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;

            props.Add(new(name, propInfo.PropertyType, defaultVal, description));
        }

        return props;
    }

    /// <summary>
    /// Specifies the version of the configuration format to use.
    /// </summary>
    [Description("Specifies the version of the configuration format to use.")]
    [XmlAttribute]
    public string FormatVersion
    {
        get => GetProperty<string>(nameof(FormatVersion));
        set => SetProperty<string>(nameof(FormatVersion), value);
    }

    /// <summary>
    /// Loads the specified configuration file and applies its settings.
    /// </summary>
    [Description("Loads the specified configuration file and applies its settings.")]
    [XmlAttribute]
    public string Base
    {
        get => GetProperty<string>(nameof(Base));
        set => SetProperty<string>(nameof(Base), value);
    }

    /// <summary>
    /// A list of configuration files to import macros from.
    /// </summary>
    [Description("A list of configuration files to import macros from.")]
    [XmlElement]
    public Import[] Imports
    {
        get => GetProperty<Import[]>(nameof(Imports));
        set => SetProperty<Import[]>(nameof(Imports), value);
    }

    /// <summary>
    /// Specifies custom macro definitions.
    /// </summary>
    [Description("Sets custom macro definitions.")]
    [DefaultValue(null)]
    [XmlArray]
    [XmlArrayItem(Type = typeof(Define))]
    public Define[] MacroDefinitions
    {
        get => GetProperty<Define[]>(nameof(MacroDefinitions));
        set => SetProperty<Define[]>(nameof(MacroDefinitions), value);
    }

    /// <summary>
    /// Specifies that the config file defines a project group and sets project group-specific options.
    /// </summary>
    [Description("Specifies that the config file defines a project group and sets project group-specific options.")]
    [DefaultValue(null)]
    [XmlElement]
    public ProjectGroup ProjectGroup
    {
        get => GetProperty<ProjectGroup>(nameof(ProjectGroup));
        set => SetProperty<ProjectGroup>(nameof(ProjectGroup), value);
    }

    /// <summary>
    /// Manages references to external assemblies.
    /// </summary>
    [Description("Manages references to external assemblies.")]
    [XmlArray]
    [XmlArrayItem(Type = typeof(AssemblyReference))]
    [XmlArrayItem(Type = typeof(PackageReference))]
    [XmlArrayItem(Type = typeof(ProjectReference))]
    public Reference[] References
    {
        get => GetProperty<Reference[]>(nameof(References));
        set => SetProperty<Reference[]>(nameof(References), value);
    }

    /// <summary>
    /// Manages references to external resources.
    /// </summary>
    [Description("Manages references to external resources.")]
    [XmlArray]
    [XmlArrayItem(Type = typeof(ManagedResource))]
    [XmlArrayItem(Type = typeof(UnmanagedResource))]
    public Resource[] Resources
    {
        get => GetProperty<Resource[]>(nameof(Resources));
        set => SetProperty<Resource[]>(nameof(Resources), value);
    }

    /// <summary>
    /// Used by editors to set the default namespace of source files.
    /// </summary>
    [Description("Used by editors to set the default namespace of source files.")]
    [XmlElement]
    public string RootNamespace
    {
        get => GetProperty<string>(nameof(RootNamespace));
        set => SetProperty<string>(nameof(RootNamespace), value);
    }

    /// <summary>
    /// Sets the name of the ouput assembly (without file extension).
    /// </summary>
    [Description("Sets the name of the ouput assembly (without file extension).")]
    [XmlElement]
    public string AssemblyFileName
    {
        get => GetProperty<string>(nameof(AssemblyFileName));
        set => SetProperty<string>(nameof(AssemblyFileName), value);
    }

    /// <summary>
    /// Sets the application type and subsystem of the program.
    /// </summary>
    [Description("Sets the application type and subsystem of the program.")]
    [XmlElement]
    [DefaultValue("Console")]
    public string ApplicationType
    {
        get => GetProperty<string>(nameof(ApplicationType));
        set => SetProperty<string>(nameof(ApplicationType), value);
    }

    /// <summary>
    /// Sets the runtime of the application. Valid values are 'Jit' and 'Aot'.
    /// </summary>
    [Description("Sets the runtime of the application. Valid values are 'Jit' and 'Aot'.")]
    [XmlElement]
    [DefaultValue(Runtime.Jit)]
    public Runtime Runtime
    {
        get => GetProperty<Runtime>(nameof(Runtime));
        set => SetProperty<Runtime>(nameof(Runtime), value);
    }

    /// <summary>
    /// Sets the processor architecture of the application.
    /// </summary>
    [Description("Sets the processor architecture of the application.")]
    [XmlElement]
    [DefaultValue(Platform.Auto)]
    public Platform Platform
    {
        get => GetProperty<Platform>(nameof(Platform));
        set => SetProperty<Platform>(nameof(Platform), value);
    }

    /// <summary>
    /// Sets the RID of the application to be used by the AOT compiler.
    /// </summary>
    [Description("Sets the RID of the application to be used by the AOT compiler.")]
    [XmlElement]
    [DefaultValue("")]
    public string RuntimeIdentifier
    {
        get => GetProperty<string>(nameof(RuntimeIdentifier));
        set => SetProperty<string>(nameof(RuntimeIdentifier), value);
    }

    /// <summary>
    /// Sets version information fields for the application.
    /// </summary>
    [Description("Sets version information fields for the application.")]
    [XmlElement]
    public List<VersionInfo> VersionInfo
    {
        get => GetProperty<List<VersionInfo>>(nameof(VersionInfo));
        set => SetProperty<List<VersionInfo>>(nameof(VersionInfo), value);
    }

    /// <summary>
    /// Sets the application icon file.
    /// </summary>
    [Description("Sets the application icon file.")]
    [DefaultValue("")]
    [XmlElement]
    public string IconFile
    {
        get => GetProperty<string>(nameof(IconFile));
        set => SetProperty<string>(nameof(IconFile), value);
    }

    /// <summary>
    /// Sets the directory where the compiled assemblies will be placed.
    /// </summary>
    [Description("Sets the directory where the compiled assemblies will be placed.")]
    [XmlElement]
    [DefaultValue("./build")]
    public string BuildDirectory
    {
        get => GetProperty<string>(nameof(BuildDirectory));
        set => SetProperty<string>(nameof(BuildDirectory), value);
    }

    /// <summary>
    /// Toggles the generation of debug symbol data.
    /// </summary>
    [Description("Toggles the generation of debug symbol data.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool EmitPdb
    {
        get => GetProperty<bool>(nameof(EmitPdb));
        set => SetProperty<bool>(nameof(EmitPdb), value);
    }

    /// <summary>
    /// If set, all 'information' messages are ignored.
    /// </summary>
    [Description("If set, all 'information' messages are ignored.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool IgnoreAllMessages
    {
        get => GetProperty<bool>(nameof(IgnoreAllMessages));
        set => SetProperty<bool>(nameof(IgnoreAllMessages), value);
    }

    /// <summary>
    /// If set, all 'warning' messages are ignored.
    /// </summary>
    [Description("If set, all 'warning' messages are ignored.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool IgnoreAllWarnings
    {
        get => GetProperty<bool>(nameof(IgnoreAllWarnings));
        set => SetProperty<bool>(nameof(IgnoreAllWarnings), value);
    }

    /// <summary>
    /// If set, all 'warning' messages will be treated as errors.
    /// </summary>
    [Description("If set, all 'warning' messages will be treated as errors.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool TreatWarningsAsErrors
    {
        get => GetProperty<bool>(nameof(TreatWarningsAsErrors));
        set => SetProperty<bool>(nameof(TreatWarningsAsErrors), value);
    }

    /// <summary>
    /// Toggles optimizations applied to the generated IL.
    /// </summary>
    [Description("Toggles optimizations applied to the generated IL.")]
    [DefaultValue(true)]
    [XmlElement]
    public bool ILOptimizations
    {
        get => GetProperty<bool>(nameof(ILOptimizations));
        set => SetProperty<bool>(nameof(ILOptimizations), value);
    }

    /// <summary>
    /// If set, displays the elapsed time after the completion of a build.
    /// </summary>
    [Description("If set, displays the elapsed time after the completion of a build.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool MeasureElapsedTime
    {
        get => GetProperty<bool>(nameof(MeasureElapsedTime));
        set => SetProperty<bool>(nameof(MeasureElapsedTime), value);
    }

    /// <summary>
    /// Sets the configuration of the generated assembly. Valid values are 'Debug' and 'Release'.
    /// </summary>
    [Description("Sets the configuration of the generated assembly. Valid values are 'Debug' and 'Release'.")]
    [XmlElement]
    [DefaultValue(ApplicationConfiguration.Debug)]
    public ApplicationConfiguration Configuration
    {
        get => GetProperty<ApplicationConfiguration>(nameof(Configuration));
        set => SetProperty<ApplicationConfiguration>(nameof(Configuration), value);
    }

    /// <summary>
    /// If set, all referenced assemblies are copied to the build output directory.
    /// </summary>
    [Description("If set, all referenced assemblies are copied to the build output directory.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool IncludeDependencies
    {
        get => GetProperty<bool>(nameof(IncludeDependencies));
        set => SetProperty<bool>(nameof(IncludeDependencies), value);
    }

    /// <summary>
    /// If set, includes a preview of the error location in all error messages.
    /// </summary>
    [Description("If set, includes a preview of the error location in all error messages.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool AdvancedErrorMessages
    {
        get => GetProperty<bool>(nameof(AdvancedErrorMessages));
        set => SetProperty<bool>(nameof(AdvancedErrorMessages), value);
    }

    /// <summary>
    /// Toggles further information on some error messages.
    /// </summary>
    [Description("Toggles further information on some error messages.")]
    [DefaultValue(true)]
    [XmlElement]
    public bool EnableTips
    {
        get => GetProperty<bool>(nameof(EnableTips));
        set => SetProperty<bool>(nameof(EnableTips), value);
    }

    /// <summary>
    /// If set, does not delete the generated native '.res' file after the build is completed.
    /// </summary>
    [Description("If set, does not delete the generated native '.res' file after the build is completed.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool PersistentResourceFile
    {
        get => GetProperty<bool>(nameof(PersistentResourceFile));
        set => SetProperty<bool>(nameof(PersistentResourceFile), value);
    }

    /// <summary>
    /// If set, does not delete the generated '.rc' file after the build is completed.
    /// </summary>
    [Description("If set, does not delete the generated '.rc' file after the build is completed.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool PersistentResourceScript
    {
        get => GetProperty<bool>(nameof(PersistentResourceScript));
        set => SetProperty<bool>(nameof(PersistentResourceScript), value);
    }

    /// <summary>
    /// If set, does not delete intermediate source files.
    /// </summary>
    [Description("If set, does not delete intermediate source files.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool KeepIntermediateFiles
    {
        get => GetProperty<bool>(nameof(KeepIntermediateFiles));
        set => SetProperty<bool>(nameof(KeepIntermediateFiles), value);
    }

    /// <summary>
    /// If set, prepends a time stamp to all compiler messages.
    /// </summary>
    [Description("If set, prepends a time stamp to all compiler messages.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool EnableMessageTimestamps
    {
        get => GetProperty<bool>(nameof(EnableMessageTimestamps));
        set => SetProperty<bool>(nameof(EnableMessageTimestamps), value);
    }

    /// <summary>
    /// Sets the verbosity of compiler messages. Valid values are 0-3 (both inclusive).
    /// </summary>
    [Description("Sets the verbosity of compiler messages. Valid values are 0-3 (both inclusive).")]
    [DefaultValue(1)]
    [XmlElement]
    public int Verbosity
    {
        get => GetProperty<int>(nameof(Verbosity));
        set => SetProperty<int>(nameof(Verbosity), value);
    }

    /// <summary>
    /// A list of log devices to write build messages to.
    /// </summary>
    [Description("A list of log devices to write build messages to.")]
    [DefaultValue(null)]
    [XmlElement]
    public BuildLogOptions BuildLogDevices
    {
        get => GetProperty<BuildLogOptions>(nameof(BuildLogDevices));
        set => SetProperty<BuildLogOptions>(nameof(BuildLogDevices), value);
    }

    /// <summary>
    /// If set, generates CIL files in human-readable form.
    /// </summary>
    [Description("If set, generates CIL files in human-readable form.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool GenerateILFiles
    {
        get => GetProperty<bool>(nameof(GenerateILFiles));
        set => SetProperty<bool>(nameof(GenerateILFiles), value);
    }

    /// <summary>
    /// If set, generates native executables along with .NET assemblies.
    /// </summary>
    [Description("If set, generates native executables along with .NET assemblies.")]
    [DefaultValue(true)]
    [XmlElement]
    public bool GenerateNativeAppHost
    {
        get => GetProperty<bool>(nameof(GenerateNativeAppHost));
        set => SetProperty<bool>(nameof(GenerateNativeAppHost), value);
    }

    /// <summary>
    /// A list of error codes that are ignored and will never be emitted.
    /// </summary>
    [Description("A list of error codes that are ignored and will never be emitted.")]
    [XmlArray]
    [XmlArrayItem(typeof(Message))]
    [XmlArrayItem(typeof(Warning))]
    public Ignore[] IgnoredMessages
    {
        get => GetProperty<Ignore[]>(nameof(IgnoredMessages));
        set => SetProperty<Ignore[]>(nameof(IgnoredMessages), value);
    }

    /// <summary>
    /// The path to a .manifest file containing Windows-specific configuration.
    /// </summary>
    [Description("The path to a .manifest file containing Windows-specific configuration.")]
    [XmlElement]
    public string AssemblyManifest
    {
        get => GetProperty<string>(nameof(AssemblyManifest));
        set => SetProperty<string>(nameof(AssemblyManifest), value);
    }

    /// <summary>
    /// If set, implicitly imports parts of the standard library in every source file.
    /// </summary>
    [Description("If set, implicitly imports parts of the standard library in every source file.")]
    [DefaultValue(true)]
    [XmlElement]
    public bool ImplicitImports
    {
        get => GetProperty<bool>(nameof(ImplicitImports));
        set => SetProperty<bool>(nameof(ImplicitImports), value);
    }

    /// <summary>
    /// If set, allows the use of type aliases such as 'int' for 'System.Int32'.
    /// </summary>
    [Description("If set, allows the use of type aliases such as 'int' for 'System.Int32'.")]
    [DefaultValue(true)]
    [XmlElement]
    public bool ImplicitTypeAliases
    {
        get => GetProperty<bool>(nameof(ImplicitTypeAliases));
        set => SetProperty<bool>(nameof(ImplicitTypeAliases), value);
    }

    /// <summary>
    /// If set, prints the full exception message and stack trace if an exception occurs during compilation.
    /// </summary>
    [Description("If set, prints the full exception message and stack trace if an exception occurs during compilation.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool PrintExceptionInfo
    {
        get => GetProperty<bool>(nameof(PrintExceptionInfo));
        set => SetProperty<bool>(nameof(PrintExceptionInfo), value);
    }

    /// <summary>
    /// If set, caches source files for limited incremental compilation capabilities.
    /// </summary>
    [Description("If set, caches source files for limited incremental compilation capabilities.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool CacheSourceFiles
    {
        get => GetProperty<bool>(nameof(CacheSourceFiles));
        set => SetProperty<bool>(nameof(CacheSourceFiles), value);
    }

    /// <summary>
    /// Sets and manages build profiles.
    /// </summary>
    [Description("Sets and manages build profiles.")]
    [DefaultValue(null)]
    [XmlArray]
    public BuildProfile[] BuildProfiles
    {
        get => GetProperty<BuildProfile[]>(nameof(BuildProfiles));
        set => SetProperty<BuildProfile[]>(nameof(BuildProfiles), value);
    }

    /// <summary>
    /// Sets and manages debug profiles.
    /// </summary>
    [Description("Sets and manages debug profiles.")]
    [DefaultValue(null)]
    [XmlArray]
    public DebugProfile[] DebugProfiles
    {
        get => GetProperty<DebugProfile[]>(nameof(DebugProfiles));
        set => SetProperty<DebugProfile[]>(nameof(DebugProfiles), value);
    }

    /// <summary>
    /// The color used for error messages (#RRGGBB).
    /// </summary>
    [Description("The color used for error messages (#RRGGBB).")]
    [DefaultValue(null)]
    [XmlElement]
    public string ErrorColor
    {
        get => GetProperty<string>(nameof(ErrorColor));
        set => SetProperty<string>(nameof(ErrorColor), value);
    }

    /// <summary>
    /// The color used for warning messages (#RRGGBB).
    /// </summary>
    [Description("The color used for warning messages (#RRGGBB).")]
    [DefaultValue(null)]
    [XmlElement]
    public string WarningColor
    {
        get => GetProperty<string>(nameof(WarningColor));
        set => SetProperty<string>(nameof(WarningColor), value);
    }

    /// <summary>
    /// The color used for information messages (#RRGGBB).
    /// </summary>
    [Description("The color used for information messages (#RRGGBB).")]
    [DefaultValue(null)]
    [XmlElement]
    public string MessageColor
    {
        get => GetProperty<string>(nameof(MessageColor));
        set => SetProperty<string>(nameof(MessageColor), value);
    }

    /// <summary>
    /// If set, arithmetic operations check for overflow and throw an appropriate exception at runtime.
    /// </summary>
    [Description("If set, arithmetic operations check for overflow and throw an appropriate exception at runtime.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool EnableOverflowChecks
    {
        get => GetProperty<bool>(nameof(EnableOverflowChecks));
        set => SetProperty<bool>(nameof(EnableOverflowChecks), value);
    }

    /// <summary>
    /// Used to configure the default code analyzer.
    /// </summary>
    [Description("Used to configure the default code analyzer.")]
    [DefaultValue(null)]
    [XmlElement("CodeAnalysis")]
    public CodeAnalysisConfiguration CodeAnalysisConfiguration
    {
        get => GetProperty<CodeAnalysisConfiguration>(nameof(CodeAnalysisConfiguration));
        set => SetProperty<CodeAnalysisConfiguration>(nameof(CodeAnalysisConfiguration), value);
    }

    /// <summary>
    /// Sets the function or method that functions as the application entry point.
    /// </summary>
    [Description("Sets the function or method that functions as the application entry point.")]
    [DefaultValue("")]
    [XmlElement]
    public string EntryPoint
    {
        get => GetProperty<string>(nameof(EntryPoint));
        set => SetProperty<string>(nameof(EntryPoint), value);
    }

    /// <summary>
    /// If enabled, runs code analyzers before compilation.
    /// </summary>
    [Description("If enabled, runs code analyzers before compilation.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool RunAnalyzers
    {
        get => GetProperty<bool>(nameof(RunAnalyzers));
        set => SetProperty<bool>(nameof(RunAnalyzers), value);
    }

    /// <summary>
    /// If enabled, the Dassie standard library (Dassie.Core.dll) is not implicitly referenced.
    /// </summary>
    [Description("If enabled, the Dassie standard library (Dassie.Core.dll) is not implicitly referenced.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool NoStdLib
    {
        get => GetProperty<bool>(nameof(NoStdLib));
        set => SetProperty<bool>(nameof(NoStdLib), value);
    }

    /// <summary>
    /// If enabled, compiler messages will display an icon to distinguish them by severity.
    /// </summary>
    [Description("If enabled, compiler messages will display an icon to distinguish them by severity.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool EnableSeverityIndicators
    {
        get => GetProperty<bool>(nameof(EnableSeverityIndicators));
        set => SetProperty<bool>(nameof(EnableSeverityIndicators), value);
    }

    /// <summary>
    /// Configures compiler extensions enabled only during the compilation.
    /// </summary>
    [Description("Configures compiler extensions enabled only during the compilation.")]
    [DefaultValue(null)]
    [XmlArray]
    public List<Extension> Extensions
    {
        get => GetProperty<List<Extension>>(nameof(Extensions));
        set => SetProperty<List<Extension>>(nameof(Extensions), value);
    }

    /// <summary>
    /// Configures the active document sources provided by compiler extensions.
    /// </summary>
    [Description("Configures the active document sources provided by compiler extensions.")]
    [DefaultValue(null)]
    [XmlElement]
    public DocumentSourceList DocumentSources
    {
        get => GetProperty<DocumentSourceList>(nameof(DocumentSources));
        set => SetProperty<DocumentSourceList>(nameof(DocumentSources), value);
    }

    /// <summary>
    /// Once this number of compilation errors is reached, the build process is terminated immediately.
    /// </summary>
    [Description("Once this number of compilation errors is reached, the build process is terminated immediately.")]
    [DefaultValue(0)]
    [XmlElement]
    public int MaxErrors
    {
        get => GetProperty<int>(nameof(MaxErrors));
        set => SetProperty<int>(nameof(MaxErrors), value);
    }
}

/// <summary>
/// The configuration of the application.
/// </summary>
[Serializable]
public enum ApplicationConfiguration
{
    /// <summary>
    /// Represents a debugging configuration.
    /// </summary>
    Debug,
    /// <summary>
    /// Represents a release configuration.
    /// </summary>
    Release
}

/// <summary>
/// The runtime of the application.
/// </summary>
[Serializable]
public enum Runtime
{
    /// <summary>
    /// Represents a JIT-compiled runtime.
    /// </summary>
    Jit,
    /// <summary>
    /// Represents an AOT-compiled runtime.
    /// </summary>
    Aot
}

/// <summary>
/// The platform of the application.
/// </summary>
[Serializable]
public enum Platform
{
    /// <summary>
    /// Represents a CPU-agnostic configuration.
    /// </summary>
    Auto,
    /// <summary>
    /// Represents an x86 (32-bit) platform.
    /// </summary>
    x86,
    /// <summary>
    /// Represents an x86 (64-bit) platform.
    /// </summary>
    x64,
    /// <summary>
    /// Represents an ARM (32-bit) platform.
    /// </summary>
    Arm32,
    /// <summary>
    /// Represents an ARM (64-bit) platform.
    /// </summary>
    Arm64
}