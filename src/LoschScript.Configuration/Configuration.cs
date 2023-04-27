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
    public Reference[] References { get; set; } = new Reference[] { };

    [XmlElement("DefaultNamespace")]
    public string DefaultNamespace { get; set; } = "asm";

    [XmlElement("AssemblyFileName")]
    public string AssemblyName { get; set; } = "asm";

    [XmlElement("ApplicationType")]
    public ApplicationType ApplicationType { get; set; } = ApplicationType.Console;

    [XmlElement("Icon")]
    public string ApplicationIcon { get; set; }

    [XmlElement("Version")]
    public string Version { get; set; } = "1.0.0.0";

    [XmlElement("BuildDirectory")]
    public string BuildOutputDirectory { get; set; } = ".\\build";

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

    [XmlElement("VersionInformation")]
    public VersionInformation VersionInformation { get; set; }
}

[Serializable]
public enum ApplicationType
{
    Console,
    WinExe,
    Library
}