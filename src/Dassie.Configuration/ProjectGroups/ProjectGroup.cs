using Dassie.Configuration.Build.Targets;

namespace Dassie.Configuration.ProjectGroups;

[Serializable]
[XmlRoot]
public class ProjectGroup
{
    [XmlArray]
    [XmlArrayItem(typeof(Project))]
    [XmlArrayItem(typeof(ProjectGroupComponent))]
    public Component[] Components { get; set; }

    [XmlElement("Executable")]
    public string ExecutableComponent { get; set; }

    [XmlArray]
    [XmlArrayItem(typeof(Directory))]
    public DeploymentTarget[] Targets { get; set; }
}