using System;
using System.ComponentModel;
using System.Xml.Serialization;
namespace Dassie.Configuration;

[Serializable]
[XmlRoot("PackageReference")]
public class PackageReference : Reference
{
    [DefaultValue("")]
    [XmlAttribute("Version")]
    public string Version { get; set; } = "";

    [XmlText]
    public string PackageId { get; set; }
}