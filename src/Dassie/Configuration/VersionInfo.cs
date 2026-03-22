using System;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Dassie.Configuration;

/// <summary>
/// Represents version information.
/// </summary>
[Serializable]
[XmlRoot]
public partial class VersionInfo : ConfigObject
{
    /// <inheritdoc/>
    public VersionInfo(PropertyStore store) : base(store) { }

    /// <summary>
    /// The language of the version info resource.
    /// </summary>
    [XmlAttribute]
    [DefaultValue("en-US")]
    [ConfigProperty]
    public partial string Language { get; set; }

    /// <summary>
    /// The locale identifier of the version info resource.
    /// </summary>
    [XmlAttribute]
    [DefaultValue(1033)]
    [ConfigProperty]
    public partial int Lcid { get; set; }

    /// <summary>
    /// The product designation of the application.
    /// </summary>
    [XmlElement]
    [ConfigProperty]
    public partial string Product { get; set; }

    /// <summary>
    /// The author (company) of the application.
    /// </summary>
    [XmlElement]
    [ConfigProperty]
    public partial string Company { get; set; }

    /// <summary>
    /// The copyright string.
    /// </summary>
    [XmlElement]
    [ConfigProperty]
    public partial string Copyright { get; set; }

    /// <summary>
    /// The trademark.
    /// </summary>
    [XmlElement]
    [ConfigProperty]
    public partial string Trademark { get; set; }

    /// <summary>
    /// The product version.
    /// </summary>
    [XmlElement]
    [ConfigProperty]
    public partial string Version { get; set; }

    /// <summary>
    /// The file version.
    /// </summary>
    [XmlElement]
    [ConfigProperty]
    public partial string FileVersion { get; set; }

    /// <summary>
    /// A description of the application.
    /// </summary>
    [XmlElement]
    [ConfigProperty]
    public partial string Description { get; set; }

    /// <summary>
    /// The internal name of the application.
    /// </summary>
    [XmlElement]
    [ConfigProperty]
    public partial string InternalName { get; set; }
}