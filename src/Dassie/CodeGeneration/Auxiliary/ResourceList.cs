namespace Dassie.CodeGeneration.Auxiliary;

/// <summary>
/// Represents the top-level resource directory in the .rsrc section of a PE file.
/// </summary>
internal class ResourceList
{
    /// <summary>
    /// An array of secondary directories of shared resource types.
    /// </summary>
    public ResourceTypeGroup[] Types { get; set; }
}

/// <summary>
/// Represents a resource directory with a specific resource type.
/// </summary>
internal class ResourceTypeGroup
{
    /// <summary>
    /// The resource type identifier.
    /// </summary>
    public uint Id;

    /// <summary>
    /// An array of resource directories containing language variants for resources.
    /// </summary>
    public ResourceLanguageGroup[] Resources { get; set; }
}

/// <summary>
/// Represents a resource directory containing language variants for a resource.
/// </summary>
internal class ResourceLanguageGroup
{
    /// <summary>
    /// An array of resource entries with specific languages.
    /// </summary>
    public ResourceEntry[] LanguageVariants { get; set; }
}

/// <summary>
/// Represents a single resource.
/// </summary>
internal class ResourceEntry
{
    /// <summary>
    /// The language identifier of the resource. For example, 1033 for English (US).
    /// </summary>
    public uint Language { get; set; }

    /// <summary>
    /// The binary data of the resource.
    /// </summary>
    public byte[] Data { get; set; }
}