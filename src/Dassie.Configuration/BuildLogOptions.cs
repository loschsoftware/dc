using System.Collections.Generic;
using System.Xml;

namespace Dassie.Configuration;

[Serializable]
[XmlRoot("Devices")]
public class BuildLogOptions
{
    [XmlAnyElement]
    public List<XmlElement> Elements { get; set; }
}
