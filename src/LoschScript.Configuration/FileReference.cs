using System;
using System.Xml.Serialization;
namespace Losch.LoschScript.Configuration;

[Serializable]
[XmlRoot("FileReference")]
public sealed class FileReference : Reference
{
    [XmlText]
    public string FileName { get; set; }
    
    [XmlAttribute("CopyToOutput")]
    public bool CopyToOutput { get; set; }
}