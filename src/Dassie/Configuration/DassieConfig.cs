using Dassie.Configuration.ProjectGroups;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

#pragma warning disable CS1591

namespace Dassie.Configuration;

public class MalformedPropertyValueException(string propertyName, Exception innerException)
    : Exception($"Malformed property value for '{propertyName}'.", innerException)
{
    public string PropertyName { get; } = propertyName;
}

public class PropertyStore
{
    public static readonly PropertyStore Empty = new();

    private readonly IEnumerable<Property> _propertyDefs;

    // Value can be:
    //      string              => "Primitive value" (string/bool/int/enum/...)
    //      XElement            => complex type
    //      (string|XElement)[] => array
    //      object              => already evaluated (fallback value)
    //      object[]            => array of evaluated values
    private readonly Dictionary<string, object> _uninstantiatedProperties;
    private readonly Dictionary<string, object> _instantiatedProperties = [];
    private readonly Dictionary<string, (int Line, int Column)> _propertyLocationMapping;

    private PropertyStore()
    {
        _propertyDefs = [];
        _uninstantiatedProperties = [];
        _propertyLocationMapping = [];
    }

    public PropertyStore(IEnumerable<Property> defs, Dictionary<string, object> uninstantiatedValues = null)
    {
        _propertyDefs = defs;
        _uninstantiatedProperties = uninstantiatedValues ?? [];
    }

    private Property GetPropertyDef(string key)
    {
        return _propertyDefs.FirstOrDefault(p => p.Name == key);
    }

    private (object Result, bool CanBeCached) Evaluate(object raw)
    {
        if (raw is string str)
        {
            // Constant string with no macros
            if (!str.Contains('$') && !str.Contains('^'))
                return (str, true);

            // TODO: Evaluate str here
            return (str, false);
        }

        return (raw, false);
    }

    private static object ValidateAndConvert(string name, object raw, Type type)
    {
        try
        {
            return Convert.ChangeType(raw, type);
        }
        catch (Exception ex)
        {
            throw new MalformedPropertyValueException(name, ex);
        }
    }

    public object Get(string key)
    {
        Property prop = GetPropertyDef(key);

        if (_instantiatedProperties.TryGetValue(key, out object cached))
            return cached;

        if (_uninstantiatedProperties.TryGetValue(key, out object pval))
        {
            (object value, bool canBeCached) = Evaluate(pval);
            
            if (prop != null)
            {
                value = ValidateAndConvert(key, value, prop.Type);

                if (canBeCached || prop.CanBeCached)
                {
                    if (_instantiatedProperties.TryAdd(key, value))
                        _uninstantiatedProperties.Remove(key);
                }
            }

            return value;
        }

        if (prop != null)
        {
            return prop.Default;
        }

        return null;
    }

    public void Set(string key, object value)
    {
        _uninstantiatedProperties.Remove(key);
        _instantiatedProperties.Remove(key);

        // Overridden values are always treated as cachable
        _instantiatedProperties.Add(key, value);
    }
}

[Serializable]
[XmlRoot("DassieConfig")]
public partial class DassieConfig
{
    public const string CurrentFormatVersion = "1.0";

    private readonly PropertyStore _store;
    
    public DassieConfig()
    {
        _store = PropertyStore.Empty;
    }

    public DassieConfig(PropertyStore store)
    {
        _store = store;
    }

    public virtual void SetProperty<T>(string name, T value)
    {
        this[name] = value;
    }

    public virtual T GetProperty<T>(string name)
    {
        return (T)this[name];
    }

    public object this[string key]
    {
        get
        {
            return _store.Get(key);
        }
        
        set
        {
            _store.Set(key, value);
        }

    }

