using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents an imported configuration file.
/// </summary>
[Serializable]
[XmlRoot]
public class Import
{
    /// <summary>
    /// The path to the imported configuration file.
    /// </summary>
    [XmlAttribute]
    public string Path { get; set; }
}