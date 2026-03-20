using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a transient extension definition.
/// </summary>
[XmlRoot]
[Serializable]
public class Extension : ConfigObject
{
    /// <inheritdoc/>
    public Extension(PropertyStore store) : base(store) { }

    /// <summary>
    /// The path of the extension assembly.
    /// </summary>
    [XmlAttribute]
    public string Path
    {
        get => Get<string>(nameof(Path));
        set => Set(nameof(Path), value);
    }

    /// <summary>
    /// A list of XML attributes passed to the extension.
    /// </summary>
    [XmlAnyAttribute]
    public List<XmlAttribute> Attributes
    {
        get => Get<List<XmlAttribute>>(nameof(Attributes));
        set => Set(nameof(Attributes), value);
    }

    /// <summary>
    /// A list of XML elements passed to the extension.
    /// </summary>
    [XmlAnyElement]
    public List<XmlElement> Elements
    {
        get => Get<List<XmlElement>>(nameof(Elements));
        set => Set(nameof(Elements), value);
    }
}