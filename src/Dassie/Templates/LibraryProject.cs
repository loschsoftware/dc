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
        ProjectTemplateDirectory srcDir = new("src",
            Children: [new ProjectTemplateSourceFile("Type1.ds", """
                export $(ProjectName)

                type Type1 = {
                    
                }
                """)]);

        Entries = [
            srcDir,
            new ProjectFile(new() {
                ApplicationType = "Library",
                BuildDirectory = "./build",
                RootNamespace = "$(ProjectName)",
                AssemblyFileName = "$(ProjectName)"
            })
        ];
    }

    public string Name => "Library";
    public bool IsCaseSensitive() => false;

    public ProjectTemplateEntry[] Entries { get; private set; }
}
