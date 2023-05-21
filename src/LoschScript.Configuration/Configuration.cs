using System;
using System.Xml.Serialization;
namespace Losch.LoschScript.Configuration;

[Serializable]
[XmlRoot("LSConfig")]
public sealed class LSConfig
{
    [XmlArray("References")]
    [XmlArrayItem(Type = typeof(AssemblyReference))]
    [XmlArrayItem(Type = typeof(FileReference))]
    public Reference[] References { get; set; }

    [XmlElement("DefaultNamespace")]
    public string DefaultNamespace { get; set; }

    [XmlElement("AssemblyFileName")]
    public string AssemblyName { get; set; }

    [XmlElement("ApplicationType")]
    public ApplicationType ApplicationType { get; set; }

    [XmlElement("IconFile")]
    public string ApplicationIcon { get; set; }

    [XmlElement("Product")]
    public string Product { get; set; }

    [XmlElement("Company")]
    public string Company { get; set; }

    [XmlElement("Copyright")]
    public string Copyright { get; set; }

    [XmlElement("Trademark")]
    public string Trademark { get; set; }

    [XmlElement("Version")]
    public string Version { get; set; }

    [XmlElement("BuildDirectory")]
    public string BuildOutputDirectory { get; set; }

    [XmlElement("EmitPdb")]
    public bool CreatePdb { get; set; } = false;

    [XmlElement("IgnoreMessages")]
    public bool IgnoreMessages { get; set; }
    
    [XmlElement("IgnoreWarnings")]
    public bool IgnoreWarnings { get; set; }
    
    [XmlElement("TreatWarningsAsErrors")]
    public bool TreatWarningsAsErrors { get; set; }

    [XmlElement("IlOptimizations")]
    public bool IlOptimizations { get; set; } = true;

    [XmlElement("Configuration")]
    public Configuration Configuration { get; set; }

    [XmlElement("VersionInformation")]
    public VersionInformation VersionInformation { get; set; }

    [XmlElement("IncludeDependencies")]
    public bool IncludeDependencies { get; set; }
}

[Serializable]
public enum ApplicationType
{
    Console,
    WinExe,
    Library
}

[Serializable]
public enum Configuration
{
    Debug,
    Release
}