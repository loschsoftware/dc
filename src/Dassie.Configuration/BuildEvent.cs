using System.ComponentModel;

namespace Dassie.Configuration;

[Serializable]
[XmlRoot]
public class BuildEvent
{
    [XmlElement]
    public string Command { get; set; }

    [DefaultValue(true)]
    [XmlAttribute]
    public bool Critical { get; set; }

    [DefaultValue(true)]
    [XmlAttribute]
    public bool Hidden { get; set; }

    [DefaultValue(false)]
    [XmlAttribute]
    public bool RunAsAdministrator { get; set; }

    [DefaultValue(true)]
    [XmlAttribute]
    public bool WaitForExit { get; set; }
}