using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a referenced resource.
/// </summary>
[XmlRoot]
[Serializable]
public abstract class Resource : ConfigObject
{
    /// <inheritdoc/>
    protected Resource(PropertyStore store) : base(store) { }

    /// <summary>
    /// The path to the resource file.
    /// </summary>
    [XmlText]
    public string Path { get; set; }
}

/// <summary>
/// Represents an unmanaged resource.
/// </summary>
[XmlRoot]
[Serializable]
public class UnmanagedResource : Resource
{
    /// <inheritdoc/>
    public UnmanagedResource(PropertyStore store) : base(store) { }
}

/// <summary>
/// Represents the type of a resource.
/// </summary>
public enum ResourceType
{
    /// <summary>
    /// An unmanaged resource.
    /// </summary>
    Unmanaged,
    /// <summary>
    /// A managed resource.
    /// </summary>
    Managed
}

/// <summary>
/// Represents a managed resource.
/// </summary>
[XmlRoot]
[Serializable]
public class ManagedResource : Resource
{
    /// <inheritdoc/>
    public ManagedResource(PropertyStore store) : base(store) { }

    /// <summary>
    /// Specifies the name which can be used to access the resource
    /// programmatically.
    /// </summary>
    [XmlAttribute]
    public string Name { get; set; }
}