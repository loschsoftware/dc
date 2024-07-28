using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Dassie.Configuration;

[Serializable]
[XmlRoot("DassieConfig")]
public sealed class DassieConfig
{
    public const string CurrentFormatVersion = "1.0";

    [XmlAttribute("FormatVersion")]
    public string FormatVersion { get; set; }

    [DefaultValue(null)]
    [XmlArray("MacroDefinitions")]
    [XmlArrayItem(Type = typeof(Define))]
    public Define[] MacroDefinitions { get; set; }

    [XmlArray("References")]
    [XmlArrayItem(Type = typeof(AssemblyReference))]
    [XmlArrayItem(Type = typeof(FileReference))]
    [XmlArrayItem(Type = typeof(ProjectReference))]
    public Reference[] References { get; set; }

    [XmlArray]
    [XmlArrayItem(Type = typeof(ManagedResource))]
    [XmlArrayItem(Type = typeof(UnmanagedResource))]
    public Resource[] Resources { get; set; }

    [XmlElement("RootNamespace")]
    public string RootNamespace { get; set; }

    [XmlElement("AssemblyFileName")]
    public string AssemblyName { get; set; }

    [XmlElement("ApplicationType")]
    [DefaultValue(null)]
    public ApplicationType ApplicationType { get; set; }

    [XmlElement("VersionInfo")]
    public VersionInfo VersionInfo { get; set; }

    [XmlElement("BuildDirectory")]
    public string BuildOutputDirectory { get; set; }

    [DefaultValue(false)]
    [XmlElement("EmitPdb")]
    public bool CreatePdb { get; set; }

    [DefaultValue(false)]
    [XmlElement("IgnoreAllMessages")]
    public bool IgnoreMessages { get; set; }

    [DefaultValue(false)]
    [XmlElement("IgnoreAllWarnings")]
    public bool IgnoreWarnings { get; set; }

    [DefaultValue(false)]
    [XmlElement("TreatWarningsAsErrors")]
    public bool TreatWarningsAsErrors { get; set; }

    [DefaultValue(true)]
    [XmlElement("IlOptimizations")]
    public bool IlOptimizations { get; set; } = true;

    [DefaultValue(false)]
    [XmlElement("MeasureElapsedTime")]
    public bool MeasureElapsedTime { get; set; }

    [XmlElement("Configuration")]
    public ApplicationConfiguration Configuration { get; set; }

    [DefaultValue(false)]
    [XmlElement("IncludeDependencies")]
    public bool IncludeDependencies { get; set; }

    [DefaultValue(false)]
    [XmlElement("AdvancedErrorMessages")]
    public bool AdvancedErrorMessages { get; set; }

    [DefaultValue(true)]
    [XmlElement("EnableTips")]
    public bool EnableTips { get; set; } = true;

    [DefaultValue(false)]
    [XmlElement("PersistentResourceFile")]
    public bool PersistentResourceFile { get; set; }

    [DefaultValue(false)]
    [XmlElement("KeepIntermediateFiles")]
    public bool KeepIntermediateFiles { get; set; } = false;

    [XmlArray("IgnoredMessages")]
    [XmlArrayItem(typeof(Message))]
    [XmlArrayItem(typeof(Warning))]
    public Ignore[] IgnoredMessages { get; set; }

    [XmlArray("CodeAnalyzers")]
    public CodeAnalyzer[] CodeAnalyzers { get; set; }

    [XmlElement]
    public string AssemblyManifest { get; set; }

    [DefaultValue(true)]
    [XmlElement]
    public bool ImplicitImports { get; set; } = true;

    [DefaultValue(true)]
    [XmlElement]
    public bool ImplicitTypeAliases { get; set; } = true;

    [DefaultValue(false)]
    [XmlElement]
    public bool PrintExceptionInfo { get; set; } = false;

    [DefaultValue(false)]
    [XmlElement]
    public bool CacheSourceFiles { get; set; } = false;

    [DefaultValue(null)]
    [XmlArray]
    public BuildProfile[] BuildProfiles { get; set; }

    [DefaultValue(null)]
    [XmlArray]
    public DebugProfile[] DebugProfiles { get; set; }
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