using Dassie.Extensions;

namespace Dassie.Cli.Commands;

internal class WatchIndefinetlyCommand : ICompilerCommand
{
    private static WatchIndefinetlyCommand _instance;
    public static WatchIndefinetlyCommand Instance => _instance ??= new();

    public string Command => "watch-indefinetly";

    public string UsageString => "";

    public string Description => "";

    public bool Hidden() => true;

    public int Invoke(string[] args) => WatchCommand.WatchIndefinetly(args);
}