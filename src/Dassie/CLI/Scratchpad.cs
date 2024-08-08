using Dassie.Configuration;
using Dassie.Errors;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace Dassie.CLI;

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
                DS0098_ScratchNotFound,
                $"The scratch '{name}' could not be found.",
                "dc");

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
                DS0098_ScratchNotFound,
                $"The scratch '{name}' could not be found.",
                "dc");

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
        Program.DisplayLogo();
        Console.WriteLine();
        Console.WriteLine("To mark the end of the input, press Ctrl+Z in an empty line and hit Enter.");
        Console.WriteLine();

        string src = Console.In.ReadToEnd();
        Console.WriteLine();

        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "Dassie", "Scratchpad");
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

        DassieConfig cfg = null;
        if (File.Exists("dsconfig.xml"))
        {
            using StreamReader sr = new("dsconfig.xml");
            XmlSerializer xmls = new(typeof(DassieConfig));
            cfg = (DassieConfig)xmls.Deserialize(sr);
        }

        int result = CliHelpers.HandleArgs([file]);

        string asm = Path.ChangeExtension(file, "dll");
        string outDir = dir;

        if (cfg != null)
        {
            if (!string.IsNullOrEmpty(cfg.BuildOutputDirectory))
            {
                if (Directory.Exists(cfg.BuildOutputDirectory))
                    outDir = cfg.BuildOutputDirectory;

                else // relative path
                    outDir = Path.Combine(outDir, cfg.BuildOutputDirectory);

                asm = Path.Combine(outDir, Path.ChangeExtension(file, "dll"));
            }

            if (!string.IsNullOrEmpty(cfg.AssemblyName))
                asm = Path.Combine(outDir, $"{cfg.AssemblyName}.dll");
        }

        if (File.Exists(asm) && !messages.Any(e => e.Severity == Severity.Error))
        {
            ProcessStartInfo psi = new()
            {
                FileName = "cmd.exe",
                Arguments = $"/c dotnet {asm}",
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
        Program.DisplayLogo();
        Console.WriteLine();
        Console.WriteLine("Usage: dc scratchpad [Command] [Options]");
        Console.WriteLine("If no command is specified, the command 'new' is used implicitly.");

        Console.WriteLine();
        Console.WriteLine("Available commands:");
        Console.WriteLine($"{"    new [Options]",-35}Creates a new scratch.");
        Console.WriteLine($"{"        --name=<Name>",-35}Specifies the name of the scratch.");
        Console.WriteLine($"{"        --config=<Path>",-35}The compiler configuration (dsconfig.xml) file to use.");
        Console.WriteLine();

        Console.WriteLine($"{"    load <Name>",-35}Loads the specified scratch.");
        Console.WriteLine($"{"    list",-35}Lists all saved scratches.");
        Console.WriteLine($"{"    delete <Name>",-35}Deletes the specified scratch.");
        Console.WriteLine($"{"    clear",-35}Deletes all saved scratches.");
        Console.WriteLine($"{"    help",-35}Shows this list.");

        return 0;
    }
}