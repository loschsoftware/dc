using Dassie.Extensions;

namespace Dassie.Cli.Commands;

internal class WatchIndefinetlyCommand : ICompilerCommand
{
    public string Command => "watch-indefinetly";

    public string UsageString => "";

    public string Description => "";

    public bool Hidden() => true;

    public int Invoke(string[] args) => WatchCommand.WatchIndefinetly();
}