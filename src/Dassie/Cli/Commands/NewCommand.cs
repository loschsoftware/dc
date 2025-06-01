using Dassie.Extensions;
using Dassie.Templates;

namespace Dassie.Cli.Commands;

internal class NewCommand : ICompilerCommand
{
    private static NewCommand _instance;
    public static NewCommand Instance => _instance ??= new();

    public string Command => "new";

    public string UsageString => "new <Template> <Name> [-f|--force]";

    public string Description => "Creates the file structure of a Dassie project.";

    public CommandHelpDetails HelpDetails() => new()
    {
        Description = Description,
        Usage = ["dc new <Template> <Name> [-f|--force]"],
        Remarks = "This command creates a new directory with the specified project name and creates the file structure of the specified project template inside. Project templates are installed as part of compiler extensions (packages). Managing compiler extensions is facilitated through the 'dc package' command.",
        Options =
        [
            ("Template", "The template to use for the new project."),
            ("Name", "The name of the project."),
            ("-f|--force", "If enabled, directories colliding with the path of the new project will be deleted.")
        ]
    };

    public int Invoke(string[] args) => TemplateBuilder.CreateStructure(args);
}