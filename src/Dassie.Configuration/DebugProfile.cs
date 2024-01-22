using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

[Serializable]
[XmlRoot]
public class DebugProfile
{
    [XmlAttribute]
    public string Name { get; set; }

    [XmlElement]
    public string Arguments { get; set; }

    [XmlElement]
    public string WorkingDirectory { get; set; }
}