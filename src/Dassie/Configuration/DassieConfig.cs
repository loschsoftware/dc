using Dassie.Configuration.Macros;
using Dassie.Configuration.ProjectGroups;
using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Specifies the format version a property was introduced in.
/// </summary>
/// <param name="minVersion"></param>
[AttributeUsage(AttributeTargets.Property)]
internal class MinVersionAttribute(string minVersion) : Attribute
{
    public string MinVersion => minVersion;
}

/// <summary>
/// Represents the top-level configuration object of the Dassie project configuration system.
/// </summary>
[Serializable]
[XmlRoot]
public partial class DassieConfig : ConfigObject
{
    /// <summary>
    /// The current version of the configuration format.
    /// </summary>
    public static readonly string CurrentFormatVersion = "1.0";

    internal string DocumentName { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DassieConfig"/> type.
    /// </summary>
    /// <param name="store">The <see cref="PropertyStore"/> backing the configuration.</param>
    public DassieConfig(PropertyStore store) : this(store, ProjectConfigurationFileName) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DassieConfig"/> type.
    /// </summary>
    public DassieConfig() : this(DefaultStore) { }

    internal DassieConfig(PropertyStore store, string documentName) : base(store)
    {
        FormatVersion = CurrentFormatVersion;
        DocumentName = documentName;
    }

    /// <summary>
    /// Creates an instance of <see cref="PropertyStore"/> with a default set of registered properties.
    /// </summary>
    internal static PropertyStore DefaultStore
    {
        get
        {
            MacroParser mp = new();
            PropertyStore ps = new(ExtensionLoader.Properties, mp);
            mp.BindPropertyResolver(ps.Get);
            return ps;
        }
    }

    /// <summary>
    /// Creates an instance of <see cref="DassieConfig"/> with default values.
    /// </summary>
    public static DassieConfig Default => new();

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

        foreach (PropertyInfo propInfo in typeof(DassieConfig).GetProperties())
        {
            if (propInfo.GetCustomAttribute<ConfigPropertyAttribute>() == null)
                continue;

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
    [MinVersion("1.0")]
    [Explicit]
    [ConfigProperty]
    public partial string FormatVersion { get; set; }

    /// <summary>
    /// Specifies wheter or not this configuration file is an SDK definition file.
    /// </summary>
    [Description("Specifies wheter or not this configuration file is an SDK definition file.")]
    [DefaultValue(false)]
    [XmlAttribute]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool IsDefinitionFile { get; set; }

    /// <summary>
    /// Specifies the SDK identifier of this configuration.
    /// </summary>
    [Description("Specifies the SDK identifier of this configuration.")]
    [XmlAttribute]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial string SdkName { get; set; }

    /// <summary>
    /// Loads the specified configuration file and applies its settings.
    /// </summary>
    [Description("Loads the specified configuration file and applies its settings.")]
    [XmlAttribute]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial string Base { get; set; }

    /// <summary>
    /// A list of configuration files to import macros from.
    /// </summary>
    [Description("A list of configuration files to import macros from.")]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial Import[] Imports { get; set; }

    /// <summary>
    /// Specifies custom macro definitions.
    /// </summary>
    [Description("Sets custom macro definitions.")]
    [DefaultValue(null)]
    [XmlArray]
    [XmlArrayItem(Type = typeof(Define))]
    [ConfigProperty]
    public partial Define[] MacroDefinitions { get; set; }

    /// <summary>
    /// Specifies that the config file defines a project group and sets project group-specific options.
    /// </summary>
    [Description("Specifies that the config file defines a project group and sets project group-specific options.")]
    [DefaultValue(null)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial ProjectGroup ProjectGroup { get; set; }

    /// <summary>
    /// Manages references to external assemblies.
    /// </summary>
    [Description("Manages references to external assemblies.")]
    [XmlArray]
    [XmlArrayItem(Type = typeof(AssemblyReference))]
    [XmlArrayItem(Type = typeof(PackageReference))]
    [XmlArrayItem(Type = typeof(ProjectReference))]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial Reference[] References { get; set; }

    /// <summary>
    /// Manages references to external resources.
    /// </summary>
    [Description("Manages references to external resources.")]
    [XmlArray]
    [XmlArrayItem(Type = typeof(ManagedResource))]
    [XmlArrayItem(Type = typeof(UnmanagedResource))]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial Resource[] Resources { get; set; }

    /// <summary>
    /// Used by editors to set the default namespace of source files.
    /// </summary>
    [Description("Used by editors to set the default namespace of source files.")]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial string RootNamespace { get; set; }

    /// <summary>
    /// Sets the name of the ouput assembly (without file extension).
    /// </summary>
    [Description("Sets the name of the ouput assembly (without file extension).")]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial string AssemblyFileName { get; set; }

    /// <summary>
    /// Sets the application type and subsystem of the program.
    /// </summary>
    [Description("Sets the application type and subsystem of the program.")]
    [XmlElement]
    [DefaultValue("Console")]
    [MinVersion("1.0")]
    [Explicit]
    [ConfigProperty]
    public partial string ApplicationType { get; set; }

    /// <summary>
    /// Sets the runtime of the application. Valid values are 'Jit' and 'Aot'.
    /// </summary>
    [Description("Sets the runtime of the application. Valid values are 'Jit' and 'Aot'.")]
    [XmlElement]
    [DefaultValue(Runtime.Jit)]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial Runtime Runtime { get; set; }

    /// <summary>
    /// Sets the processor architecture of the application.
    /// </summary>
    [Description("Sets the processor architecture of the application.")]
    [XmlElement]
    [DefaultValue(Platform.Auto)]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial Platform Platform { get; set; }

    /// <summary>
    /// Sets the RID of the application to be used by the AOT compiler.
    /// </summary>
    [Description("Sets the RID of the application to be used by the AOT compiler.")]
    [XmlElement]
    [DefaultValue("")]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial string RuntimeIdentifier { get; set; }

    /// <summary>
    /// Sets version information fields for the application.
    /// </summary>
    [Description("Sets version information fields for the application.")]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial List<VersionInfo> VersionInfo { get; set; }

    /// <summary>
    /// Sets the application icon file.
    /// </summary>
    [Description("Sets the application icon file.")]
    [DefaultValue("")]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial string IconFile { get; set; }

    /// <summary>
    /// Sets the directory where the compiled assemblies will be placed.
    /// </summary>
    [Description("Sets the directory where the compiled assemblies will be placed.")]
    [XmlElement]
    [DefaultValue("./build")]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial string BuildDirectory { get; set; }

    /// <summary>
    /// Toggles the generation of debug symbol data.
    /// </summary>
    [Description("Toggles the generation of debug symbol data.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool EmitPdb { get; set; }

    /// <summary>
    /// If set, all 'information' messages are ignored.
    /// </summary>
    [Description("If set, all 'information' messages are ignored.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool IgnoreAllMessages { get; set; }

    /// <summary>
    /// If set, all 'warning' messages are ignored.
    /// </summary>
    [Description("If set, all 'warning' messages are ignored.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool IgnoreAllWarnings { get; set; }

    /// <summary>
    /// If set, all 'warning' messages will be treated as errors.
    /// </summary>
    [Description("If set, all 'warning' messages will be treated as errors.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool TreatWarningsAsErrors { get; set; }

    /// <summary>
    /// Toggles optimizations applied to the generated IL.
    /// </summary>
    [Description("Toggles optimizations applied to the generated IL.")]
    [DefaultValue(true)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool ILOptimizations { get; set; }

    /// <summary>
    /// If set, displays the elapsed time after the completion of a build.
    /// </summary>
    [Description("If set, displays the elapsed time after the completion of a build.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool MeasureElapsedTime { get; set; }

    /// <summary>
    /// Sets the configuration of the generated assembly. Valid values are 'Debug' and 'Release'.
    /// </summary>
    [Description("Sets the configuration of the generated assembly. Valid values are 'Debug' and 'Release'.")]
    [XmlElement]
    [DefaultValue(ApplicationConfiguration.Debug)]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial ApplicationConfiguration Configuration { get; set; }

    /// <summary>
    /// If set, all referenced assemblies are copied to the build output directory.
    /// </summary>
    [Description("If set, all referenced assemblies are copied to the build output directory.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool IncludeDependencies { get; set; }

    /// <summary>
    /// If set, includes a preview of the error location in all error messages.
    /// </summary>
    [Description("If set, includes a preview of the error location in all error messages.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool AdvancedErrorMessages { get; set; }

    /// <summary>
    /// Toggles further information on some error messages.
    /// </summary>
    [Description("Toggles further information on some error messages.")]
    [DefaultValue(true)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool EnableTips { get; set; }

    /// <summary>
    /// If set, does not delete the generated native '.res' file after the build is completed.
    /// </summary>
    [Description("If set, does not delete the generated native '.res' file after the build is completed.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool PersistentResourceFile { get; set; }

    /// <summary>
    /// If set, does not delete the generated '.rc' file after the build is completed.
    /// </summary>
    [Description("If set, does not delete the generated '.rc' file after the build is completed.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool PersistentResourceScript { get; set; }

    /// <summary>
    /// If set, does not delete intermediate source files.
    /// </summary>
    [Description("If set, does not delete intermediate source files.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool KeepIntermediateFiles { get; set; }

    /// <summary>
    /// If set, prepends a time stamp to all compiler messages.
    /// </summary>
    [Description("If set, prepends a time stamp to all compiler messages.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool EnableMessageTimestamps { get; set; }

    /// <summary>
    /// Sets the verbosity of compiler messages. Valid values are 0-3 (both inclusive).
    /// </summary>
    [Description("Sets the verbosity of compiler messages. Valid values are 0-3 (both inclusive).")]
    [DefaultValue(1)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial int Verbosity { get; set; }

    /// <summary>
    /// A list of log devices to write build messages to.
    /// </summary>
    [Description("A list of log devices to write build messages to.")]
    [DefaultValue(null)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial BuildLogOptions BuildLogDevices { get; set; }

    /// <summary>
    /// If set, generates CIL files in human-readable form.
    /// </summary>
    [Description("If set, generates CIL files in human-readable form.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool GenerateILFiles { get; set; }

    /// <summary>
    /// If set, generates native executables along with .NET assemblies.
    /// </summary>
    [Description("If set, generates native executables along with .NET assemblies.")]
    [DefaultValue(true)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool GenerateNativeAppHost { get; set; }

    /// <summary>
    /// A list of error codes that are ignored and will never be emitted.
    /// </summary>
    [Description("A list of error codes that are ignored and will never be emitted.")]
    [XmlArray]
    [XmlArrayItem(typeof(Message))]
    [XmlArrayItem(typeof(Warning))]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial Ignore[] IgnoredMessages { get; set; }

    /// <summary>
    /// The path to a .manifest file containing Windows-specific configuration.
    /// </summary>
    [Description("The path to a .manifest file containing Windows-specific configuration.")]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial string AssemblyManifest { get; set; }

    /// <summary>
    /// If set, implicitly imports parts of the standard library in every source file.
    /// </summary>
    [Description("If set, implicitly imports parts of the standard library in every source file.")]
    [DefaultValue(true)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool ImplicitImports { get; set; }

    /// <summary>
    /// If set, allows the use of type aliases such as 'int' for 'System.Int32'.
    /// </summary>
    [Description("If set, allows the use of type aliases such as 'int' for 'System.Int32'.")]
    [DefaultValue(true)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool ImplicitTypeAliases { get; set; }

    /// <summary>
    /// If set, prints the full exception message and stack trace if an exception occurs during compilation.
    /// </summary>
    [Description("If set, prints the full exception message and stack trace if an exception occurs during compilation.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool PrintExceptionInfo { get; set; }

    /// <summary>
    /// If set, caches source files for limited incremental compilation capabilities.
    /// </summary>
    [Description("If set, caches source files for limited incremental compilation capabilities.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool CacheSourceFiles { get; set; }

    /// <summary>
    /// Sets and manages build profiles.
    /// </summary>
    [Description("Sets and manages build profiles.")]
    [DefaultValue(null)]
    [XmlArray]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial BuildProfile[] BuildProfiles { get; set; }

    /// <summary>
    /// Sets and manages debug profiles.
    /// </summary>
    [Description("Sets and manages debug profiles.")]
    [DefaultValue(null)]
    [XmlArray]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial DebugProfile[] DebugProfiles { get; set; }

    /// <summary>
    /// The color used for error messages (#RRGGBB).
    /// </summary>
    [Description("The color used for error messages (#RRGGBB).")]
    [DefaultValue(null)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial string ErrorColor { get; set; }

    /// <summary>
    /// The color used for warning messages (#RRGGBB).
    /// </summary>
    [Description("The color used for warning messages (#RRGGBB).")]
    [DefaultValue(null)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial string WarningColor { get; set; }

    /// <summary>
    /// The color used for information messages (#RRGGBB).
    /// </summary>
    [Description("The color used for information messages (#RRGGBB).")]
    [DefaultValue(null)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial string MessageColor { get; set; }

    /// <summary>
    /// If set, arithmetic operations check for overflow and throw an appropriate exception at runtime.
    /// </summary>
    [Description("If set, arithmetic operations check for overflow and throw an appropriate exception at runtime.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool EnableOverflowChecks { get; set; }

    /// <summary>
    /// Used to configure the default code analyzer.
    /// </summary>
    [Description("Used to configure the default code analyzer.")]
    [DefaultValue(null)]
    [XmlElement("CodeAnalysis")]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial CodeAnalysisConfiguration CodeAnalysisConfiguration { get; set; }

    /// <summary>
    /// Sets the function or method that functions as the application entry point.
    /// </summary>
    [Description("Sets the function or method that functions as the application entry point.")]
    [DefaultValue("")]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial string EntryPoint { get; set; }

    /// <summary>
    /// If enabled, runs code analyzers before compilation.
    /// </summary>
    [Description("If enabled, runs code analyzers before compilation.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool RunAnalyzers { get; set; }

    /// <summary>
    /// If enabled, the Dassie standard library (Dassie.Core.dll) is not implicitly referenced.
    /// </summary>
    [Description("If enabled, the Dassie standard library (Dassie.Core.dll) is not implicitly referenced.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool NoStdLib { get; set; }

    /// <summary>
    /// If enabled, compiler messages will display an icon to distinguish them by severity.
    /// </summary>
    [Description("If enabled, compiler messages will display an icon to distinguish them by severity.")]
    [DefaultValue(false)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial bool EnableSeverityIndicators { get; set; }

    /// <summary>
    /// Configures compiler extensions enabled only during the compilation.
    /// </summary>
    [Description("Configures compiler extensions enabled only during the compilation.")]
    [DefaultValue(null)]
    [XmlArray]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial List<Extension> Extensions { get; set; }

    /// <summary>
    /// Configures the active document sources provided by compiler extensions.
    /// </summary>
    [Description("Configures the active document sources provided by compiler extensions.")]
    [DefaultValue(null)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial DocumentSourceList DocumentSources { get; set; }

    /// <summary>
    /// Once this number of compilation errors is reached, the build process is terminated immediately.
    /// </summary>
    [Description("Once this number of compilation errors is reached, the build process is terminated immediately.")]
    [DefaultValue(0)]
    [XmlElement]
    [MinVersion("1.0")]
    [ConfigProperty]
    public partial int MaxErrors { get; set; }
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