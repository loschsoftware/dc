using Dassie.Extensions;
using System.Diagnostics;

namespace Dassie.Cli.Commands;

internal class QuitCommand : ICompilerCommand
{
    public string Command => "quit";

    public string UsageString => "quit";

    public string Description => "Stops all file watchers.";

    public QuitCommand()
    {
        _help = CommandHelpStringBuilder.GenerateHelpString(this);
    }

    private readonly string _help;
    public string Help() => _help;

    public int Invoke(string[] args)
    {
        LogOut.WriteLine("No longer watching file changes.");

        WatchCommand.watchProcess = new Process();
        WatchCommand.watchProcess.StartInfo.FileName = "taskkill.exe";
        WatchCommand.watchProcess.StartInfo.Arguments = "/f /im dc.exe";
        WatchCommand.watchProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        WatchCommand.watchProcess.StartInfo.CreateNoWindow = true;
        WatchCommand.watchProcess.Start();
        return 0;
    }
}
