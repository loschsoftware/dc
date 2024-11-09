namespace Dassie.CodeAnalysis.Structure;

/// <summary>
/// Represents the code structure of a Dassie project. Used by LSEdit for generating structure views.
/// </summary>
public class ProjectStructure
{
    /// <summary>
    /// The namespaces contained within the project.
    /// </summary>
    public Namespace[] Namespaces { get; set; }

    /// <summary>
    /// The types outside of a namespace that are part of the code structure.
    /// </summary>
    public Type[] Types { get; set; }
}