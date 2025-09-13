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

    public override string Description => "Watches all .ds files in the current folder structure and automatically recompiles when files are changed.";

    public override List<string> Aliases => ["auto"];

    public override CommandHelpDetails HelpDetails => new()
    {
        Description = "Watches all .ds files in the current folder structure and automatically recompiles when files are changed.",
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
            ("-c|--command <Command>", "Specifies the compiler command that is executed when files are changed. The default value is 'build'."),
            ("-p|--profile <Profile>", "Specifies the build profile that is used when files are changed. If this option is set, the '--command' option cannot be used."),
            ("<Directory>", "Specifies the directory that is watched for changed source files. Cannot be combined with the '--command' and '--profile' options."),
            ("--quit", "Stops all currently running watchers.")
        ],
        Examples =
        [
            ("dc watch", "Watches all .ds files in the current directory and its subdirectories and automatically recompiles using the default build profile when files are changed."),
            ("dc watch -c run", "Watches all .ds files in the current directory and its subdirectories and automatically executes the 'dc run' command when files are changed."),
            ("dc watch -p CustomProfile", "Watches all .ds files in the current directory and its subdirectories and automatically recompiles using the 'CustomProfile' build profile when files are changed."),
            ("dc watch ./src", "Watches all .ds files in the './src' directory and its subdirectories and automatically recompiles using the default build profile when files are changed."),
            ("dc watch --quit", "Stops all file watchers.")
        ]
    };

    public override int Invoke(string[] args)
    {
        if (args.Contains("--quit"))
            return Quit(args.Except(["--quit"]).ToArray());

        if (args.Contains("--indefinetly"))
            return WatchIndefinetly(args.Except(["--indefinetly"]).ToArray());

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
                        EmitErrorMessage(
                            0, 0, 0,
                            DS0194_ExpectedCliOptionValue,
                            $"Expected value for option '{args[i]}'.",
                            CompilerExecutableName);

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
                            DS0195_DCWatchInvalidCombination,
                            $"'dc watch': Command to execute was already set to '{command}'",
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
                        EmitErrorMessage(
                            0, 0, 0,
                            DS0195_DCWatchInvalidCombination,
                            $"'dc watch': Build profile was already set to '{profile}'",
                            CompilerExecutableName);
                    }

                    profile = args[i++ + 1];
                    continue;
                }

                if (!string.IsNullOrEmpty(command) || !string.IsNullOrEmpty(profile))
                {
                    error = true;
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0195_DCWatchInvalidCombination,
                        "'dc watch': Compilation target can only be manually specified if neither '--profile' nor '--command' are set.",
                        CompilerExecutableName);
                }

                if (!Directory.Exists(args[i]))
                {
                    error = true;
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0195_DCWatchInvalidCombination,
                        $"'dc watch': Directory '{args[i]}' does not exist.",
                        CompilerExecutableName);
                }

                if (!string.IsNullOrEmpty(dir))
                {
                    error = true;
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0195_DCWatchInvalidCombination,
                        $"'dc watch': Compilation target directory was already set to '{args[i]}'.",
                        CompilerExecutableName);
                }

                dir = args[i];
            }
        }

        if (!string.IsNullOrEmpty(command) && !string.IsNullOrEmpty(profile))
        {
            error = true;
            EmitErrorMessage(
                0, 0, 0,
                DS0195_DCWatchInvalidCombination,
                "'dc watch': The options '--command' and '--profile' cannot be combined.",
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

        LogOut.Write("Watching file changes. Use ");

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("dc watch --quit");
        Console.ForegroundColor = ConsoleColor.Gray;

        LogOut.WriteLine(" to stop watching changes.");
        
        watchProcess = new SDProcess();
#if STANDALONE
        watchProcess.StartInfo.FileName = $"{Environment.GetCommandLineArgs()[0]}";
        watchProcess.StartInfo.Arguments = $"watch-indefinetly {processArgs}";
#else
        watchProcess.StartInfo.FileName = "dotnet";
        watchProcess.StartInfo.Arguments = $"{Assembly.GetCallingAssembly().Location} watch --indefinetly {processArgs}";
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
                EmitErrorMessage(
                    0, 0, 0,
                    DS0212_UnexpectedArgument,
                    $"Unexpected argument '{arg}'.",
                    CompilerExecutableName);
            }

            return -1;
        }

        SDProcess[] processes = SDProcess.GetProcesses();

        string pidFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "pid.txt");
        if (!File.Exists(pidFilePath))
        {
            LogOut.WriteLine("No file watchers running.");
            return 0;
        }

        File.ReadAllLines(pidFilePath).Select(int.Parse).ToList().ForEach(i =>
        {
            if (processes.Any(p => p.Id == i))
                SDProcess.GetProcessById(i).Kill();
        });

        File.Delete(pidFilePath);

        LogOut.WriteLine("No longer watching file changes.");
        return 0;
    }
}