using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

[XmlRoot]
[Serializable]
public class BuildProfile
{
    [XmlAttribute]
    public string Name { get; set; }

    [XmlElement]
    public string Command { get; set; }
}