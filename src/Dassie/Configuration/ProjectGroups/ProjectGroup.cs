using System;
using System.Xml;
using System.Xml.Serialization;

namespace Dassie.Configuration.ProjectGroups;

/// <summary>
/// Represents a project group.
/// </summary>
[Serializable]
[XmlRoot]
public class ProjectGroup
{
    /// <summary>
    /// Represents a list of targets.
    /// </summary>
    [Serializable]
    [XmlRoot]
    public class TargetList
    {
        /// <summary>
        /// An array of XML nodes representing the deployment targets of a project group.
        /// </summary>
        [XmlAnyElement]
        public XmlNode[] Targets { get; set; }
    }

    /// <summary>
    /// An array containing the components of the project group.
    /// </summary>
    [XmlArray]
    [XmlArrayItem(typeof(Project))]
    [XmlArrayItem(typeof(ProjectGroupComponent))]
    public Component[] Components { get; set; }

    /// <summary>
    /// Specifies the executable component of the project group.
    /// </summary>
    [XmlElement("Executable")]
    public string ExecutableComponent { get; set; }

    /// <summary>
    /// A list of deployment targets.
    /// </summary>
    [XmlElement]
    public TargetList Targets { get; set; }
}