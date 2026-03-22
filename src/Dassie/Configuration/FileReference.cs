using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a file reference.
/// </summary>
[Serializable]
[XmlRoot("FileReference")]
public partial class FileReference : Reference
{
    /// <inheritdoc/>
    public FileReference(PropertyStore store) : base(store) { }

    /// <summary>
    /// The path to the referenced file.
    /// </summary>
    [XmlText]
    [ConfigProperty]
    public partial string FileName { get; set; }

    /// <summary>
    /// Specifies wheter or not to copy the referenced file to the build output directory.
    /// </summary>
    [XmlAttribute]
    [ConfigProperty]
    public partial bool CopyToOutput { get; set; }
}