namespace Dassie.CodeAnalysis.Structure;

/// <summary>
/// Represents a namespace inside of a Dassie code structure.
/// </summary>
public class Namespace
{
    /// <summary>
    /// The name of the namespace.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The child namespaces of the namespace.
    /// </summary>
    public Namespace[] Namespaces { get; set; }

    /// <summary>
    /// The types contained within the namespace.
    /// </summary>
    public Type[] Types { get; set; }
}