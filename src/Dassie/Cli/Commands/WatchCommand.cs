﻿using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dassie.Cli.Commands;

internal class WatchCommand : ICompilerCommand
{
    private static WatchCommand _instance;
    public static WatchCommand Instance => _instance ??= new();

    internal static Process watchProcess = null;

    public string Command => "watch";

    public string UsageString => "watch, auto [Options]";

    public string Description => "Watches all .ds files in the current folder structure and automatically recompiles when files are changed.";

    public List<string> Aliases() => ["auto"];

    public int Invoke(string[] args)
    {
        bool error = false;
        string command = "";
        string profile = "";
        string dir = "";

        if (args != null && args.Length > 0)
        {
            if (args.Any(a => a == "-h" || a == "--help" || a == "-?"))
            {
                StringBuilder sb = new();

                sb.AppendLine();
                sb.AppendLine($"dc watch: {Description}");
                sb.AppendLine();
                sb.AppendLine("Usage: dc watch [(--command|-c) <Command>] [(--profile|-p) <Profile>] [Directory] [--help]");

                sb.AppendLine();
                sb.AppendLine("Available options:");
                sb.Append($"{"    --command, -c <Command>",-35}{HelpCommand.FormatLines("Specifies the compiler command that is executed when files are changed. Default is 'build'.", indentWidth: 35)}");
                sb.Append($"{"    --profile, -p <Profile>",-35}{HelpCommand.FormatLines("Specifies the build profile that is used when files are changed. If this option is set, the '--command' option cannot be used.", indentWidth: 35)}");
                sb.Append($"{"    <Directory>",-35}{HelpCommand.FormatLines("Specifies the directory that is watched for changed source files. Cannot be combined with the '--command' and '--profile' options.", indentWidth: 35)}");
                sb.Append($"{"    --help",-35}{HelpCommand.FormatLines("Shows this page.", indentWidth: 35)}");

                HelpCommand.DisplayLogo();
                Console.Write(sb.ToString());
                return 0;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith('-'))
                {
                    if (args.Length < i + 2)
                    {
                        EmitErrorMessage(
                            0, 0, 0,
                            DS0193_ExpectedCliOptionValue,
                            $"Expected value for option '{args[i]}'.",
                            "dc");

                        return -1;
                    }
                }

                if (args[i] == "-c" || args[i] == "--command")
                {
                    if (!string.IsNullOrEmpty(command))
                    {
                        error = true;
                        EmitErrorMessage(
                            0, 0, 0,
                            DS0194_DCWatchInvalidCombination,
                            $"'dc watch': Command to execute was already set to '{command}'",
                            "dc");
                    }

                    command = args[i++ + 1];
                    continue;
                }

                if (args[i] == "-p" || args[i] == "--profile")
                {
                    if (!string.IsNullOrEmpty(profile))
                    {
                        error = true;
                        EmitErrorMessage(
                            0, 0, 0,
                            DS0194_DCWatchInvalidCombination,
                            $"'dc watch': Build profile was already set to '{profile}'",
                            "dc");
                    }

                    profile = args[i++ + 1];
                    continue;
                }

                if (!string.IsNullOrEmpty(command) || !string.IsNullOrEmpty(profile))
                {
                    error = true;
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0194_DCWatchInvalidCombination,
                        "'dc watch': Compilation target can only be manually specified if neither '--profile' nor '--command' are set.",
                        "dc");
                }

                if (!Directory.Exists(args[i]))
                {
                    error = true;
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0194_DCWatchInvalidCombination,
                        $"'dc watch': Directory '{args[i]}' does not exist.",
                        "dc");
                }

                if (!string.IsNullOrEmpty(dir))
                {
                    error = true;
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0194_DCWatchInvalidCombination,
                        $"'dc watch': Compilation target directory was already set to '{args[i]}'.",
                        "dc");
                }

                dir = args[i];
            }
        }

        if (!string.IsNullOrEmpty(command) && !string.IsNullOrEmpty(profile))
        {
            error = true;
            EmitErrorMessage(
                0, 0, 0,
                DS0194_DCWatchInvalidCombination,
                "'dc watch': The options '--command' and '--profile' cannot be combined.",
                "dc");
        }

        if (error)
            return -1;

        if (string.IsNullOrEmpty(profile) && string.IsNullOrEmpty(command))
            command = "build";

        string processArgs = command;

        if (!string.IsNullOrEmpty(profile))
            processArgs = $"build {profile}";

        if (!string.IsNullOrEmpty(dir))
            processArgs = dir;

        LogOut.Write("Watching file changes. Use ");

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("dc quit");
        Console.ForegroundColor = ConsoleColor.Gray;

        LogOut.WriteLine(" to stop watching changes.");

        watchProcess = new Process();
#if STANDALONE
        watchProcess.StartInfo.FileName = $"{Environment.GetCommandLineArgs()[0]}";
        watchProcess.StartInfo.Arguments = $"watch-indefinetly {processArgs}";
#else
        watchProcess.StartInfo.FileName = "dotnet";
        watchProcess.StartInfo.Arguments = $"{Assembly.GetCallingAssembly().Location} watch-indefinetly {processArgs}";
#endif
        watchProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        watchProcess.StartInfo.CreateNoWindow = true;
        watchProcess.Start();

        using StreamWriter sw = File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "pid.txt"));
        sw.WriteLine(watchProcess.Id);
        return 0;
    }

    public static int WatchIndefinetly(string[] args)
    {
        FileSystemWatcher watcher;

        if (args != null && args.Length > 0 && Directory.Exists(args[0]))
            watcher = new(args[0]);
        else
            watcher = new(Directory.GetCurrentDirectory(), "*.ds");

        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;

#if STANDALONE
            string cmd = $"{Environment.GetCommandLineArgs()[0]} {string.Join(" ", args)}";
#else
        string cmd = $"{Assembly.GetCallingAssembly().Location} {string.Join(" ", args)}";
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

        while (true)
            watcher.WaitForChanged(WatcherChangeTypes.All);
    }
}