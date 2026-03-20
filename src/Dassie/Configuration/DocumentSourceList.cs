using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a list of document sources.
/// </summary>
[XmlRoot("DocumentSources")]
[Serializable]
public class DocumentSourceList : ConfigObject
{
    /// <inheritdoc/>
    public DocumentSourceList(PropertyStore store) : base(store) { }

    /// <summary>
    /// A list of XML elements representing the document sources to enable.
    /// </summary>
    [XmlAnyElement]
    public List<XmlElement> Sources
    {
        get => Get<List<XmlElement>>(nameof(Sources));
        set => Set(nameof(Sources), value);
    }
}