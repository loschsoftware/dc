using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents an imported configuration file.
/// </summary>
[Serializable]
[XmlRoot]
public class Import : ConfigObject
{
    /// <inheritdoc/>
    public Import(PropertyStore store) : base(store) { }

    /// <summary>
    /// The path to the imported configuration file.
    /// </summary>
    [XmlAttribute]
    public string Path
    {
        get => Get<string>(nameof(Path));
        set => Set(nameof(Path), value);
    }
}