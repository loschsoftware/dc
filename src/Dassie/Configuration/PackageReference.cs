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
    /// <inheritdoc/>
    public PackageReference(PropertyStore store) : base(store) { }

    /// <summary>
    /// The version of the package to import.
    /// </summary>
    [DefaultValue("")]
    [XmlAttribute]
    public string Version
    {
        get => Get<string>(nameof(Version));
        set => Set(nameof(Version), value);
    }

    /// <summary>
    /// The identifier of the package to import.
    /// </summary>
    [XmlText]
    public string PackageId
    {
        get => Get<string>(nameof(PackageId));
        set => Set(nameof(PackageId), value);
    }
}