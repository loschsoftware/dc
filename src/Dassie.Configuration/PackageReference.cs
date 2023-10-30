using System;
using System.Xml.Serialization;
namespace Dassie.Configuration;

[Serializable]
[XmlRoot("PackageReference")]
public class PackageReference : Reference
{
    [XmlAttribute("Version")]
    public string Version { get; set; }

    [XmlText]
    public string PackageId { get; set; }
}