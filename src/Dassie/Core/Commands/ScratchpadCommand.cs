using Dassie.Configuration;
using Dassie.Core.Properties;
using Dassie.Extensions;
using Dassie.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SDProcess = System.Diagnostics.Process;

namespace Dassie.Core.Commands;

internal class ScratchpadCommand : CompilerCommand
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
                return CompileFromEditor(args);

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
            Console.WriteLine(StringHelper.ScratchpadCommand_ClearedAllScratches);
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
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0099_ScratchNotFound,
                    nameof(StringHelper.ScratchpadCommand_ScratchNotFound), [name],
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
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0099_ScratchNotFound,
                    nameof(StringHelper.ScratchpadCommand_ScratchNotFound), [name],
                    CompilerExecutableName);

                return -1;
            }

            if (EditorProperty.Instance.GetValue().ToString().Equals("default", StringComparison.OrdinalIgnoreCase))
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0259_DCScratchpadLoadDefaultEditor,
                    nameof(StringHelper.ScratchpadCommand_LoadNotSupportedDefaultEditor), [],
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
            Console.WriteLine(StringHelper.ScratchpadCommand_SavedScratches);
            Console.WriteLine();
            
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "Dassie", "Scratchpad");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string[] scratches = Directory.GetDirectories(path);
            if (scratches.Length == 0)
            {
                Console.WriteLine(StringHelper.ScratchpadCommand_NoScratchesFound);
                return 0;
            }

            int dateStringLength = DateTime.Now.ToString().Length;
            Console.WriteLine($"{StringHelper.ScratchpadCommand_LastModified.PadRight(dateStringLength)}\t{StringHelper.ScratchpadCommand_Name}");

            foreach (DirectoryInfo di in scratches.Select(d => new DirectoryInfo(d)))
                Console.WriteLine($"{di.LastWriteTime}\t{di.Name}");

            return 0;
        }

        /// <summary>
        /// Allows the user to enter Dassie source code from the console, which will be compiled and executed.
        /// </summary>
        /// <returns>The exit code.</returns>
        private static int CompileFromEditor(string[] args)
        {
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

            if (!Console.IsInputRedirected)
            {
                HelpCommand.DisplayLogo();
                Console.WriteLine();
            }

            if (EditorProperty.Instance.GetValue().ToString().Equals("default", StringComparison.OrdinalIgnoreCase))
            {
                if (!Console.IsInputRedirected)
                {
                    Console.WriteLine(StringHelper.ScratchpadCommand_MarkEndOfInput);
                    Console.WriteLine();
                }

                File.WriteAllText(file, Console.In.ReadToEnd());

                if (!Console.IsInputRedirected)
                    Console.WriteLine();
            }
            else
            {
                File.Create(file).Dispose();
                SDProcess.Start(EditorProperty.Instance.GetValue().ToString(), Path.GetFullPath(file)).WaitForExit();
            }

            Directory.SetCurrentDirectory(dir);

            DassieConfig cfg = ProjectFileSerializer.DassieConfig;

            int result = CompileCommand.Instance.Invoke([file]);

            string outDir = Path.Combine(dir, "build");
            string asm = Path.Combine(outDir, Path.ChangeExtension(Path.GetFileName(file), "dll"));

            if (cfg != null)
            {
                if (!string.IsNullOrEmpty(cfg.BuildDirectory))
                {
                    if (Directory.Exists(cfg.BuildDirectory))
                        outDir = cfg.BuildDirectory;

                    else // relative path
                        outDir = Path.Combine(dir, cfg.BuildDirectory);

                    asm = Path.Combine(outDir, Path.ChangeExtension(file, "dll"));
                }

                if (!string.IsNullOrEmpty(cfg.AssemblyFileName))
                    asm = Path.Combine(outDir, $"{cfg.AssemblyFileName}.dll");
            }

            if (File.Exists(asm) && !EmittedMessages.Any(e => e.Severity == Severity.Error))
            {
                ProcessStartInfo psi = new()
                {
                    FileName = "dotnet",
                    Arguments = asm,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                SDProcess.Start(psi).WaitForExit();
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

    public override string Command => "scratchpad";

    public override string Description => StringHelper.ScratchpadCommand_Description;

    public override List<string> Aliases => ["sp"];

    public override CommandHelpDetails HelpDetails => GetHelpDetails();
    private static CommandHelpDetails GetHelpDetails()
    {
        StringBuilder commandsSb = new();
        commandsSb.Append($"{"    new [Options]",-35}{HelpCommand.FormatLines(StringHelper.ScratchpadCommand_NewDescription, indentWidth: 35)}");
        commandsSb.Append($"{"        --name=<Name>",-35}{HelpCommand.FormatLines(StringHelper.ScratchpadCommand_NameOption, indentWidth: 35)}");
        commandsSb.Append($"{"        --config=<Path>",-35}{HelpCommand.FormatLines(StringHelper.ScratchpadCommand_ConfigOption, indentWidth: 35)}");
        commandsSb.AppendLine();

        commandsSb.Append($"{"    load <Name>",-35}{HelpCommand.FormatLines(StringHelper.ScratchpadCommand_LoadDescription, indentWidth: 35)}");
        commandsSb.Append($"{"    list",-35}{HelpCommand.FormatLines(StringHelper.ScratchpadCommand_ListDescription, indentWidth: 35)}");
        commandsSb.Append($"{"    delete <Name>",-35}{HelpCommand.FormatLines(StringHelper.ScratchpadCommand_DeleteDescription, indentWidth: 35)}");
        commandsSb.Append($"{"    clear",-35}{HelpCommand.FormatLines(StringHelper.ScratchpadCommand_ClearDescription, indentWidth: 35)}");

        return new()
        {
            Description = StringHelper.ScratchpadCommand_Description,
            Usage = ["dc scratchpad [Command] [Options]"],
            Remarks = StringHelper.ScratchpadCommand_Remarks,
            Options =
            [
                ("Command", StringHelper.ScratchpadCommand_CommandOption),
                ("Options", StringHelper.ScratchpadCommand_OptionsOption)
            ],
            CustomSections =
            [
                (StringHelper.ScratchpadCommand_AvailableCommands, commandsSb.ToString())
            ],
            Examples =
            [
                ("dc scratchpad", StringHelper.ScratchpadCommand_Example1),
                ("dc scratchpad new --name=myScratch", StringHelper.ScratchpadCommand_Example2),
                ("dc scratchpad list", StringHelper.ScratchpadCommand_Example3),
                ("dc scratchpad clear", StringHelper.ScratchpadCommand_Example4)
            ]
        };
    }

    public override int Invoke(string[] args) => Scratchpad.HandleScratchpadCommands(args);
}
