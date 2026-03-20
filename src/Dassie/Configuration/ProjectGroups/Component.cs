using System;
using System.Xml.Serialization;

namespace Dassie.Configuration.ProjectGroups;

/// <summary>
/// An abstract base class for all project group components.
/// </summary>
[XmlRoot]
[Serializable]
public abstract class Component : ConfigObject
{
    /// <inheritdoc/>
    protected Component(PropertyStore store) : base(store) { }
}

/// <summary>
/// Represents a project that is part of a project group.
/// </summary>
[XmlRoot]
[Serializable]
public class Project : Component
{
    /// <inheritdoc/>
    public Project(PropertyStore store) : base(store) { }

    /// <summary>
    /// The path to the project file.
    /// </summary>
    [XmlText]
    public string Path
    {
        get => Get<string>(nameof(Path));
        set => Set(nameof(Path), value);
    }
}

/// <summary>
/// Represents a project group that is part of a project group.
/// </summary>
[XmlRoot("ProjectGroup")]
[Serializable]
public class ProjectGroupComponent : Component
{
    /// <inheritdoc/>
    public ProjectGroupComponent(PropertyStore store) : base(store) { }

    /// <summary>
    /// The path to the project group file.
    /// </summary>
    [XmlText]
    public string Path
    {
        get => Get<string>(nameof(Path));
        set => Set(nameof(Path), value);
    }
}