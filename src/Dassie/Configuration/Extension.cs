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
public class Extension
{
    /// <summary>
    /// The path of the extension assembly.
    /// </summary>
    [XmlAttribute]
    public string Path { get; set; }

    /// <summary>
    /// A list of XML attributes passed to the extension.
    /// </summary>
    [XmlAnyAttribute]
    public List<XmlAttribute> Attributes { get; set; }

    /// <summary>
    /// A list of XML elements passed to the extension.
    /// </summary>
    [XmlAnyElement]
    public List<XmlElement> Elements { get; set; }
}