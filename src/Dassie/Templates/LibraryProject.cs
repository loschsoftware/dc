using Dassie.Extensions;

namespace Dassie.Templates;

/// <summary>
/// Provides an integrated project template for a library.
/// </summary>
internal class LibraryProject : IProjectTemplate
{
    private static LibraryProject _instance;
    public static LibraryProject Instance => _instance ??= new();

    private LibraryProject()
    {
        ProjectTemplateDirectory srcDir = new()
        {
            Name = "src",
            Children = [new ProjectTemplateFile() {
                Name = "type1.ds",
                FormattedContent = """
                type Type1 = {
                }
                """
            }]
        };

        Entries = [
            srcDir,
            new ProjectFile() {
                Content = new() {
                    ApplicationType = "Library",
                    BuildOutputDirectory = "./build",
                    RootNamespace = "$(ProjectName)",
                    AssemblyName = "$(ProjectName)"
                }
            }
        ];
    }

    public string Name => "Library";
    public bool IsCaseSensitive() => false;

    public ProjectTemplateEntry[] Entries { get; private set; }
}
