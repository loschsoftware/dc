namespace Dassie.Extensions;

/// <summary>
/// Represents a project template to be used with the 'dc new' command.
/// </summary>
public interface IProjectTemplate
{
    /// <summary>
    /// The name of the project template.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// An array of file system entries that are part of the project.
    /// </summary>
    public ProjectTemplateEntry[] Entries { get; set; }
}