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
        ProjectTemplateDirectory srcDir = new()
        {
            Name = "src",
            Children = [new ProjectTemplateFile() {
                Name = "main.ds",
                FormattedContent = """
                println "Hello World!"
                """
            }]
        };

        Entries = [
            srcDir,
            new ProjectFile() {
                Content = new() {
                    BuildOutputDirectory = "./build",
                    RootNamespace = "$(ProjectName)",
                    AssemblyName = "$(ProjectName)"
                }
            }
        ];
    }

    public string Name => "Console";
    public bool IsCaseSensitive() => false;

    public ProjectTemplateEntry[] Entries { get; private set; }
}