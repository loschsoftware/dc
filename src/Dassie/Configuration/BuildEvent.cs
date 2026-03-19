using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Dassie.Configuration;

[Serializable]
[XmlRoot]
public class BuildEvent
{
    [DefaultValue("")]
    [XmlAttribute]
    public string Name { get; set; }

    [DefaultValue(true)]
    [XmlAttribute]
    public bool Critical { get; set; }

    [XmlAnyElement]
    public List<XmlElement> CommandNodes { get; set; }
}