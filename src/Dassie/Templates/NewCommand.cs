using Dassie.Extensions;

namespace Dassie.Templates;

internal class NewCommand : ICompilerCommand
{
    public string Command => "new";

    public string UsageString => "new <Type> <Name>";

    public string Description => "Creates the file structure of a Dassie project.";

    public string Help => @"
new command
";

    public int Invoke(string[] args) => DSTemplates.CreateStructure(args);
}