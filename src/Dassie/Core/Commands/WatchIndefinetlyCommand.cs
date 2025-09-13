using Dassie.Extensions;

namespace Dassie.Core.Commands;

internal class WatchIndefinetlyCommand : CompilerCommand
{
    private static WatchIndefinetlyCommand _instance;
    public static WatchIndefinetlyCommand Instance => _instance ??= new();

    public override string Command => "watch-indefinetly";
    public override string Description => "";
    public override CommandOptions Options => CommandOptions.Hidden | CommandOptions.NoHelpRouting;

    public override int Invoke(string[] args) => WatchCommand.WatchIndefinetly(args);
}