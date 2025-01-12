using Dassie.Configuration;

namespace Dassie.Extensions;

/// <summary>
/// Represents a project file (dsconfig.xml) as part of a project template.
/// </summary>
public class ProjectFile : ProjectTemplateEntry
{
    /// <summary>
    /// Sets the configuration data to be serialized.
    /// </summary>
    public DassieConfig Content { get; set; }
}