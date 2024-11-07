namespace Dassie.Extensions;

/// <summary>
/// Represents a directory that is part of a project template.
/// </summary>
public class ProjectTemplateDirectory : ProjectTemplateEntry
{
    /// <summary>
    /// The name of the directory.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The child elements of the directory.
    /// </summary>
    public ProjectTemplateEntry[] Children { get; set; }
}