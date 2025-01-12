using Dassie.Extensions;
using Dassie.Templates;

namespace Dassie.Cli.Commands;

internal class NewCommand : ICompilerCommand
{
    private static NewCommand _instance;
    public static NewCommand Instance => _instance ??= new();

    public string Command => "new";

    public string UsageString => "new <Type> <Name>";

    public string Description => "Creates the file structure of a Dassie project.";

    public int Invoke(string[] args) => TemplateBuilder.CreateStructure(args);
}