    public static Property[] DefaultPropertyRegistrations { get; } =
    [
        new("Imports", typeof(Import[]), Array.Empty<Import>(), Description: "A list of configuration files to import macros from."),
        new("MacroDefinitions", typeof(Define[]), Array.Empty<Define>(), Description: "Sets custom macro definitions."),
        new("ProjectGroup", typeof(ProjectGroup), null, Description: "Specifies that the config file defines a project group and sets project group-specific options."),
        new("References", typeof(Reference[]), Array.Empty<Reference>(), Description: "Manages references to external assemblies."),
        new("Resources", typeof(Resource[]), Array.Empty<Resource>(), Description: "Manages references to external resources."),
        new("RootNamespace", typeof(string), Description: "Used by editors to set the default namespace of source files."),
        new("AssemblyFileName", typeof(string), Description: "Sets the name of the ouput assembly (without file extension)."),
        new("ApplicationType", typeof(string), "Console", Description: "Sets the application type and subsystem of the program."),
        new("Runtime", typeof(Runtime), Runtime.Jit, Description: "Sets the runtime of the application. Valid values are 'Jit' and 'Aot'."),
        new("Platform", typeof(Platform), Platform.Auto, Description: "Sets the processor architecture of the application."),
        new("RuntimeIdentifier", typeof(string), "", Description: "Sets the RID of the application to be used by the AOT compiler."),
        new("VersionInfo", typeof(List<VersionInfo>), Description: "Sets version information fields for the application."),
        new("IconFile", typeof(string), "", Description: "Sets the application icon file."),
        new("BuildDirectory", typeof(string), "./build", Description: "Sets the directory where the compiled assemblies will be placed."),
        new("EmitPdb", typeof(bool), false, Description: "Toggles generation of debug symbol data."),
        new("IgnoreAllMessages", typeof(bool), false, Description: "If set, all 'information' messages are ignored."),
        new("IgnoreAllWarnings", typeof(bool), false, Description: "If set, all 'warning' messages are ignored."),
        new("TreatWarningsAsErrors", typeof(bool), false, Description: "If set, all 'warning' messages will be treated as errors."),
        new("ILOptimizations", typeof(bool), true, Description: "Toggles optimizations done to the generated IL."),
        new("MeasureElapsedTime", typeof(bool), false, Description: "If set, displays the elapsed time after the completion of a build."),
        new("Configuration", typeof(ApplicationConfiguration), ApplicationConfiguration.Debug, Description: "Sets the configuration of the generated assembly. Valid values are 'Debug' and 'Release'."),
        new("IncludeDependencies", typeof(bool), false, Description: "If set, all referenced assemblies are copied to the build output directory."),
        new("AdvancedErrorMessages", typeof(bool), false, Description: "If set, includes a preview of the error location in all error messages."),
        new("EnableTips", typeof(bool), true, Description: "Toggles further information on some error messages."),
        new("PersistentResourceFile", typeof(bool), false, Description: "If set, does not delete the generated native '.res' file after the build is completed."),
        new("PersistentResourceScript", typeof(bool), false, Description: "If set, does not delete the generated '.rc' file after the build is completed."),
        new("KeepIntermediateFiles", typeof(bool), false, Description: "If set, does not delete intermediate source files."),
        new("EnableMessageTimestamps", typeof(bool), false, Description: "If set, prepends a time stamp to all compiler messages."),
        new("Verbosity", typeof(int), 1, Description: "Sets the verbosity of compiler messages. Valid values are 0-3 (both inclusive)."),
        new("BuildLogDevices", typeof(BuildLogOptions), null, Description: "A list of log devices to write build messages to."),
        new("GenerateILFiles", typeof(bool), false, Description: "If set, generates CIL files in human-readable form."),
        new("GenerateNativeAppHost", typeof(bool), true, Description: "If set, generates native executables along with the .NET assemblies."),
        new("IgnoredMessages", typeof(Ignore[]), Array.Empty<Ignore>(), Description: "A list of error codes that are ignored and will never be emitted."),
        new("AssemblyManifest", typeof(string), Description: "The path to a .manifest file containing Windows-specific configuration."),
        new("ImplicitImports", typeof(bool), true, Description: "If set, implicitly imports parts of the standard library in every source file."),
        new("ImplicitTypeAliases", typeof(bool), true, Description: "If set, allows the use of type aliases such as 'int' for 'System.Int32'."),
        new("PrintExceptionInfo", typeof(bool), false, Description: "If set, prints the full exception message and stack trace if an exception occurs during compilation."),
        new("CacheSourceFiles", typeof(bool), false, Description: "If set, caches source files for limited incremental compilation capabilities."),
        new("BuildProfiles", typeof(BuildProfile[]), Array.Empty<BuildProfile>(), Description: "Sets and manages build profiles."),
        new("DebugProfiles", typeof(DebugProfile[]), Array.Empty<DebugProfile>(), Description: "Sets and manages debug profiles."),
        new("ErrorColor", typeof(string), null, Description: "The color used for error messages (#RRGGBB)."),
        new("WarningColor", typeof(string), null, Description: "The color used for warning messages (#RRGGBB)."),
        new("MessageColor", typeof(string), null, Description: "The color used for information messages (#RRGGBB)."),
        new("EnableOverflowChecks", typeof(bool), false, Description: "If set, arithmetic operations check for overflow and throw an appropriate exception at runtime."),
        new("CodeAnalysis", typeof(CodeAnalysisConfiguration), null, Description: "Used to configure the default code analyzer."),
        new("EntryPoint", typeof(string), "", Description: "Sets the function or method that functions as the application entry point."),
        new("RunAnalyzers", typeof(bool), false, Description: "If enabled, runs code analyzers before compilation."),
        new("NoStdLib", typeof(bool), false, Description: "If enabled, the Dassie standard library (Dassie.Core.dll) is not implicitly referenced."),
        new("EnableSeverityIndicators", typeof(bool), false, Description: "If enabled, compiler messages will display an icon to distinguish them by severity."),
        new("Extensions", typeof(List<Extension>), null, Description: "Configures compiler extensions enabled only during the compilation."),
        new("DocumentSources", typeof(DocumentSourceList), null, Description: "Configures the active document sources provided by compiler extensions."),
        new("MaxErrors", typeof(int), 0, Description: "Once this number of compilation errors is reached, the build process is stopped immediately."),
    ];

