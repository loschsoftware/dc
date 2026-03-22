using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a reference to an external assembly.
/// </summary>
[Serializable]
[XmlRoot("AssemblyReference")]
public partial class AssemblyReference : Reference
{
    /// <inheritdoc/>
    public AssemblyReference(PropertyStore store) : base(store) { }

    /// <summary>
    /// The path to the referenced assembly.
    /// </summary>
    [XmlText]
    [ConfigProperty]
    public partial string AssemblyPath { get; set; }

    /// <summary>
    /// Specifies wheter or not to copy the referenced assembly to the output directory of a build.
    /// </summary>
    [XmlAttribute]
    [ConfigProperty]
    public partial bool CopyToOutput { get; set; }

    /// <summary>
    /// Specifies wheter or not to implicitly import all namespaces of the referenced assembly.
    /// </summary>
    [XmlAttribute]
    [ConfigProperty]
    public partial bool ImportNamespacesImplicitly { get; set; }
}