using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Dassie.Configuration;

[Serializable]
[XmlRoot("Devices")]
public class BuildLogOptions
{
    [XmlAnyElement]
    public List<XmlElement> Elements { get; set; }
}