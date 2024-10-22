using Dassie.Extensions;

namespace Dassie.Cli.Commands;

internal class ScratchpadCommand : ICompilerCommand
{
    public string Command => "scratchpad";

    public string UsageString => "scratchpad [Command] [Options]";

    public string Description => "Allows compiling and running Dassie source code from the console. Use 'dc scratchpad help' to display available commands.";

    public string Help => @"
scratchpad command";

    public int Invoke(string[] args) => Scratchpad.HandleScratchpadCommands(args);
}
