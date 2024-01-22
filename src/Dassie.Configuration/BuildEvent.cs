using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Dassie.Configuration;

[Serializable]
[XmlRoot]
public class BuildEvent
{
    [XmlElement]
    public string Command { get; set; }

    [DefaultValue(true)]
    [XmlElement]
    public bool Critical { get; set; }
}