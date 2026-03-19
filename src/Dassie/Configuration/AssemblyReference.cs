using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a reference to an external assembly.
/// </summary>
[Serializable]
[XmlRoot("AssemblyReference")]
public class AssemblyReference : Reference
{
    /// <summary>
    /// The path to the referenced assembly.
    /// </summary>
    [XmlText]
    public string AssemblyPath { get; set; }

    /// <summary>
    /// Specifies wheter or not to copy the referenced assembly to the output directory of a build.
    /// </summary>
    [XmlAttribute("CopyToOutput")]
    public bool CopyToOutput { get; set; }

    /// <summary>
    /// Specifies wheter or not to implicitly import all namespaces of the referenced assembly.
    /// </summary>
    [XmlAttribute("ImportNamespacesImplicitly")]
    public bool ImportNamespacesImplicitly { get; set; }
}