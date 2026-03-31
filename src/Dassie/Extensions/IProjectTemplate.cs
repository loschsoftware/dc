using Dassie.Configuration;
using System.Collections.Generic;

namespace Dassie.Extensions;

internal class AdHocTemplate(string name, ProjectTemplateEntry[] entries) : IProjectTemplate
{
    public string Name => name;
    public ProjectTemplateEntry[] Entries => entries;
}

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

    /// <summary>
    /// Generates a project template from a directory.
    /// </summary>
    /// <param name="name">The name of the project template.</param>
    /// <param name="path">The directory containing the project template elements.</param>
    /// <returns>A project template based on the specific directory structure.</returns>
    public static IProjectTemplate FromDirectory(string name, string path)
    {
        static ProjectTemplateEntry GetEntryForFileSystemElement(string path)
        {
            string fileName = Path.GetFileName(path);

            if (File.Exists(path))
            {
                if (fileName == ProjectConfigurationFileName)
                    return new ProjectFile(ProjectFileSerializer.Deserialize(path));

                if (Path.GetExtension(path) == ".ds")
                    return new ProjectTemplateSourceFile(fileName, File.ReadAllText(path));

                return new ProjectTemplateAuxiliaryFile(fileName, File.ReadAllBytes(path));
            }

            List<ProjectTemplateEntry> entries = [];
            foreach (string fse in Directory.GetFileSystemEntries(path))
                entries.Add(GetEntryForFileSystemElement(fse));

            return new ProjectTemplateDirectory(fileName, entries.ToArray());
        }

        List<ProjectTemplateEntry> entries = [];

        foreach (string fse in Directory.GetFileSystemEntries(path))
            entries.Add(GetEntryForFileSystemElement(fse));

        return new AdHocTemplate(name, entries.ToArray());
    }
}