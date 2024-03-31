namespace Dassie.CodeAnalysis.Structure;

/// <summary>
/// Represents a type inside a Dassie code structure.
/// </summary>
public class Type
{
    /// <summary>
    /// The name of the type, without the namespace.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The files containing the type definition.
    /// </summary>
    public string[] Files { get; set; }
}