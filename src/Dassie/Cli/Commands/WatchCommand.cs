using System;
using System.Diagnostics;
using System.IO;

namespace Dassie.Cli.Commands;

internal static partial class CliCommands
{
    private static Process watchProcess = null;

    public static int WatchForFileChanges(string[] args)
    {
        LogOut.Write("Watching file changes. Use ");

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("dc quit");
        Console.ForegroundColor = ConsoleColor.Gray;

        LogOut.WriteLine(" to stop watching changes.");

        watchProcess = new Process();
        watchProcess.StartInfo.FileName = "dc.exe";
        watchProcess.StartInfo.Arguments = "-watch-indefinetly";
        watchProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        watchProcess.StartInfo.CreateNoWindow = true;
        watchProcess.Start();

        return 0;
    }

    public static int QuitWatching()
    {
        LogOut.WriteLine("No longer watching file changes.");

        watchProcess = new Process();
        watchProcess.StartInfo.FileName = "taskkill.exe";
        watchProcess.StartInfo.Arguments = "/f /im dc.exe";
        watchProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        watchProcess.StartInfo.CreateNoWindow = true;
        watchProcess.Start();
        return 0;
    }
    
    public static int WatchIndefinetly()
    {
        watchProcess = new Process();
        watchProcess.StartInfo.FileName = "dc.exe";
        watchProcess.StartInfo.Arguments = $"build";
        watchProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        watchProcess.StartInfo.CreateNoWindow = true;
        watchProcess.Start();
        watchProcess.WaitForExit();

        while (true)
        {
            FileSystemWatcher watcher = new(Directory.GetCurrentDirectory(), "*.ds")
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };

            string cmd = "build";

            watcher.Changed += Compile;
            watcher.Created += Compile;
            watcher.Deleted += Compile;

            void Compile(object sender, FileSystemEventArgs e)
            {
                var buildProcess = new Process();
                buildProcess.StartInfo.FileName = "dc.exe";
                buildProcess.StartInfo.Arguments = cmd;
                buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                buildProcess.Start();
                buildProcess.WaitForExit();
            }

            watcher.WaitForChanged(WatcherChangeTypes.All);
        }
    }
}