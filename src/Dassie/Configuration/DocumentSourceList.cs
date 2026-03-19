using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Dassie.Configuration;

[XmlRoot("DocumentSources")]
[Serializable]
public class DocumentSourceList
{
    [XmlAnyElement]
    public List<XmlElement> Sources { get; set; }
}