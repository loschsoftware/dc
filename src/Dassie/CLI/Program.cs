﻿using Dassie.Configuration;
using Dassie.Extensions;
using Dassie.Meta;
using Dassie.Templates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Dassie.CLI;

internal class Program
{
    internal static int Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = Encoding.Unicode;
            GetOrCreateToolPathsFile();

            List<IPackage> extensions = ExtensionLoader.LoadInstalledExtensions();
            Dictionary<string, Func<string[], int>> customCommands = ExtensionLoader.GetAllCommands(extensions);
            Dictionary<string, string> commandDescriptions = ExtensionLoader.GetCommandDescriptions(extensions);

            args ??= [];
            if (args.Length == 0)
                return DisplayHelpMessage(commandDescriptions);

            string command = args[0];
            if (customCommands.TryGetValue(command, out Func<string[], int> cmd))
                return cmd(args[1..]);

            return args switch
            {
                ["config"] => CliHelpers.BuildDassieConfig(),
                ["build", ..] => CliHelpers.CompileAll(args[1..]),
                ["run", ..] => CliHelpers.Run(args[1..]),
                ["check" or "verify"] => CliHelpers.CheckAll(),
                ["check" or "verify", ..] => CliHelpers.Check(args[1..]),
                ["make" or "new", ..] => DSTemplates.CreateStructure(args),
                ["watch" or "auto", ..] => WatchForFileChanges(args),
                ["scratchpad", ..] => Scratchpad.HandleScratchpadCommands(args[1..]),
                ["package", ..] => ExtensionManagerCommandLine.HandleArgs(args[1..]),
                ["-watch-indefinetly"] => WatchIndefinetly(string.Join(" ", args)),
                ["quit"] => QuitWatching(),
                [] or ["help" or "?" or "-h" or "--help" or "/?" or "/help"] => DisplayHelpMessage(commandDescriptions),
                _ => CliHelpers.HandleArgs(args)
            };
        }
        catch (Exception ex)
        {
            if (ex is IOException ioEx)
                EmitErrorMessage(0, 0, 0, DS0029_FileAccessDenied, $"File access denied.");

            else if (ex is UnauthorizedAccessException uaEx)
                EmitErrorMessage(0, 0, 0, DS0029_FileAccessDenied, $"File access denied.");

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

    private static void GetOrCreateToolPathsFile()
    {
        XmlSerializer xmls = new(typeof(ToolPaths));

        Directory.CreateDirectory(Path.GetDirectoryName(ToolPaths.ToolPathsFile));
        if (File.Exists(ToolPaths.ToolPathsFile))
        {
            using StreamReader sr = new(ToolPaths.ToolPathsFile);
            GlobalConfig.ExternalToolPaths = (ToolPaths)xmls.Deserialize(sr);
            return;
        }

        GlobalConfig.ExternalToolPaths = new()
        {
            Tools = []
        };

        using StreamWriter sw = new(ToolPaths.ToolPathsFile);
        xmls.Serialize(sw, GlobalConfig.ExternalToolPaths);
    }

    public static void DisplayLogo()
    {
        Version v = Assembly.GetExecutingAssembly().GetName().Version;

        // 8517 -> Days between 01/01/2000 and 27/04/2023, on which development on dc was started
        Version version = new(v.Major, v.Minor, v.Build - 8517);
        DateTime buildDate = new DateTime(2000, 1, 1).AddDays(v.Build);

        ConsoleColor def = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.Yellow;

        LogOut.WriteLine();
        LogOut.WriteLine("Dassie Command Line Compiler for .NET");
        LogOut.WriteLine($"Version {version.ToString(2)}, Build {version.Build} ({buildDate.ToShortDateString()})");

        Console.ForegroundColor = def;
    }
    
    public static string FormatLines(string text, bool initialPadLeft = false, int indentWidth = 50)
    {
        int maxWidth = Console.BufferWidth - 50 - 5;

        if (maxWidth < 30)
            return $"{text}{Environment.NewLine}";

        StringBuilder sb = new();
        string[] words = text.Split(' ');

        while (words.Length > 0)
        {
            StringBuilder lineBuilder = new();

            while (words.Length > 0 && lineBuilder.Length + words[0].Length < maxWidth)
            {
                lineBuilder.Append(words[0] + " ");
                words = words.Skip(1).ToArray();
            }

            string line = lineBuilder.ToString();

            if (initialPadLeft || sb.Length > 0)
                line = line.PadLeft(indentWidth + line.Length);

            sb.AppendLine(line);
        }

        return sb.ToString();
    }

    static int DisplayHelpMessage(Dictionary<string, string> installedCommands)
    {
        if (Console.BufferWidth - 50 - 5 < 30)
        {
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Increase the console width for better viewing experience.");
            Console.ForegroundColor = prev;
        }

        StringBuilder sb = new();

        sb.AppendLine();
        sb.AppendLine("Usage:");

        sb.Append("    dc <Command> [Options]".PadRight(50));
        sb.Append(FormatLines("Executes a command from the list below."));

        sb.Append("    dc <FileNames> [Options]".PadRight(50));
        sb.AppendLine("Compiles the specified source files.");

        sb.AppendLine();
        sb.AppendLine("Commands:");

        sb.Append("    new <Type> <Name>".PadRight(50));

        sb.AppendLine("Creates the file structure of a Dassie project.");

        sb.Append("        console".PadRight(25).PadRight(50));
        sb.AppendLine("Specifies a command-line application.");
        sb.Append("        library".PadRight(25).PadRight(50));
        sb.AppendLine("Specifies a dynamic linked library.");
        sb.AppendLine();

        sb.Append("    build [BuildProfile]".PadRight(50));
        sb.Append(FormatLines("Executes the specified build profile, or compiles all .ds source files in the current directory if none is specified."));

        sb.Append("    run [Arguments]".PadRight(50));
        sb.Append(FormatLines("Executes the output executable of the current project with the specified arguments. Does not recompile the project."));

        sb.Append("    watch, auto".PadRight(50));
        sb.Append(FormatLines("Watches all .ds files in the current folder structure and automatically recompiles when files are changed."));

        sb.Append("    quit".PadRight(50));
        sb.Append(FormatLines("Stops all file watchers."));

        sb.Append("    scratchpad [Command] [Options]".PadRight(50));
        sb.Append(FormatLines("Allows compiling and running Dassie source code from the console. Use 'dc scratchpad help' to display available commands."));

        sb.Append("    check, verify [FileNames]".PadRight(50));
        sb.Append(FormatLines("Checks the specified files, or all .ds files in the current folder structure, for syntax errors."));

        sb.Append("    config".PadRight(50));
        sb.Append(FormatLines("Creates a new dsconfig.xml file with default values."));

        sb.Append("    package [Command] [Options]".PadRight(50));
        sb.Append(FormatLines("Used to install and manage compiler extensions. Use 'dc package help' to display available commands."));

        sb.Append("    help, ?".PadRight(50));
        sb.Append(FormatLines("Shows this page."));

        if (installedCommands.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("External commands:");

            foreach (KeyValuePair<string, string> cmd in installedCommands)
                sb.Append($"{$"    {cmd.Key}",-50}{FormatLines(cmd.Value.Replace(Environment.NewLine, " "))}");
        }

        sb.AppendLine();
        sb.AppendLine("Options:");
        sb.AppendLine(FormatLines("Options from project files (dsconfig.xml) can be included in the following way:", true, 4));
        
        sb.Append("    --<PropertyName>=<Value>".PadRight(50));
        sb.Append(FormatLines("For simple properties of type 'string', 'bool' or 'enum'. The property name is case-insensitive. For boolean properties, 0 and 1 are supported aliases for false and true. Example: --MeasureElapsedTime=true"));

        sb.Append("    --<ArrayPropertyName>+<Value>".PadRight(50));
        sb.Append(FormatLines("To add elements to an array property. Property names are recognized by the first characters, where 'References' takes precedence over 'Resources'. Example: --R+\"assembly.dll\""));

        sb.Append("    --<PropertyName>::<ChildProperty>=<Value>".PadRight(50));
        sb.Append(FormatLines("For setting child properties of more complex objects. Object names are recognized by first characters. Example: --VersionInfo::Description=\"Application\""));

        DisplayLogo();
        LogOut.Write(sb.ToString());
        return 0;
    }
}