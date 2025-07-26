using System.Collections.Generic;
using System.Xml;

namespace Dassie.Configuration;

[XmlRoot]
[Serializable]
public class Extension
{
    [XmlAttribute]
    public string Path { get; set; }

    [XmlAnyAttribute]
    public List<XmlAttribute> Attributes { get; set; }

    [XmlAnyElement]
    public List<XmlElement> Elements { get; set; }
}