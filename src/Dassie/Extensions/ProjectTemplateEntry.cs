using Dassie.Configuration;

namespace Dassie.Extensions;

/// <summary>
/// Represents a file system entry inside of a project template.
/// </summary>
public abstract record ProjectTemplateEntry { }

/// <summary>
/// Represents a source file that is part of a project template.
/// </summary>
/// <param name="Name">The name of the file.</param>
/// <param name="FormattedContent">The content of the file. Allows using Dassie project file macros.</param>
public record ProjectTemplateSourceFile(string Name, string FormattedContent) : ProjectTemplateEntry;

/// <summary>
/// Represents a raw file that is part of a project template.
/// </summary>
/// <param name="Name">The name of the file.</param>
/// <param name="Contents">The contents of the file.</param>
public record ProjectTemplateAuxiliaryFile(string Name, byte[] Contents) : ProjectTemplateEntry;

/// <summary>
/// Represents a directory that is part of a project template.
/// </summary>
/// <param name="Name">The name of the directory.</param>
/// <param name="Children">The child elements of the directory.</param>
public record ProjectTemplateDirectory(string Name, ProjectTemplateEntry[] Children) : ProjectTemplateEntry;

/// <summary>
/// Represents a project file (dsconfig.xml) that is part of a project template.
/// </summary>
/// <param name="Content">Specifies the configuration data to be serialized.</param>
public record ProjectFile(DassieConfig Content) : ProjectTemplateEntry;