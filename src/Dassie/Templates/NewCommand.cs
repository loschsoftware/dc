using Dassie.Cli.Commands;
using Dassie.Extensions;

namespace Dassie.Templates;

internal class NewCommand : ICompilerCommand
{
    public string Command => "new";

    public string UsageString => "new <Type> <Name>";

    public string Description => "Creates the file structure of a Dassie project.";
    
    public NewCommand()
    {
        _help = CommandHelpStringBuilder.GenerateHelpString(this);
    }

    private readonly string _help;
    public string Help() => _help;

    public int Invoke(string[] args) => DSTemplates.CreateStructure(args);
}