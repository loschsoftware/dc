using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

[Serializable]
[XmlRoot]
public class Import
{
    [XmlAttribute]
    public string Path { get; set; }
}