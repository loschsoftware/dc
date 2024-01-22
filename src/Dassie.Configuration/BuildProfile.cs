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
    public string Arguments { get; set; }

    [XmlArray]
    public BuildEvent[] PreBuildEvents { get; set; }

    [XmlArray]
    public BuildEvent[] PostBuildEvents { get; set; }
}