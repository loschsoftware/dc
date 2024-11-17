namespace Dassie.Configuration;

[Serializable]
[XmlRoot]
public class VersionInfo
{
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

    [XmlElement("FileVersion")]
    public string FileVersion { get; set; }

    [XmlElement("Description")]
    public string Description { get; set; }

    [XmlElement("InternalName")]
    public string InternalName { get; set; }
}