using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SDProcess = System.Diagnostics.Process;

namespace Dassie.Core.Commands;

internal class WatchCommand : CompilerCommand
{
    private static WatchCommand _instance;
    public static WatchCommand Instance => _instance ??= new();
    
    internal static SDProcess watchProcess = null;

    public override string Command => "watch";

    public override string Description => StringHelper.WatchCommand_Description;

    public override List<string> Aliases => ["auto"];

    public override CommandHelpDetails HelpDetails => new()
    {
        Description = StringHelper.WatchCommand_Description,
        Usage =
        [
            "dc watch",
            "dc watch -c|--command <Command>",
            "dc watch -p|--profile <Profile>",
            "dc watch <Directory>",
            "dc watch --quit"
        ],
        Options =
        [
            ("-c|--command <Command>", StringHelper.WatchCommand_CommandOptionDescription),
            ("-p|--profile <Profile>", StringHelper.WatchCommand_ProfileOptionDescription),
            ("<Directory>", StringHelper.WatchCommand_DirectoryOptionDescription),
            ("--quit", StringHelper.WatchCommand_QuitOptionDescription)
        ],
        Examples =
        [
            ("dc watch", StringHelper.WatchCommand_Example1),
            ("dc watch -c run", StringHelper.WatchCommand_Example2),
            ("dc watch -p Release", StringHelper.WatchCommand_Example3),
            ("dc watch ./src", StringHelper.WatchCommand_Example4)
        ]
    };

    public override int Invoke(string[] args)
    {
        if (args.Contains("--quit"))
            return Quit(args.Except(["--quit"]).ToArray());

        if (args.Contains("--indefinitely"))
            return WatchIndefinitely(args.Except(["--indefinitely"]).ToArray());

        bool error = false;
        string command = "";
        string profile = "";
        string dir = "";

        if (args != null && args.Length > 0)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith('-'))
                {
                    if (args.Length < i + 2)
                    {
                        EmitErrorMessageFormatted(
                            0, 0, 0,
                            DS0194_ExpectedCliOptionValue,
                            nameof(StringHelper.WatchCommand_ExpectedValue), [args[i]],
                            CompilerExecutableName);

                        return -1;
                    }
                }

                if (args[i] == "-c" || args[i] == "--command")
                {
                    if (!string.IsNullOrEmpty(command))
                    {
                        error = true;
                        EmitErrorMessageFormatted(
                            0, 0, 0,
                            DS0195_DCWatchInvalidCombination,
                            nameof(StringHelper.WatchCommand_CommandAlreadySet), [command],
                            CompilerExecutableName);
                    }

                    command = args[i++ + 1];
                    continue;
                }

                if (args[i] == "-p" || args[i] == "--profile")
                {
                    if (!string.IsNullOrEmpty(profile))
                    {
                        error = true;
                        EmitErrorMessageFormatted(
                            0, 0, 0,
                            DS0195_DCWatchInvalidCombination,
                            nameof(StringHelper.WatchCommand_ProfileAlreadySet), [profile],
                            CompilerExecutableName);
                    }

                    profile = args[i++ + 1];
                    continue;
                }

                if (!string.IsNullOrEmpty(command) || !string.IsNullOrEmpty(profile))
                {
                    error = true;
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0195_DCWatchInvalidCombination,
                        nameof(StringHelper.WatchCommand_TargetOnlyWithoutOptions), [],
                        CompilerExecutableName);
                }

                if (!Directory.Exists(args[i]))
                {
                    error = true;
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0195_DCWatchInvalidCombination,
                        nameof(StringHelper.WatchCommand_DirectoryNotExist), [args[i]],
                        CompilerExecutableName);
                }

                if (!string.IsNullOrEmpty(dir))
                {
                    error = true;
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0195_DCWatchInvalidCombination,
                        nameof(StringHelper.WatchCommand_TargetDirectoryAlreadySet), [args[i]],
                        CompilerExecutableName);
                }

                dir = args[i];
            }
        }

        if (!string.IsNullOrEmpty(command) && !string.IsNullOrEmpty(profile))
        {
            error = true;
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0195_DCWatchInvalidCombination,
                nameof(StringHelper.WatchCommand_CommandAndProfileCannotCombine), [],
                CompilerExecutableName);
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

        LogOut.Write(StringHelper.WatchCommand_WatchingFileChanges);

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write(StringHelper.WatchCommand_StopWatchingCommand);
        Console.ForegroundColor = ConsoleColor.Gray;

        LogOut.WriteLine(StringHelper.WatchCommand_ToStopWatching);
        
        watchProcess = new SDProcess();
#if STANDALONE
        watchProcess.StartInfo.FileName = $"{Environment.GetCommandLineArgs()[0]}";
        watchProcess.StartInfo.Arguments = $"watch-indefinitely {processArgs}";
#else
        watchProcess.StartInfo.FileName = "dotnet";
        watchProcess.StartInfo.Arguments = $"{Assembly.GetCallingAssembly().Location} watch --indefinitely {processArgs}";
#endif
        watchProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        watchProcess.StartInfo.CreateNoWindow = true;
        watchProcess.Start();

        using StreamWriter sw = File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "pid.txt"));
        sw.WriteLine(watchProcess.Id);
        return 0;
    }
    
    public static int WatchIndefinitely(string[] args)
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
            var buildProcess = new SDProcess();
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

    private static int Quit(string[] args)
    {
        if (args.Length > 0)
        {
            foreach (string arg in args)
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0212_UnexpectedArgument,
                    nameof(StringHelper.WatchCommand_UnexpectedArgument), [arg],
                    CompilerExecutableName);
            }

            return -1;
        }

        SDProcess[] processes = SDProcess.GetProcesses();

        string pidFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "pid.txt");
        if (!File.Exists(pidFilePath))
        {
            LogOut.WriteLine(StringHelper.WatchCommand_NoWatchersRunning);
            return 0;
        }

        File.ReadAllLines(pidFilePath).Select(int.Parse).ToList().ForEach(i =>
        {
            if (processes.Any(p => p.Id == i))
                SDProcess.GetProcessById(i).Kill();
        });

        File.Delete(pidFilePath);

        LogOut.WriteLine(StringHelper.WatchCommand_NoLongerWatching);
        return 0;
    }
}