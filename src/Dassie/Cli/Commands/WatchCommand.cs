﻿using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Dassie.Cli.Commands;

internal class WatchCommand : ICompilerCommand
{
    internal static Process watchProcess = null;

    public string Command => "watch";

    public string UsageString => "watch, auto";

    public string Description => "Watches all .ds files in the current folder structure and automatically recompiles when files are changed.";
    
    public WatchCommand()
    {
        _help = CommandHelpStringBuilder.GenerateHelpString(this);
    }

    private readonly string _help;
    public string Help() => _help;

    public List<string> Aliases() => ["auto"];

    public int Invoke(string[] args)
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