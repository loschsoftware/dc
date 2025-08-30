using Dassie.Configuration;
using Dassie.Errors;
using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Dassie.Cli.Commands;

internal class ScratchpadCommand : ICompilerCommand
{
    /// <summary>
    /// Provides tools for managing and starting "scratchpad" sessions, which allow the user to compile and execute applications using source code from standard input.
    /// </summary>
    internal static class Scratchpad
    {
        public static int HandleScratchpadCommands(string[] args)
        {
            args ??= [];

            if (args.Length == 0)
                args = ["new"];

            string command = args[0];

            if (command == "delete" && args.Length > 1)
                return DeleteScratch(args[1]);

            if (command == "clear")
                return ClearScratches();

            if (command == "list")
                return ListScratches();

            if (command == "load" && args.Length > 1)
                return LoadScratch(args[1]);

            if (command == "new" || command.StartsWith("--"))
                return CompileFromStdIn(args);

            return ShowUsage();
        }

        /// <summary>
        /// Deletes all saved scratches.
        /// </summary>
        /// <returns>Always returns the exit code 0.</returns>
        private static int ClearScratches()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "Dassie", "Scratchpad");
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Console.WriteLine();
            Console.WriteLine("Cleared all scratches.");
            return 0;
        }

        /// <summary>
        /// Deletes the specified scratch.
        /// </summary>
        /// <param name="name">The name of the scratch to delete.</param>
        /// <returns>The exit code.</returns>
        private static int DeleteScratch(string name)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "Dassie", "Scratchpad", name);
            if (!Directory.Exists(path))
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0099_ScratchNotFound,
                    $"The scratch '{name}' could not be found.",
                    CompilerExecutableName);

                return -1;
            }

            Directory.Delete(path, true);
            return 0;
        }

        /// <summary>
        /// Loads the specified scratch for editing.
        /// </summary>
        /// <param name="name">The name of the scratch to load.</param>
        /// <returns>The exit code.</returns>
        private static int LoadScratch(string name)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "Dassie", "Scratchpad", name);
            if (!Directory.Exists(path))
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0099_ScratchNotFound,
                    $"The scratch '{name}' could not be found.",
                    CompilerExecutableName);

                return -1;
            }

            throw new NotImplementedException("Loading existing scratches is not yet implemented.");
        }

        /// <summary>
        /// Displays a list of all saved scratches.
        /// </summary>
        /// <returns>Always returns the exit code 0.</returns>
        private static int ListScratches()
        {
            Console.WriteLine("Saved scratches:");
            Console.WriteLine();

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "Dassie", "Scratchpad");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string[] scratches = Directory.GetDirectories(path);
            if (scratches.Length == 0)
            {
                Console.WriteLine("No scratches found.");
                return 0;
            }

            int dateStringLength = DateTime.Now.ToString().Length;
            Console.WriteLine($"{"Last modified".PadRight(dateStringLength)}\tName");

            foreach (DirectoryInfo di in scratches.Select(d => new DirectoryInfo(d)))
                Console.WriteLine($"{di.LastWriteTime}\t{di.Name}");

            return 0;
        }

        /// <summary>
        /// Allows the user to enter Dassie source code from the console, which will be compiled and executed.
        /// </summary>
        /// <returns>The exit code.</returns>
        private static int CompileFromStdIn(string[] args)
        {
            HelpCommand.DisplayLogo();
            Console.WriteLine();
            Console.WriteLine("To mark the end of the input, press Ctrl+Z in an empty line and hit Enter.");
            Console.WriteLine();

            string src = Console.In.ReadToEnd();
            Console.WriteLine();

            string dir = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "Dassie", "Scratchpad")).FullName;
            string scratchName = "scratch";
            int lowestAvailableScratchIndex = 0;
            foreach (string scratchDir in Directory.GetDirectories(dir).Select(d => d.Split(Path.DirectorySeparatorChar).Last()))
            {
                if (!scratchDir.StartsWith("scratch"))
                    continue;

                if (scratchDir.EndsWith($"{lowestAvailableScratchIndex:000}"))
                    lowestAvailableScratchIndex++;
            }

            string scratchNameIndexStr = $"{lowestAvailableScratchIndex:000}";
            if (lowestAvailableScratchIndex > 999)
                scratchNameIndexStr = lowestAvailableScratchIndex.ToString();

            scratchName = $"{scratchName}{scratchNameIndexStr}";

            if (args.Any(a => a.StartsWith("--name=")))
                scratchName = args.First(a => a.StartsWith("--name=")).Split('=')[1];

            dir = Path.Combine(dir, scratchName);
            Directory.CreateDirectory(dir);

            if (args.Any(a => a.StartsWith("--config=")))
            {
                string cfgFile = args.First(a => a.StartsWith("--config=")).Split("=")[1];
                if (File.Exists(cfgFile))
                    File.Copy(cfgFile, Path.Combine(dir, Path.GetFileName(cfgFile)));
            }

            string file = Path.Combine(dir, "scratch.ds");
            File.WriteAllText(file, src);

            Directory.SetCurrentDirectory(dir);

            DassieConfig cfg = ProjectFileDeserializer.DassieConfig;

            int result = CompileCommand.Instance.Invoke([file]);

            string outDir = Path.Combine(dir, "build");
            string asm = Path.Combine(outDir, Path.ChangeExtension(Path.GetFileName(file), "dll"));

            if (cfg != null)
            {
                if (!string.IsNullOrEmpty(cfg.BuildOutputDirectory))
                {
                    if (Directory.Exists(cfg.BuildOutputDirectory))
                        outDir = cfg.BuildOutputDirectory;

                    else // relative path
                        outDir = Path.Combine(dir, cfg.BuildOutputDirectory);

                    asm = Path.Combine(outDir, Path.ChangeExtension(file, "dll"));
                }

                if (!string.IsNullOrEmpty(cfg.AssemblyName))
                    asm = Path.Combine(outDir, $"{cfg.AssemblyName}.dll");
            }

            if (File.Exists(asm) && !Messages.Any(e => e.Severity == Severity.Error))
            {
                ProcessStartInfo psi = new()
                {
                    FileName = "dotnet",
                    Arguments = asm,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Process.Start(psi).WaitForExit();
            }

            return result;
        }

        /// <summary>
        /// Displays a help page for the Dassie scratchpad.
        /// </summary>
        /// <returns>Always returns 0.</returns>
        private static int ShowUsage()
        {
            HelpCommand.Instance.Invoke(["scratchpad"]);
            return 0;
        }
    }

    private static ScratchpadCommand _instance;
    public static ScratchpadCommand Instance => _instance ??= new();

    public string Command => "scratchpad";

    public string Description => "Allows compiling and running Dassie source code from the console. Use 'dc scratchpad help' to display available commands.";

    public List<string> Aliases() => ["sp"];

    public CommandHelpDetails HelpDetails()
    {
        StringBuilder commandsSb = new();
        commandsSb.Append($"{"    new [Options]",-35}{HelpCommand.FormatLines("Creates a new scratch.", indentWidth: 35)}");
        commandsSb.Append($"{"        --name=<Name>",-35}{HelpCommand.FormatLines("Specifies the name of the scratch.", indentWidth: 35)}");
        commandsSb.Append($"{"        --config=<Path>",-35}{HelpCommand.FormatLines("The compiler configuration (dsconfig.xml) file to use.", indentWidth: 35)}");
        commandsSb.AppendLine();

        commandsSb.Append($"{"    load <Name>",-35}{HelpCommand.FormatLines("Loads the specified scratch.", indentWidth: 35)}");
        commandsSb.Append($"{"    list",-35}{HelpCommand.FormatLines("Lists all saved scratches.", indentWidth: 35)}");
        commandsSb.Append($"{"    delete <Name>",-35}{HelpCommand.FormatLines("Deletes the specified scratch.", indentWidth: 35)}");
        commandsSb.Append($"{"    clear",-35}{HelpCommand.FormatLines("Deletes all saved scratches.", indentWidth: 35)}");
        commandsSb.Append($"{"    help",-35}{HelpCommand.FormatLines("Shows this list.", indentWidth: 35)}");

        return new()
        {
            Description = "Allows compiling and running Dassie source code from the console.",
            Usage = ["dc scratchpad [Command] [Options]"],
            Remarks = "If no command is specified, the command 'new' is used implicitly.",
            Options =
            [
                ("Command", "The subcommand to execute."),
                ("Options", "Additional options passed to the subcommand.")
            ],
            CustomSections =
            [
                ("Available commands", commandsSb.ToString())
            ]
        };
    } 

    public int Invoke(string[] args) => Scratchpad.HandleScratchpadCommands(args);
}
