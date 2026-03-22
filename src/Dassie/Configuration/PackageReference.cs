using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a package reference.
/// </summary>
[Serializable]
[XmlRoot("PackageReference")]
public partial class PackageReference : Reference
{
    /// <inheritdoc/>
    public PackageReference(PropertyStore store) : base(store) { }

    /// <summary>
    /// The version of the package to import.
    /// </summary>
    [DefaultValue("")]
    [XmlAttribute]
    [ConfigProperty]
    public partial string Version { get; set; }

    /// <summary>
    /// The identifier of the package to import.
    /// </summary>
    [XmlText]
    [ConfigProperty]
    public partial string PackageId { get; set; }
}