    [Description("Specifies the version of the dsconfig format to use.")]
    [XmlAttribute("FormatVersion")]
    public virtual string FormatVersion { get; set; }

    [Description("Loads the specified configuration file and applies its settings.")]
    [XmlAttribute]
    public virtual string Base { get; set; }

    [Description("A list of configuration files to import macros from.")]
    [XmlElement]
    public virtual Import[] Imports { get; set; }

    [Description("Sets custom macro definitions.")]
    [DefaultValue(null)]
    [XmlArray("MacroDefinitions")]
    [XmlArrayItem(Type = typeof(Define))]
    public virtual Define[] MacroDefinitions { get; set; }

    [Description("Specifies that the config file defines a project group and sets project group-specific options.")]
    [DefaultValue(null)]
    [XmlElement]
    public virtual ProjectGroup ProjectGroup { get; set; }

    [Description("Manages references to external assemblies.")]
    [XmlArray("References")]
    [XmlArrayItem(Type = typeof(AssemblyReference))]
    [XmlArrayItem(Type = typeof(PackageReference))]
    [XmlArrayItem(Type = typeof(ProjectReference))]
    public virtual Reference[] References { get; set; }

    [Description("Manages references to external resources.")]
    [XmlArray]
    [XmlArrayItem(Type = typeof(ManagedResource))]
    [XmlArrayItem(Type = typeof(UnmanagedResource))]
    public virtual Resource[] Resources { get; set; }

    [Description("Used by editors to set the default namespace of source files.")]
    [XmlElement("RootNamespace")]
    public virtual string RootNamespace { get; set; }

    [Description("Sets the name of the ouput assembly (without file extension).")]
    [XmlElement("AssemblyFileName")]
    public virtual string AssemblyFileName { get; set; }

    [Description("Sets the application type and subsystem of the program.")]
    [XmlElement("ApplicationType")]
    [DefaultValue("Console")]
    public virtual string ApplicationType { get; set; } = "Console";

    [Description("Sets the runtime of the application. Valid values are 'Jit' and 'Aot'.")]
    [XmlElement]
    [DefaultValue(Runtime.Jit)]
    public virtual Runtime Runtime { get; set; }

