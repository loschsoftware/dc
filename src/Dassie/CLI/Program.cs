using Dassie.Configuration;
using Dassie.CLI.Interactive;
using Dassie.Templates;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace Dassie.CLI;

internal class Program
{
    static int Main(string[] args)
    {

#if NET7_COMPATIBLE
        ConsoleColor prev = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Warning: This is not a standalone build of the Dassie compiler and might not be able to execute some actions.");
        Console.ForegroundColor = prev;
#endif

        try
        {
            return args switch
            {
                ["config"] => CliHelpers.BuildDassieConfig(),
                ["build", ..] => CliHelpers.CompileAll(args[1..]),
                ["check" or "verify"] => CliHelpers.CheckAll(),
                ["check" or "verify", ..] => CliHelpers.Check(args[1..]),
                ["interactive" or "repl"] => InteractiveShell.Start(),
                ["interpret" or "run", ..] => CliHelpers.InterpretFiles(args),
                ["make" or "new", ..] => DSTemplates.CreateStructure(args),
                ["watch" or "auto", ..] => WatchForFileChanges(args),
                ["call", ..] => CliHelpers.CallMethod(args),
                ["-watch-indefinetly"] => WatchIndefinetly(string.Join(" ", args)),
                ["-viewfrags", ..] => CliHelpers.ViewFragments(args),
                ["quit"] => QuitWatching(),
                [] or ["help" or "?"] => DisplayHelpMessage(),
                _ => CliHelpers.HandleArgs(args)
            };
        }
        catch (Exception ex)
        {
            if (ex is IOException)
                EmitErrorMessage(0, 0, 0, DS0029_FileAccessDenied, $"Access to file '{Path.GetFileName(CurrentFile.Path)}' denied.");

            if (messages.Count == 0)
            {
                EmitErrorMessage(0, 0, 0, DS0000_UnexpectedError, $"Unhandled exception of type '{ex.GetType()}'.", "dc.exe");
                Console.WriteLine();
            }

            if (Debugger.IsAttached)
                throw;

            if (File.Exists("dsconfig.xml"))
            {
                try
                {
                    XmlSerializer xmls = new(typeof(DassieConfig));
                    using StreamReader sr = new("dsconfig.xml");

                    DassieConfig config = (DassieConfig)xmls.Deserialize(sr);

                    if (config.PrintExceptionInfo)
                        Console.WriteLine(ex);
                }
                catch { }
            }

            return -1;
        }
    }

    static Process watchProcess = null;

    static int WatchForFileChanges(string[] args)
    {
        string _args = string.Join(" ", args);

        LogOut.Write("Watching file changes. Use ");

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("dc quit");
        Console.ForegroundColor = ConsoleColor.Gray;

        LogOut.WriteLine(" to stop watching changes.");

        watchProcess = new Process();
        watchProcess.StartInfo.FileName = "dc.exe";
        watchProcess.StartInfo.Arguments = $"build {_args}";
        watchProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        watchProcess.StartInfo.CreateNoWindow = true;
        watchProcess.Start();
        watchProcess.WaitForExit();

        watchProcess = new Process();
        watchProcess.StartInfo.FileName = "dc.exe";
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
        watchProcess.StartInfo.Arguments = "/f /im dc.exe";
        watchProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        watchProcess.StartInfo.CreateNoWindow = true;
        watchProcess.Start();
        return 0;
    }

