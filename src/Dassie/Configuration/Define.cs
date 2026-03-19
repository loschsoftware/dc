using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

[XmlRoot("Define")]
[Serializable]
public class Define
{
    [XmlAttribute("Macro")]
    public string Name { get; set; }

    [XmlAttribute]
    public string Parameters { get; set; }

    [XmlAttribute]
    public bool Trim { get; set; }

    [XmlText]
    public string Value { get; set; }
}