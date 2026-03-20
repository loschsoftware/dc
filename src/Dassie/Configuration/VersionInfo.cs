using System;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Dassie.Configuration;

/// <summary>
/// Represents version information.
/// </summary>
[Serializable]
[XmlRoot]
public class VersionInfo : ConfigObject
{
    /// <inheritdoc/>
    public VersionInfo(PropertyStore store) : base(store) { }

    /// <summary>
    /// The language of the version info resource.
    /// </summary>
    [XmlAttribute]
    [DefaultValue("en-US")]
    public string Language
    {
        get => Get<string>(nameof(Language));
        set => Set(nameof(Language), value);
    }

    /// <summary>
    /// The locale identifier of the version info resource.
    /// </summary>
    [XmlAttribute]
    [DefaultValue(1033)]
    public int Lcid
    {
        get => Get<int>(nameof(Lcid));
        set => Set(nameof(Lcid), value);
    }

    /// <summary>
    /// The product designation of the application.
    /// </summary>
    [XmlElement]
    public string Product
    {
        get => Get<string>(nameof(Product));
        set => Set(nameof(Product), value);
    }

    /// <summary>
    /// The author (company) of the application.
    /// </summary>
    [XmlElement]
    public string Company
    {
        get => Get<string>(nameof(Company));
        set => Set(nameof(Company), value);
    }

    /// <summary>
    /// The copyright string.
    /// </summary>
    [XmlElement]
    public string Copyright
    {
        get => Get<string>(nameof(Copyright));
        set => Set(nameof(Copyright), value);
    }

    /// <summary>
    /// The trademark.
    /// </summary>
    [XmlElement]
    public string Trademark
    {
        get => Get<string>(nameof(Trademark));
        set => Set(nameof(Trademark), value);
    }

    /// <summary>
    /// The product version.
    /// </summary>
    [XmlElement]
    public string Version
    {
        get => Get<string>(nameof(Version));
        set => Set(nameof(Version), value);
    }

    /// <summary>
    /// The file version.
    /// </summary>
    [XmlElement]
    public string FileVersion
    {
        get => Get<string>(nameof(FileVersion));
        set => Set(nameof(FileVersion), value);
    }

    /// <summary>
    /// A description of the application.
    /// </summary>
    [XmlElement]
    public string Description
    {
        get => Get<string>(nameof(Description));
        set => Set(nameof(Description), value);
    }

    /// <summary>
    /// The internal name of the application.
    /// </summary>
    [XmlElement]
    public string InternalName { get; set; }
}