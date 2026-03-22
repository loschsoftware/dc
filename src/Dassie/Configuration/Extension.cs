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
public partial class Extension : ConfigObject
{
    /// <inheritdoc/>
    public Extension(PropertyStore store) : base(store) { }

    /// <summary>
    /// The path of the extension assembly.
    /// </summary>
    [XmlAttribute]
    [ConfigProperty]
    public partial string Path { get; set; }

    /// <summary>
    /// A list of XML attributes passed to the extension.
    /// </summary>
    [XmlAnyAttribute]
    [ConfigProperty]
    public partial List<XmlAttribute> Attributes { get; set; }

    /// <summary>
    /// A list of XML elements passed to the extension.
    /// </summary>
    [XmlAnyElement]
    [ConfigProperty]
    public partial List<XmlElement> Elements { get; set; }
}