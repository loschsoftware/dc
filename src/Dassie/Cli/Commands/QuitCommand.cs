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

    public string Description => "Stops all file watchers.";

    public CommandHelpDetails HelpDetails() => new()
    {
        Description = Description,
        Usage = ["dc quit"]
    };

    public int Invoke(string[] args)
    {
        if (args.Length > 0)
        {
            foreach (string arg in args)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0212_UnexpectedArgument,
                    $"Unexpected argument '{arg}'.",
                    CompilerExecutableName);
            }

            return -1;
        }

        Process[] processes = Process.GetProcesses();

        string pidFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "pid.txt");
        if (!File.Exists(pidFilePath))
        {
            LogOut.WriteLine("No file watchers running.");
            return 0;
        }

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
