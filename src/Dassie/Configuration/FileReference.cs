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
    /// <inheritdoc/>
    public FileReference(PropertyStore store) : base(store) { }

    /// <summary>
    /// The path to the referenced file.
    /// </summary>
    [XmlText]
    public string FileName
    {
        get => Get<string>(nameof(FileName));
        set => Set(nameof(FileName), value);
    }

    /// <summary>
    /// Specifies wheter or not to copy the referenced file to the build output directory.
    /// </summary>
    [XmlAttribute]
    public bool CopyToOutput
    {
        get => Get<bool>(nameof(CopyToOutput));
        set => Set(nameof(CopyToOutput), value);
    }
}