using System;
using System.Xml.Serialization;
namespace Losch.LoschScript.Configuration;

[Serializable]
[XmlRoot("AssemblyReference")]
public class AssemblyReference : Reference
{
    [XmlText]
    public string AssemblyPath { get; set; }

    [XmlAttribute("CopyToOutput")]
    public bool CopyToOutput { get; set; }

    [XmlAttribute("ImportNamespacesImplicitly")]
    public bool ImportNamespacesImplicitly { get; set; }
}