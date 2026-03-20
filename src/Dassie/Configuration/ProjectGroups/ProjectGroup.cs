using System;
using System.Xml;
using System.Xml.Serialization;

namespace Dassie.Configuration.ProjectGroups;

/// <summary>
/// Represents a project group.
/// </summary>
[Serializable]
[XmlRoot]
public class ProjectGroup : ConfigObject
{
    /// <inheritdoc/>
    public ProjectGroup(PropertyStore store) : base(store) { }

    /// <summary>
    /// Represents a list of targets.
    /// </summary>
    [Serializable]
    [XmlRoot]
    public class TargetList : ConfigObject
    {
        /// <inheritdoc/>
        public TargetList(PropertyStore store) : base(store) { }

        /// <summary>
        /// An array of XML nodes representing the deployment targets of a project group.
        /// </summary>
        [XmlAnyElement]
        public XmlNode[] Targets
        {
            get => Get<XmlNode[]>(nameof(Targets));
            set => Set(nameof(Targets), value);
        }
    }

    /// <summary>
    /// An array containing the components of the project group.
    /// </summary>
    [XmlArray]
    [XmlArrayItem(typeof(Project))]
    [XmlArrayItem(typeof(ProjectGroupComponent))]
    public Component[] Components
    {
        get => Get<Component[]>(nameof(Components));
        set => Set(nameof(Components), value);
    }

    /// <summary>
    /// Specifies the executable component of the project group.
    /// </summary>
    [XmlElement("Executable")]
    public string ExecutableComponent
    {
        get => Get<string>(nameof(ExecutableComponent));
        set => Set(nameof(ExecutableComponent), value);
    }

    /// <summary>
    /// A list of deployment targets.
    /// </summary>
    [XmlElement]
    public TargetList Targets
    {
        get => Get<TargetList>(nameof(Targets));
        set => Set(nameof(Targets), value);
    }
}