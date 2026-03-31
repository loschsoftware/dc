using Dassie.Extensions;

namespace Dassie.Templates;

/// <summary>
/// Provides an integrated project template for a console application.
/// </summary>
internal class ConsoleProject : IProjectTemplate
{
    private static ConsoleProject _instance;
    public static ConsoleProject Instance => _instance ??= new();

    private ConsoleProject()
    {
        ProjectTemplateDirectory srcDir = new("src",
            Children: [new ProjectTemplateSourceFile("main.ds", """
                println "Hello World!"
                """)]);

        Entries = [
            srcDir,
            new ProjectFile(new() {
                BuildDirectory = "./build",
                RootNamespace = "$(ProjectName)",
                AssemblyFileName = "$(ProjectName)"
            })
        ];
    }

    public string Name => "Console";
    public bool IsCaseSensitive() => false;

    public ProjectTemplateEntry[] Entries { get; private set; }
}