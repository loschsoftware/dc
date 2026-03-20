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
    /// <inheritdoc/>
    public AssemblyReference(PropertyStore store) : base(store) { }

    /// <summary>
    /// The path to the referenced assembly.
    /// </summary>
    [XmlText]
    public string AssemblyPath
    {
        get => Get<string>(nameof(AssemblyPath));
        set => Set(nameof(AssemblyPath), value);
    }

    /// <summary>
    /// Specifies wheter or not to copy the referenced assembly to the output directory of a build.
    /// </summary>
    [XmlAttribute]
    public bool CopyToOutput
    {
        get => Get<bool>(nameof(CopyToOutput));
        set => Set(nameof(CopyToOutput), value);
    }

    /// <summary>
    /// Specifies wheter or not to implicitly import all namespaces of the referenced assembly.
    /// </summary>
    [XmlAttribute]
    public bool ImportNamespacesImplicitly
    {
        get => Get<bool>(nameof(ImportNamespacesImplicitly));
        set => Set(nameof(ImportNamespacesImplicitly), value);
    }
}