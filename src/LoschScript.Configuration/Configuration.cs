﻿using LoschScript.Configuration;
using System;
using System.ComponentModel;
using System.Xml.Serialization;
namespace Losch.LoschScript.Configuration;

[Serializable]
[XmlRoot("LSConfig")]
public sealed class LSConfig
{
    [XmlAttribute("FormatVersion")]
    public string FormatVersion { get; set; }

    [XmlArray("References")]
    [XmlArrayItem(Type = typeof(AssemblyReference))]
    [XmlArrayItem(Type = typeof(FileReference))]
    public Reference[] References { get; set; }

    [XmlArray]
    [XmlArrayItem(Type = typeof(ManagedResource))]
    [XmlArrayItem(Type = typeof(UnmanagedResource))]
    public Resource[] Resources { get; set; }

    [XmlElement("DefaultNamespace")]
    public string DefaultNamespace { get; set; }

    [XmlElement("AssemblyFileName")]
    public string AssemblyName { get; set; }

    [XmlElement("ApplicationType")]
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

    [XmlElement("VersionInformation")]
    public VersionInformation VersionInformation { get; set; }

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

    [XmlArray("IgnoredMessages")]
    [XmlArrayItem(typeof(Message))]
    [XmlArrayItem(typeof(Warning))]
    public Ignore[] IgnoredMessages { get; set; }

    [XmlArray("CodeAnalyzers")]
    public CodeAnalyzer[] CodeAnalyzers { get; set; }

    [XmlElement]
    public string AssemblyManifest { get; set; }
}

[Serializable]
public enum ApplicationType
{
    Console,
    WinExe,
    Library
}

[Serializable]
public enum ApplicationConfiguration
{
    Debug,
    Release
}