    [Description("Sets the processor architecture of the application.")]
    [XmlElement]
    [DefaultValue(Platform.Auto)]
    public virtual Platform Platform { get; set; }

    [Description("Sets the RID of the application to be used by the AOT compiler.")]
    [XmlElement]
    [DefaultValue("")]
    public virtual string RuntimeIdentifier { get; set; }

    [Description("Sets version information fields for the application.")]
    [XmlElement("VersionInfo")]
    public virtual List<VersionInfo> VersionInfo { get; set; }

    [Description("Sets the application icon file.")]
    [DefaultValue("")]
    [XmlElement]
    public virtual string IconFile { get; set; }

    [Description("Sets the directory where the compiled assemblies will be placed.")]
    [XmlElement("BuildDirectory")]
    [DefaultValue("./build")]
    public virtual string BuildDirectory { get; set; } = "./build";

    [Description("Toggles generation of debug symbol data.")]
    [DefaultValue(false)]
    [XmlElement("EmitPdb")]
    public virtual bool EmitPdb { get; set; }

    [Description("If set, all 'information' messages are ignored.")]
    [DefaultValue(false)]
    [XmlElement("IgnoreAllMessages")]
    public virtual bool IgnoreAllMessages { get; set; }

    [Description("If set, all 'warning' messages are ignored.")]
    [DefaultValue(false)]
    [XmlElement("IgnoreAllWarnings")]
    public virtual bool IgnoreAllWarnings { get; set; }

    [Description("If set, all 'warning' messages will be treated as errors.")]
    [DefaultValue(false)]
    [XmlElement("TreatWarningsAsErrors")]
    public virtual bool TreatWarningsAsErrors { get; set; }

    [Description("Toggles optimizations done to the generated IL.")]
    [DefaultValue(true)]
    [XmlElement("ILOptimizations")]
    public virtual bool ILOptimizations { get; set; } = true;

    [Description("If set, displays the elapsed time after the completion of a build.")]
    [DefaultValue(false)]
    [XmlElement("MeasureElapsedTime")]
    public virtual bool MeasureElapsedTime { get; set; }

    [Description("Sets the configuration of the generated assembly. Valid values are 'Debug' and 'Release'.")]
    [XmlElement("Configuration")]
    [DefaultValue(ApplicationConfiguration.Debug)]
    public virtual ApplicationConfiguration Configuration { get; set; }

    [Description("If set, all referenced assemblies are copied to the build output directory.")]
    [DefaultValue(false)]
    [XmlElement("IncludeDependencies")]
    public virtual bool IncludeDependencies { get; set; }

    [Description("If set, includes a preview of the error location in all error messages.")]
    [DefaultValue(false)]
    [XmlElement("AdvancedErrorMessages")]
    public virtual bool AdvancedErrorMessages { get; set; }

    [Description("Toggles further information on some error messages.")]
    [DefaultValue(true)]
    [XmlElement("EnableTips")]
    public virtual bool EnableTips { get; set; } = true;

    [Description("If set, does not delete the generated native '.res' file after the build is completed.")]
    [DefaultValue(false)]
    [XmlElement("PersistentResourceFile")]
    public virtual bool PersistentResourceFile { get; set; }

    [Description("If set, does not delete the generated '.rc' file after the build is completed.")]
    [DefaultValue(false)]
    [XmlElement]
    public virtual bool PersistentResourceScript { get; set; }

    [Description("If set, does not delete intermediate source files.")]
    [DefaultValue(false)]
    [XmlElement("KeepIntermediateFiles")]
    public virtual bool KeepIntermediateFiles { get; set; } = false;

    [Description("If set, prepends a time stamp to all compiler messages.")]
    [DefaultValue(false)]
    [XmlElement]
    public virtual bool EnableMessageTimestamps { get; set; } = false;

    [Description("Sets the verbosity of compiler messages. Valid values are 0-3 (both inclusive).")]
    [DefaultValue(1)]
    [XmlElement]
    public virtual int Verbosity { get; set; } = 1;

    [Description("A list of log devices to write build messages to.")]
    [DefaultValue(null)]
    [XmlElement("BuildLogDevices")]
    public virtual BuildLogOptions BuildLogDevices { get; set; }

