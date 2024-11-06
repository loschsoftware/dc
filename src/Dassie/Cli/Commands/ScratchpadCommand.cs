using Dassie.Extensions;
using System.Collections.Generic;

namespace Dassie.Cli.Commands;

internal class ScratchpadCommand : ICompilerCommand
{
    private static ScratchpadCommand _instance;
    public static ScratchpadCommand Instance => _instance ??= new();

    public string Command => "scratchpad";

    public string UsageString => "scratchpad, sp [Command] [Options]";

    public string Description => "Allows compiling and running Dassie source code from the console. Use 'dc scratchpad help' to display available commands.";

    public List<string> Aliases() => ["sp"];

    public int Invoke(string[] args) => Scratchpad.HandleScratchpadCommands(args);
}
