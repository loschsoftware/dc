using Dassie.Extensions;
using System;
using System.Diagnostics;
using System.Linq;

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
        string pidFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "pid.txt");
        File.ReadAllLines(pidFilePath).Select(int.Parse).ToList().ForEach(i => Process.GetProcessById(i).Kill());
        File.Delete(pidFilePath);

        LogOut.WriteLine("No longer watching file changes.");
        return 0;
    }
}
