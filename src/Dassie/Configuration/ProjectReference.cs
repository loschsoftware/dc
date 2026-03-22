using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a project reference.
/// </summary>
[Serializable]
[XmlRoot]
public partial class ProjectReference : Reference
{
    /// <inheritdoc/>
    public ProjectReference(PropertyStore store) : base(store) { }

    /// <summary>
    /// Specifies wheter or not to copy the output files of the project to the build output directory.
    /// </summary>
    [DefaultValue(true)]
    [XmlAttribute]
    [ConfigProperty]
    public partial bool CopyToOutput { get; set; }

    /// <summary>
    /// The path to the project file to reference.
    /// </summary>
    [XmlText]
    [ConfigProperty]
    public partial string ProjectFile { get; set; }
}