using System;
using System.Xml.Serialization;

namespace LoschScript.Configuration;

[Serializable]
[XmlRoot("Analyzer")]
public class CodeAnalyzer
{
    [XmlText]
    public string FullName { get; set; }
}