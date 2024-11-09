using System;
using System.Xml.Serialization;
namespace Dassie.Configuration;

[Serializable]
[XmlRoot("FileReference")]
public class FileReference : Reference
{
    [XmlText]
    public string FileName { get; set; }

    [XmlAttribute("CopyToOutput")]
    public bool CopyToOutput { get; set; }
}