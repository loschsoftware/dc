using LoschScript.Templates;
using System;
using System.Diagnostics;
using System.IO;

namespace LoschScript.CLI;

internal class Program
{
    static int Main(string[] args)
    {
        //#if RELEASE
        try
        {
            //#endif
            return args switch
            {
                ["config"] => Helpers.BuildLSConfig(),
                ["build", ..] => Helpers.CompileAll(args[1..]),
                ["interactive" or "repl"] => Interactive.StartInteractiveSession(),
                ["interpret" or "run", ..] => Helpers.InterpretFiles(args),
                ["make" or "new", ..] => LSTemplates.CreateStructure(args),
                ["watch" or "auto", ..] => WatchForFileChanges(args),
                ["call", ..] => Helpers.CallMethod(args),
                ["-watch-indefinetly"] => WatchIndefinetly(string.Join(" ", args)),
                ["-viewfrags", ..] => Helpers.ViewFragments(args),
                ["quit"] => QuitWatching(),
                [] or ["help" or "?"] => DisplayHelpMessage(),
                _ => Helpers.HandleArgs(args)
            };
            //#if RELEASE
        }
        catch (Exception ex)
        {
            if (messages.Count == 0)
            {
                EmitErrorMessage(0, 0, 0, LS0000_UnexpectedError, $"Unhandled exception of type '{ex.GetType()}'.", "lsc.exe");
                Console.WriteLine();
            }

            return -1;
        }
        //#endif
    }

    static Process watchProcess = null;

    static int WatchForFileChanges(string[] args)
    {
        string _args = string.Join(" ", args);

        LogOut.Write("Watching file changes. Use ");

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("lsc quit");
        Console.ForegroundColor = ConsoleColor.Gray;

        LogOut.WriteLine(" to stop watching changes.");

        watchProcess = new Process();
        watchProcess.StartInfo.FileName = "lsc.exe";
        watchProcess.StartInfo.Arguments = $"build {_args}";
        watchProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        watchProcess.StartInfo.CreateNoWindow = true;
        watchProcess.Start();
        watchProcess.WaitForExit();

        watchProcess = new Process();
        watchProcess.StartInfo.FileName = "lsc.exe";
        watchProcess.StartInfo.Arguments = "-watch-indefinetly";
        watchProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        watchProcess.StartInfo.CreateNoWindow = true;
        watchProcess.Start();

        return 0;
    }

    static int QuitWatching()
    {
        LogOut.WriteLine("No longer watching file changes.");

        watchProcess = new Process();
        watchProcess.StartInfo.FileName = "taskkill.exe";
        watchProcess.StartInfo.Arguments = "/f /im lsc.exe";
        watchProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        watchProcess.StartInfo.CreateNoWindow = true;
        watchProcess.Start();
        return 0;
    }

    static int WatchIndefinetly(string args)
    {
        while (true)
        {
            FileSystemWatcher watcher = new(Directory.GetCurrentDirectory(), "*.ls")
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };

            string cmd = $"build {args}";

            watcher.Changed += Compile;
            watcher.Created += Compile;
            watcher.Deleted += Compile;

            void Compile(object sender, FileSystemEventArgs e)
            {
                var buildProcess = new Process();
                buildProcess.StartInfo.FileName = "lsc.exe";
                buildProcess.StartInfo.Arguments = cmd;
                buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                buildProcess.Start();
                buildProcess.WaitForExit();
            }

            watcher.WaitForChanged(WatcherChangeTypes.All);
        }
    }

    static int DisplayHelpMessage()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.WriteLine();
        LogOut.WriteLine("LoschScript Compiler Command Line (lsc.exe)");
        LogOut.WriteLine("Command Line Arguments:");
        LogOut.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("<FileName> [<FileName>..]".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        LogOut.WriteLine("Compiles the specified source files.");

        LogOut.WriteLine();
        LogOut.WriteLine("The following switches are valid for this argument:");
        LogOut.WriteLine();

        LogOut.Write("-i".PadRight(25).PadRight(50));
        LogOut.WriteLine("Interprets the program and doesn't save an assembly to the disk.");
        LogOut.Write("-diagnostics".PadRight(25).PadRight(50));
        LogOut.WriteLine("Provides advanced diagnostic information.");
        LogOut.Write("-elapsed".PadRight(25).PadRight(50));
        LogOut.WriteLine("Measures the elapsed build time.");
        LogOut.Write("-default".PadRight(25).PadRight(50));
        LogOut.WriteLine("Uses the default configuration and ignores lsconfig.xml files.");
        LogOut.Write("-out:<FileName>".PadRight(25).PadRight(50));
        LogOut.WriteLine("Specifies the output assembly name, ignoring lsconfig.xml.");
        LogOut.Write("-optimize".PadRight(25).PadRight(50));
        LogOut.WriteLine("Applies IL optimizations to the assembly, ignoring lsconfig.xml.");
        LogOut.Write("-ilout".PadRight(25).PadRight(50));
        LogOut.WriteLine("Saves the generated CIL code to the disk in a human-readable format.");
        LogOut.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("new <Type> <Name>".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        LogOut.WriteLine("Creates the file structure of a LoschScript project.");

        LogOut.WriteLine();
        LogOut.WriteLine("Possible values for \"Type\" are:");
        LogOut.WriteLine();

        LogOut.Write("console".PadRight(25).PadRight(50));
        LogOut.WriteLine("Specifies a command-line application.");
        LogOut.Write("library".PadRight(25).PadRight(50));
        LogOut.WriteLine("Specifies a dynamic linked library.");
        LogOut.Write("script".PadRight(25).PadRight(50));
        LogOut.WriteLine("A script can be used to run LoschScript code embedded in LS/.NET applications.");
        LogOut.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("build".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        LogOut.WriteLine("Compiles all .ls source files in the current directory.");
        //LogOut.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("watch, auto".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        LogOut.WriteLine("Watches all .ls files in the current directory and automatically recompiles when files are changed.");

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("quit".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        LogOut.WriteLine("Stops all file watchers.");

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("check <FileName>".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        LogOut.WriteLine("Checks the specified file for syntax errors.");

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("optimize <FileName>".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        LogOut.WriteLine("Applies IL optimizations to the specified assembly.");
        //LogOut.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("merge <OutputFileName> [<FileName>..]".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        LogOut.WriteLine("Merges the specified assemblies into one.");
        //LogOut.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("config".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        LogOut.WriteLine("Creates a new lsconfig.xml file with default values.");
        //LogOut.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("p, preprocess <FileName>".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        LogOut.WriteLine("Preprocesses <FileName>.");

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("interactive, repl".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        LogOut.WriteLine("Provides a read-evaluate-print-loop to run single expressions.");

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("help, ?".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        LogOut.WriteLine("Shows this page.");

        LogOut.WriteLine();
        LogOut.WriteLine("Valid prefixes for options are -, --, and /.");
        return 0;
    }
}