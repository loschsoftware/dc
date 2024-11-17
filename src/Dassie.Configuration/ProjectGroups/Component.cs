namespace Dassie.Configuration.ProjectGroups;

[XmlRoot]
[Serializable]
public abstract class Component { }

[XmlRoot]
[Serializable]
public class Project : Component
{
    [XmlText]
    public string Path { get; set; }
}

[XmlRoot("ProjectGroup")]
[Serializable]
public class ProjectGroupComponent : Component
{
    [XmlText]
    public string Path { get; set; }
}