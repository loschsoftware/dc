using Dassie.Extensions;
using Dassie.Templates;

namespace Dassie.Cli.Commands;

internal class NewCommand : ICompilerCommand
{
    private static NewCommand _instance;
    public static NewCommand Instance => _instance ??= new();

    public string Command => "new";

    public string Description => "Creates the file structure of a Dassie project.";

    public CommandHelpDetails HelpDetails() => new()
    {
        Description = Description,
        Usage =
        [
            "dc new <Template> <Name> [-f|--force]",
            "dc new --list-templates"
        ],
        Remarks = "This command creates a new directory with the specified project name and creates the file structure of the specified project template inside. Project templates are installed as part of compiler extensions (packages). Managing compiler extensions is facilitated through the 'dc package' command. The Dassie compiler includes the templates 'Console' and 'Library' by default.",
        Options =
        [
            ("Template", "The template to use for the new project."),
            ("Name", "The name of the project."),
            ("-f|--force", "If enabled, directories colliding with the path of the new project will be deleted."),
            ("--list-templates", "Lists all installed project templates.")
        ],
        Examples =
        [
            ("dc new console MyApp", "Creates a new console application project named 'MyApp' in the current directory."),
            ("dc new custom-template MyProject -f", "Creates a new project based on the template 'custom-template' named 'MyProject' in the current directory. If a directory named 'MyProject' already exists, it will be deleted first."),
            ("dc new --list-templates", "Lists all installed project templates along with the package they are installed from.")
        ]
    };

    public int Invoke(string[] args) => TemplateBuilder.CreateStructure(args);
}