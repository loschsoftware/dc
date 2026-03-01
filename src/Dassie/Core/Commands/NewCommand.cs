using Dassie.Extensions;
using Dassie.Templates;

namespace Dassie.Core.Commands;

internal class NewCommand : CompilerCommand
{
    private static NewCommand _instance;
    public static NewCommand Instance => _instance ??= new();

    public override string Command => "new";

    public override string Description => StringHelper.NewCommand_Description;

    public override CommandHelpDetails HelpDetails => new()
    {
        Description = Description,
        Usage =
        [
            "dc new <Template> <Name> [-f|--force]",
            "dc new --list-templates"
        ],
        Remarks = StringHelper.NewCommand_Remarks,
        Options =
        [
            ("Template", StringHelper.NewCommand_TemplateOption),
            ("Name", StringHelper.NewCommand_NameOption),
            ("-f|--force", StringHelper.NewCommand_ForceOption),
            ("--list-templates", StringHelper.NewCommand_ListTemplatesOption)
        ],
        Examples =
        [
            ("dc new console MyApp", StringHelper.NewCommand_Example1),
            ("dc new custom-template MyProject -f", StringHelper.NewCommand_Example2),
            ("dc new --list-templates", StringHelper.NewCommand_Example3)
        ]
    };

    public override int Invoke(string[] args) => TemplateBuilder.CreateStructure(args);
}