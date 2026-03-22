using System;
using System.Xml;
using System.Xml.Serialization;

namespace Dassie.Configuration.ProjectGroups;

/// <summary>
/// Represents a project group.
/// </summary>
[Serializable]
[XmlRoot]
public partial class ProjectGroup : ConfigObject
{
    /// <inheritdoc/>
    public ProjectGroup(PropertyStore store) : base(store) { }

    /// <summary>
    /// Represents a list of targets.
    /// </summary>
    [Serializable]
    [XmlRoot]
    public partial class TargetList : ConfigObject
    {
        /// <inheritdoc/>
        public TargetList(PropertyStore store) : base(store) { }

        /// <summary>
        /// An array of XML nodes representing the deployment targets of a project group.
        /// </summary>
        [XmlAnyElement]
        [ConfigProperty]
        public partial XmlNode[] Targets { get; set; }
    }

    /// <summary>
    /// An array containing the components of the project group.
    /// </summary>
    [XmlArray]
    [XmlArrayItem(typeof(Project))]
    [XmlArrayItem(typeof(ProjectGroupComponent))]
    [ConfigProperty]
    public partial Component[] Components { get; set; }

    /// <summary>
    /// Specifies the executable component of the project group.
    /// </summary>
    [XmlElement("Executable")]
    [ConfigProperty]
    public partial string ExecutableComponent { get; set; }

    /// <summary>
    /// A list of deployment targets.
    /// </summary>
    [XmlElement]
    [ConfigProperty]
    public partial TargetList Targets { get; set; }
}