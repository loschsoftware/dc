using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a list of document transformers.
/// </summary>
[XmlRoot("DocumentTransformers")]
[Serializable]
public partial class DocumentTransformerList : ConfigObject
{
    /// <inheritdoc/>
    public DocumentTransformerList(PropertyStore store) : base(store) { }

    /// <summary>
    /// A list of XML elements representing the document transformers to enable.
    /// </summary>
    [XmlAnyElement]
    [ConfigProperty]
    public partial List<XmlElement> Transformers { get; set; }
}