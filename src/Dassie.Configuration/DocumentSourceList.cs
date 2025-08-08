using System.Collections.Generic;
using System.Xml;

namespace Dassie.Configuration;

[XmlRoot("DocumentSources")]
[Serializable]
public class DocumentSourceList
{
    [XmlAnyElement]
    public List<XmlElement> Sources { get; set; }
}