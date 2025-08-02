namespace Dassie.Extensions.Web;

/// <summary>
/// Represents the JSON structure of an entry in an extension source.
/// </summary>
public class ExtensionMetadata
{
    /// <summary>
    /// The package metadata of the extension.
    /// </summary>
    public PackageMetadata Metadata { get; set; }

    /// <summary>
    /// The URI of the actual extension assembly.
    /// </summary>
    public string Uri { get; set; }
}