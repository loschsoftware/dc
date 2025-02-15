using Dassie.Configuration.ProjectGroups;
using System.ComponentModel;

namespace Dassie.Configuration;

[Serializable]
[XmlRoot("DassieConfig")]
public sealed class DassieConfig
{
    public const string CurrentFormatVersion = "1.0";

    [Description("Specifies the version of the dsconfig format to use.")]
    [XmlAttribute("FormatVersion")]
    public string FormatVersion { get; set; }

    [Description("Loads the specified configuration file and applies its settings.")]
    [XmlAttribute]
    public string Import { get; set; }

    [Description("Sets custom macro definitions.")]
    [DefaultValue(null)]
    [XmlArray("MacroDefinitions")]
    [XmlArrayItem(Type = typeof(Define))]
    public Define[] MacroDefinitions { get; set; }

    [Description("Specifies that the config file defines a project group and sets project group-specific options.")]
    [DefaultValue(null)]
    [XmlElement]
    public ProjectGroup ProjectGroup { get; set; }

    [Description("Manages references to external assemblies.")]
    [XmlArray("References")]
    [XmlArrayItem(Type = typeof(AssemblyReference))]
    [XmlArrayItem(Type = typeof(PackageReference))]
    [XmlArrayItem(Type = typeof(ProjectReference))]
    public Reference[] References { get; set; }

    [Description("Manages references to external resources.")]
    [XmlArray]
    [XmlArrayItem(Type = typeof(ManagedResource))]
    [XmlArrayItem(Type = typeof(UnmanagedResource))]
    public Resource[] Resources { get; set; }

    [Description("Used by editors to set the default namespace of source files.")]
    [XmlElement("RootNamespace")]
    public string RootNamespace { get; set; }

    [Description("Sets the name of the ouput assembly (without file extension).")]
    [XmlElement("AssemblyFileName")]
    public string AssemblyName { get; set; }

    [Description("Sets the application type and subsystem of the program.")]
    [XmlElement("ApplicationType")]
    [DefaultValue(ApplicationType.Console)]
    public ApplicationType ApplicationType { get; set; } = ApplicationType.Console;

    [Description("Sets the runtime of the application. Valid values are 'Jit' and 'Aot'.")]
    [XmlElement]
    [DefaultValue(Runtime.Jit)]
    public Runtime Runtime { get; set; }

    [Description("Sets the processor architecture of the application.")]
    [XmlElement]
    [DefaultValue(Platform.Auto)]
    public Platform Platform { get; set; }

    [Description("Sets the RID of the application to be used by the AOT compiler.")]
    [XmlElement]
    [DefaultValue("")]
    public string RuntimeIdentifier { get; set; }

    [Description("Defines version information of the application.")]
    [XmlElement("VersionInfo")]
    public VersionInfo VersionInfo { get; set; }

    [Description("Sets the directory where the compiled assemblies will be placed.")]
    [XmlElement("BuildDirectory")]
    [DefaultValue("./build")]
    public string BuildOutputDirectory { get; set; } = "./build";

    [Description("Toggles generation of debug symbol data.")]
    [DefaultValue(false)]
    [XmlElement("EmitPdb")]
    public bool CreatePdb { get; set; }

    [Description("If set, all 'information' messages are ignored.")]
    [DefaultValue(false)]
    [XmlElement("IgnoreAllMessages")]
    public bool IgnoreMessages { get; set; }

    [Description("If set, all 'warning' messages are ignored.")]
    [DefaultValue(false)]
    [XmlElement("IgnoreAllWarnings")]
    public bool IgnoreWarnings { get; set; }

    [Description("If set, all 'warning' messages will be treated as errors.")]
    [DefaultValue(false)]
    [XmlElement("TreatWarningsAsErrors")]
    public bool TreatWarningsAsErrors { get; set; }

    [Description("Toggles optimizations done to the generated IL.")]
    [DefaultValue(true)]
    [XmlElement("ILOptimizations")]
    public bool ILOptimizations { get; set; } = true;

    [Description("If set, displays the elapsed time after the completion of a build.")]
    [DefaultValue(false)]
    [XmlElement("MeasureElapsedTime")]
    public bool MeasureElapsedTime { get; set; }

    [Description("Sets the configuration of the generated assembly. Valid values are 'Debug' and 'Release'.")]
    [XmlElement("Configuration")]
    [DefaultValue(ApplicationConfiguration.Debug)]
    public ApplicationConfiguration Configuration { get; set; }

    [Description("If set, all referenced assemblies are copied to the build output directory.")]
    [DefaultValue(false)]
    [XmlElement("IncludeDependencies")]
    public bool IncludeDependencies { get; set; }

