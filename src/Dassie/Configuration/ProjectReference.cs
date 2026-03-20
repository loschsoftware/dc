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
    /// <inheritdoc/>
    public ProjectReference(PropertyStore store) : base(store) { }

    /// <summary>
    /// Specifies wheter or not to copy the output files of the project to the build output directory.
    /// </summary>
    [DefaultValue(true)]
    [XmlAttribute]
    public bool CopyToOutput
    {
        get => Get<bool>(nameof(CopyToOutput));
        set => Set(nameof(CopyToOutput), value);
    }

    /// <summary>
    /// The path to the project file to reference.
    /// </summary>
    [XmlText]
    public string ProjectFile
    {
        get => Get<string>(nameof(ProjectFile));
        set => Set(nameof(ProjectFile), value);
    }
}