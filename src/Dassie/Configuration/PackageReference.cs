using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a package reference.
/// </summary>
[Serializable]
[XmlRoot("PackageReference")]
public class PackageReference : Reference
{
    /// <summary>
    /// The version of the package to import.
    /// </summary>
    [DefaultValue("")]
    [XmlAttribute("Version")]
    public string Version { get; set; } = "";

    /// <summary>
    /// The identifier of the package to import.
    /// </summary>
    [XmlText]
    public string PackageId { get; set; }
}