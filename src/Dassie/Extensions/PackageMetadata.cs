using System;

namespace Dassie.Extensions;

/// <summary>
/// Defines version information for a Dassie compiler extension.
/// </summary>
public class PackageMetadata
{
    /// <summary>
    /// The name of the extension.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A description of the extension.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The current version of the extension.
    /// </summary>
    public Version Version { get; set; }

    /// <summary>
    /// The author of the extension.
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// The unique ID of the extension, calculated from author, name and version number.
    /// </summary>
    public string Id => $"{Author}.{Name}_v{Version}";
}