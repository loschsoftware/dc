using System;
using System.Xml.Serialization;

namespace Dassie.Configuration.ProjectGroups;

/// <summary>
/// An abstract base class for all project group components.
/// </summary>
[XmlRoot]
[Serializable]
public abstract class Component { }

/// <summary>
/// Represents a project that is part of a project group.
/// </summary>
[XmlRoot]
[Serializable]
public class Project : Component
{
    /// <summary>
    /// The path to the project file.
    /// </summary>
    [XmlText]
    public string Path { get; set; }
}

/// <summary>
/// Represents a project group that is part of a project group.
/// </summary>
[XmlRoot("ProjectGroup")]
[Serializable]
public class ProjectGroupComponent : Component
{
    /// <summary>
    /// The path to the project group file.
    /// </summary>
    [XmlText]
    public string Path { get; set; }
}