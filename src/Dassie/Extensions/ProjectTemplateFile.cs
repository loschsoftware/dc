namespace Dassie.Extensions;

/// <summary>
/// Represents a file that is part of a project template.
/// </summary>
public class ProjectTemplateFile : ProjectTemplateEntry
{
    /// <summary>
    /// The name of the file.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The content of the file. Allows using Dassie project file macros.
    /// </summary>
    public string FormattedContent { get; set; }
}