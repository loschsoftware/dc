using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a project reference.
/// </summary>
[Serializable]
[XmlRoot]
public class ProjectReference : Reference
{
    /// <summary>
    /// Specifies wheter or not to copy the output files of the project to the build output directory.
    /// </summary>
    [DefaultValue(true)]
    [XmlAttribute]
    public bool CopyToOutput { get; set; } = true;

    /// <summary>
    /// The path to the project file to reference.
    /// </summary>
    [XmlText]
    public string ProjectFile { get; set; }
}