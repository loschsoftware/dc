using System.ComponentModel;

namespace Dassie.Configuration;

[Serializable]
[XmlRoot]
public class ProjectReference : Reference
{
    [DefaultValue(true)]
    [XmlAttribute]
    public bool CopyToOutput { get; set; } = true;

    [XmlText]
    public string ProjectFile { get; set; }
}