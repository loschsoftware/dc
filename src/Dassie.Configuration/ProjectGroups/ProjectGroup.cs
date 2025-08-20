using System.Xml;

namespace Dassie.Configuration.ProjectGroups;

[Serializable]
[XmlRoot]
public class ProjectGroup
{
    [Serializable]
    [XmlRoot]
    public class TargetList
    {
        [XmlAnyElement]
        public XmlNode[] Targets { get; set; }
    }

    [XmlArray]
    [XmlArrayItem(typeof(Project))]
    [XmlArrayItem(typeof(ProjectGroupComponent))]
    public Component[] Components { get; set; }

    [XmlElement("Executable")]
    public string ExecutableComponent { get; set; }

    [XmlElement]
    public TargetList Targets { get; set; }
}