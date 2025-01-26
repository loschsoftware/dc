using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Dassie.Cli.Commands;

internal class WatchCommand : ICompilerCommand
{
    private static WatchCommand _instance;
    public static WatchCommand Instance => _instance ??= new();

    internal static Process watchProcess = null;

    public string Command => "watch";

    public string UsageString => "watch, auto";

    public string Description => "Watches all .ds files in the current folder structure and automatically recompiles when files are changed.";

    public List<string> Aliases() => ["auto"];

    public int Invoke(string[] args)
    {
        LogOut.Write("Watching file changes. Use ");

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("dc quit");
        Console.ForegroundColor = ConsoleColor.Gray;

        LogOut.WriteLine(" to stop watching changes.");

        watchProcess = new Process();
#if STANDALONE
        watchProcess.StartInfo.FileName = $"{Environment.GetCommandLineArgs()[0]}";
        watchProcess.StartInfo.Arguments = "watch-indefinetly";
#else
        watchProcess.StartInfo.FileName = "dotnet";
        watchProcess.StartInfo.Arguments = $"{Assembly.GetCallingAssembly().Location} watch-indefinetly";
#endif
        watchProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        watchProcess.StartInfo.CreateNoWindow = true;
        watchProcess.Start();

        using StreamWriter sw = File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "pid.txt"));
        sw.WriteLine(watchProcess.Id);
        return 0;
    }

    public static int WatchIndefinetly()
    {
        watchProcess = new Process();
#if STANDALONE
        watchProcess.StartInfo.FileName = $"{Environment.GetCommandLineArgs()[0]}";
        watchProcess.StartInfo.Arguments = "build";
#else
        watchProcess.StartInfo.FileName = "dotnet";
        watchProcess.StartInfo.Arguments = $"{Assembly.GetCallingAssembly().Location} watch-indefinetly";
#endif
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

#if STANDALONE
            string cmd = $"{Environment.GetCommandLineArgs()[0]} build";
#else
            string cmd = $"{Assembly.GetCallingAssembly().Location} build";
#endif

            watcher.Changed += Compile;
            watcher.Created += Compile;
            watcher.Deleted += Compile;

            void Compile(object sender, FileSystemEventArgs e)
            {
                var buildProcess = new Process();
#if STANDALONE
                buildProcess.StartInfo.FileName = cmd.Split(' ')[0];
                buildProcess.StartInfo.Arguments = cmd.Split(' ')[1];
#else
                buildProcess.StartInfo.FileName = "dotnet";
                buildProcess.StartInfo.Arguments = cmd;
#endif
                buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                buildProcess.Start();
                buildProcess.WaitForExit();
            }

            watcher.WaitForChanged(WatcherChangeTypes.All);
        }
    }
}