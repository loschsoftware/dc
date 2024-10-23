using Dassie.Extensions;

namespace Dassie.Cli.Commands;

internal class ScratchpadCommand : ICompilerCommand
{
    public string Command => "scratchpad";

    public string UsageString => "scratchpad [Command] [Options]";

    public string Description => "Allows compiling and running Dassie source code from the console. Use 'dc scratchpad help' to display available commands.";
    
    public ScratchpadCommand()
    {
        _help = CommandHelpStringBuilder.GenerateHelpString(this);
    }

    private readonly string _help;
    public string Help() => _help;

    public int Invoke(string[] args)
    {
        if (args.Length > 0 && args[0] == "scratchpad")
            args = args[1..];

        return Scratchpad.HandleScratchpadCommands(args);
    }
}
