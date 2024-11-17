using Dassie.Configuration.ProjectGroups;

namespace Dassie.Configuration.Build.Targets;

[Serializable]
[XmlRoot]
public class Directory : DeploymentTarget
{
    [XmlText]
    public string Path { get; set; }
}