    static int WatchIndefinetly(string args)
    {
        while (true)
        {
            FileSystemWatcher watcher = new(Directory.GetCurrentDirectory(), "*.ds")
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
                buildProcess.StartInfo.FileName = "dc.exe";
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
        Version v = Assembly.GetExecutingAssembly().GetName().Version;

        // 8517 -> Days between 01/01/2000 and 27/04/2023, on which development on dc was started
        Version version = new(v.Major, v.Minor, v.Build - 8517);
        DateTime buildDate = new DateTime(2000, 1, 1).AddDays(v.Build);

        ConsoleColor def = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.Yellow;

        LogOut.WriteLine();
        LogOut.WriteLine("Dassie Command Line Compiler for .NET (dc.exe)");
        LogOut.WriteLine($"Version {version.ToString(2)}, Build {version.Build} ({buildDate.ToShortDateString()})");

        Console.ForegroundColor = def;

        LogOut.WriteLine();
        LogOut.WriteLine("Command Line Arguments:");
        LogOut.WriteLine();

        LogOut.Write("<FileName> [<FileName>..]".PadRight(50));
        LogOut.WriteLine("Compiles the specified source files.");

        //Console.ForegroundColor = ConsoleColor.Yellow;

        //LogOut.Write("    -i".PadRight(25).PadRight(50));
        //LogOut.WriteLine("Interprets the program and doesn't save an assembly to the disk.");
        //LogOut.Write("    -diagnostics".PadRight(25).PadRight(50));
        //LogOut.WriteLine("Provides advanced diagnostic information.");
        //LogOut.Write("    -elapsed".PadRight(25).PadRight(50));
        //LogOut.WriteLine("Measures the elapsed build time.");
        //LogOut.Write("    -default".PadRight(25).PadRight(50));
        //LogOut.WriteLine("Uses the default configuration and ignores dsconfig.xml files.");
        //LogOut.Write("    -out:<FileName>".PadRight(25).PadRight(50));
        //LogOut.WriteLine("Specifies the output assembly name, ignoring dsconfig.xml.");
        //LogOut.Write("    -optimize".PadRight(25).PadRight(50));
        //LogOut.WriteLine("Applies IL optimizations to the assembly, ignoring dsconfig.xml.");
        //LogOut.Write("    -ilout".PadRight(25).PadRight(50));
        //LogOut.WriteLine("Saves the generated CIL code to the disk in a human-readable format.");
        //LogOut.Write("    -rc".PadRight(25).PadRight(50));
        //LogOut.WriteLine("Emits a Windows resource script (.rc file) associated with the program.");

        //Console.ForegroundColor = def;

        LogOut.WriteLine();

        LogOut.Write("new <Type> <Name>".PadRight(50));

        LogOut.WriteLine("Creates the file structure of a Dassie project.");

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write("    console".PadRight(25).PadRight(50));
        LogOut.WriteLine("Specifies a command-line application.");
        LogOut.Write("    library".PadRight(25).PadRight(50));
        LogOut.WriteLine("Specifies a dynamic linked library.");
        //LogOut.Write("    script".PadRight(25).PadRight(50));
        //LogOut.WriteLine("A script can be used to run Dassie code embedded in LS/.NET applications.");
        LogOut.WriteLine();
        Console.ForegroundColor = def;

        LogOut.Write("build".PadRight(50));
        LogOut.WriteLine("Compiles all .ds source files in the current directory.");
        LogOut.WriteLine();

        LogOut.Write("watch, auto".PadRight(50));
        LogOut.WriteLine("Watches all .ds files in the current folder structure and automatically recompiles when files are changed.");
        LogOut.WriteLine();

        LogOut.Write("quit".PadRight(50));
        LogOut.WriteLine("Stops all file watchers.");
        LogOut.WriteLine();

        LogOut.Write("check, verify [<FileName>..]".PadRight(50));
        LogOut.WriteLine("Checks the specified files, or all .ds files in the current folder structure, for syntax errors.");
        LogOut.WriteLine();

        LogOut.Write("config".PadRight(50));
        LogOut.WriteLine("Creates a new dsconfig.xml file with default values.");
        LogOut.WriteLine();

        //LogOut.Write("p, preprocess <FileName>".PadRight(50));
        //LogOut.WriteLine("Preprocesses <FileName>.");
        //LogOut.WriteLine();

        //LogOut.Write("interactive, repl".PadRight(50));
        //LogOut.WriteLine("Provides a read-evaluate-print-loop to run single expressions.");
        //LogOut.WriteLine();

        LogOut.Write("help, ?".PadRight(50));
        LogOut.WriteLine("Shows this page.");

        LogOut.WriteLine();
        //LogOut.WriteLine("Valid prefixes for options are -, --, and /.");
        return 0;
    }
}