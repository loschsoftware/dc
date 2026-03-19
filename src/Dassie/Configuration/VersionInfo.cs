using System;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Dassie.Configuration;

/// <summary>
/// Represents version information.
/// </summary>
[Serializable]
[XmlRoot]
public class VersionInfo
{
    /// <summary>
    /// The language of the version info resource.
    /// </summary>
    [XmlAttribute]
    [DefaultValue("en-US")]
    public string Language { get; set; }

    /// <summary>
    /// The locale identifier of the version info resource.
    /// </summary>
    [XmlAttribute]
    [DefaultValue(1033)]
    public int Lcid { get; set; }

    /// <summary>
    /// The product designation of the application.
    /// </summary>
    [XmlElement("Product")]
    public string Product { get; set; }

    /// <summary>
    /// The author (company) of the application.
    /// </summary>
    [XmlElement("Company")]
    public string Company { get; set; }

    /// <summary>
    /// The copyright string.
    /// </summary>
    [XmlElement("Copyright")]
    public string Copyright { get; set; }

    /// <summary>
    /// The trademark.
    /// </summary>
    [XmlElement("Trademark")]
    public string Trademark { get; set; }

    /// <summary>
    /// The product version.
    /// </summary>
    [XmlElement("Version")]
    public string Version { get; set; }

    /// <summary>
    /// The file version.
    /// </summary>
    [XmlElement("FileVersion")]
    public string FileVersion { get; set; }

    /// <summary>
    /// A description of the application.
    /// </summary>
    [XmlElement("Description")]
    public string Description { get; set; }

    /// <summary>
    /// The internal name of the application.
    /// </summary>
    [XmlElement("InternalName")]
    public string InternalName { get; set; }
}