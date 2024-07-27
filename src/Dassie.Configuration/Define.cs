using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

[XmlRoot("Define")]
[Serializable]
public class Define
{
    [XmlAttribute("Macro")]
    public string Name { get; set; }

    [XmlText]
    public string Value { get; set; }
}