    [Description("If set, generates CIL files in human-readable form.")]
    [DefaultValue(false)]
    [XmlElement]
    public virtual bool GenerateILFiles { get; set; } = false;

    [Description("If set, generates native executables along with the .NET assemblies.")]
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool GenerateNativeAppHost { get; set; } = true;

    [Description("A list of error codes that are ignored and will never be emitted.")]
    [XmlArray("IgnoredMessages")]
    [XmlArrayItem(typeof(Message))]
    [XmlArrayItem(typeof(Warning))]
    public virtual Ignore[] IgnoredMessages { get; set; }

    [Description("The path to a .manifest file containing Windows-specific configuration.")]
    [XmlElement]
    public virtual string AssemblyManifest { get; set; }

    [Description("If set, implicitly imports parts of the standard library in every source file.")]
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool ImplicitImports { get; set; } = true;

    [Description("If set, allows the use of type aliases such as 'int' for 'System.Int32'.")]
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool ImplicitTypeAliases { get; set; } = true;

    [Description("If set, prints the full exception message and stack trace if an exception occurs during compilation.")]
    [DefaultValue(false)]
    [XmlElement]
    public virtual bool PrintExceptionInfo { get; set; } = false;

    [Description("If set, caches source files for limited incremental compilation capabilities.")]
    [DefaultValue(false)]
    [XmlElement]
    public virtual bool CacheSourceFiles { get; set; } = false;

    [Description("Sets and manages build profiles.")]
    [DefaultValue(null)]
    [XmlArray]
    public virtual BuildProfile[] BuildProfiles { get; set; }

    [Description("Sets and manages debug profiles.")]
    [DefaultValue(null)]
    [XmlArray]
    public virtual DebugProfile[] DebugProfiles { get; set; }

    [Description("The color used for error messages (#RRGGBB).")]
    [DefaultValue(null)]
    [XmlElement]
    public virtual string ErrorColor { get; set; }

    [Description("The color used for warning messages (#RRGGBB).")]
    [DefaultValue(null)]
    [XmlElement]
    public virtual string WarningColor { get; set; }

    [Description("The color used for information messages (#RRGGBB).")]
    [DefaultValue(null)]
    [XmlElement]
    public virtual string MessageColor { get; set; }

    [Description("If set, arithmetic operations check for overflow and throw an appropriate exception at runtime.")]
    [DefaultValue(false)]
    [XmlElement]
    public virtual bool EnableOverflowChecks { get; set; }

    [Description("Used to configure the default code analyzer.")]
    [DefaultValue(null)]
    [XmlElement("CodeAnalysis")]
    public virtual CodeAnalysisConfiguration CodeAnalysisConfiguration { get; set; }

    [Description("Sets the function or method that functions as the application entry point.")]
    [DefaultValue("")]
    [XmlElement]
    public virtual string EntryPoint { get; set; }

    [Description("If enabled, runs code analyzers before compilation.")]
    [DefaultValue(false)]
    [XmlElement]
    public virtual bool RunAnalyzers { get; set; }

    [Description("If enabled, the Dassie standard library (Dassie.Core.dll) is not implicitly referenced.")]
    [DefaultValue(false)]
    [XmlElement]
    public virtual bool NoStdLib { get; set; }

    [Description("If enabled, compiler messages will display an icon to distinguish them by severity.")]
    [DefaultValue(false)]
    [XmlElement]
    public virtual bool EnableSeverityIndicators { get; set; }

    [Description("Configures compiler extensions enabled only during the compilation.")]
    [DefaultValue(null)]
    [XmlArray]
    public virtual List<Extension> Extensions { get; set; }

    [Description("Configures the active document sources provided by compiler extensions.")]
    [DefaultValue(null)]
    [XmlElement]
    public virtual DocumentSourceList DocumentSources { get; set; }

    [Description("Once this number of compilation errors is reached, the build process is stopped immediately.")]
    [DefaultValue(0)]
    [XmlElement]
    public virtual int MaxErrors { get; set; }
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