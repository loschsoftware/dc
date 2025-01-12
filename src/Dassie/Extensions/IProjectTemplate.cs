namespace Dassie.Extensions;

/// <summary>
/// Represents a project template to be used with the 'dc new' command.
/// </summary>
public interface IProjectTemplate
{
    /// <summary>
    /// The name of the project template.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// An array of file system entries that are part of the project.
    /// </summary>
    public ProjectTemplateEntry[] Entries { get; }

    /// <summary>
    /// Allows setting the case sensitivity of the project template name.
    /// </summary>
    /// <returns>By default, returns <see langword="true"/>.</returns>
    public virtual bool IsCaseSensitive() => true;
}