    [Description("If set, includes a preview of the error location in all error messages.")]
    [DefaultValue(false)]
    [XmlElement("AdvancedErrorMessages")]
    public bool AdvancedErrorMessages { get; set; }

    [Description("Toggles further information on some error messages.")]
    [DefaultValue(true)]
    [XmlElement("EnableTips")]
    public bool EnableTips { get; set; } = true;

    [Description("If set, does not delete the generated native '.res' file after the build is completed.")]
    [DefaultValue(false)]
    [XmlElement("PersistentResourceFile")]
    public bool PersistentResourceFile { get; set; }

    [Description("If set, does not delete the generated '.rc' file after the build is completed.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool PersistentResourceScript { get; set; }

    [Description("If set, does not delete intermediate source files.")]
    [DefaultValue(false)]
    [XmlElement("KeepIntermediateFiles")]
    public bool KeepIntermediateFiles { get; set; } = false;

    [Description("If set, prepends a time stamp to all compiler messages.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool EnableMessageTimestamps { get; set; } = false;

    [Description("Sets the verbosity of compiler messages. Valid values are 0, 1 and 2.")]
    [DefaultValue(0)]
    [XmlElement]
    public int Verbosity { get; set; } = 0;

    [Description("A list of log devices to write build messages to.")]
    [DefaultValue(null)]
    [XmlElement("BuildLogDevices")]
    public BuildLogOptions BuildLogOptions { get; set; }

    [Description("If set, generates CIL files in human-readable form.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool GenerateILFiles { get; set; } = false;

    [Description("If set, generates native executables along with the .NET assemblies.")]
    [DefaultValue(true)]
    [XmlElement]
    public bool GenerateNativeAppHost { get; set; } = true;

    [Description("A list of error codes that are ignored and will never be emitted.")]
    [XmlArray("IgnoredMessages")]
    [XmlArrayItem(typeof(Message))]
    [XmlArrayItem(typeof(Warning))]
    public Ignore[] IgnoredMessages { get; set; }

    [Description("The path to a .manifest file containing Windows-specific configuration.")]
    [XmlElement]
    public string AssemblyManifest { get; set; }

    [Description("If set, implicitly imports parts of the standard library in every source file.")]
    [DefaultValue(true)]
    [XmlElement]
    public bool ImplicitImports { get; set; } = true;

    [Description("If set, allows the use of type aliases such as 'int' for 'System.Int32'.")]
    [DefaultValue(true)]
    [XmlElement]
    public bool ImplicitTypeAliases { get; set; } = true;

    [Description("If set, prints the full exception message and stack trace if an exception occurs during compilation.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool PrintExceptionInfo { get; set; } = false;

    [Description("If set, caches source files for limited incremental compilation capabilities.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool CacheSourceFiles { get; set; } = false;

    [Description("Sets and manages build profiles.")]
    [DefaultValue(null)]
    [XmlArray]
    public BuildProfile[] BuildProfiles { get; set; }

    [Description("Sets and manages debug profiles.")]
    [DefaultValue(null)]
    [XmlArray]
    public DebugProfile[] DebugProfiles { get; set; }

    [Description("The color used for error messages (#RRGGBB).")]
    [DefaultValue(null)]
    [XmlElement]
    public string ErrorColor { get; set; }

    [Description("The color used for warning messages (#RRGGBB).")]
    [DefaultValue(null)]
    [XmlElement]
    public string WarningColor { get; set; }

    [Description("The color used for information messages (#RRGGBB).")]
    [DefaultValue(null)]
    [XmlElement]
    public string MessageColor { get; set; }

    [Description("If set, arithmetic operations check for overflow and throw an appropriate exception at runtime.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool EnableOverflowChecks { get; set; }

    [Description("Used to configure the default code analyzer.")]
    [DefaultValue(null)]
    [XmlElement("CodeAnalysis")]
    public CodeAnalysisConfiguration CodeAnalysisConfiguration { get; set; }

    [Description("Sets the function or method that functions as the application entry point.")]
    [DefaultValue("")]
    [XmlElement]
    public string EntryPoint { get; set; }

    [Description("If enabled, runs code analyzers before compilation.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool RunAnalyzers { get; set; }

    [Description("If enabled, the Dassie standard library (Dassie.Core.dll) is not implicitly referenced.")]
    [DefaultValue(false)]
    [XmlElement]
    public bool NoStdLib { get; set; }
}

[Serializable]
public enum ApplicationType
{
    Console,
    WinExe,
    Library,
    Installer
}

[Serializable]
public enum ApplicationConfiguration
{
    Debug,
    Release
}

[Serializable]
public enum Runtime
{
    Jit,
    Aot
}

[Serializable]
public enum Platform
{
    Auto,
    x86,
    x64,
    Arm32,
    Arm64
}