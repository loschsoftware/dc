using Dassie.Extensions;
using System;
using System.Diagnostics;
using System.Linq;

namespace Dassie.Cli.Commands;

internal class QuitCommand : ICompilerCommand
{
    private static QuitCommand _instance;
    public static QuitCommand Instance => _instance ??= new();

    public string Command => "quit";

    public string UsageString => "quit";

    public string Description => "Stops all file watchers.";

    public int Invoke(string[] args)
    {
        Process[] processes = Process.GetProcesses();

        string pidFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "pid.txt");
        File.ReadAllLines(pidFilePath).Select(int.Parse).ToList().ForEach(i =>
        {
            if (processes.Any(p => p.Id == i))
                Process.GetProcessById(i).Kill();
        });

        File.Delete(pidFilePath);

        LogOut.WriteLine("No longer watching file changes.");
        return 0;
    }
}
