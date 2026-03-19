using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a file reference.
/// </summary>
[Serializable]
[XmlRoot("FileReference")]
public class FileReference : Reference
{
    /// <summary>
    /// The path to the referenced file.
    /// </summary>
    [XmlText]
    public string FileName { get; set; }

    /// <summary>
    /// Specifies wheter or not to copy the referenced file to the build output directory.
    /// </summary>
    [XmlAttribute("CopyToOutput")]
    public bool CopyToOutput { get; set; }
}