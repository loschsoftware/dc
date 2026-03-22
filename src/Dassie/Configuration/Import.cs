using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents an imported configuration file.
/// </summary>
[Serializable]
[XmlRoot]
public partial class Import : ConfigObject
{
    /// <inheritdoc/>
    public Import(PropertyStore store) : base(store) { }

    /// <summary>
    /// The path to the imported configuration file.
    /// </summary>
    [XmlAttribute]
    [ConfigProperty]
    public partial string Path { get; set; }
}