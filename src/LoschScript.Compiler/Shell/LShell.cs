using System.IO;
using System;
using Losch.LoschScript.Configuration;
using System.Diagnostics;
using LoschScript.CLI;
using System.Xml.Serialization;

namespace LoschScript.Shell;

internal static class LShell
{
    public static void Start()
    {
        // Initialize default ShellContext
        ShellContext _ = new();
        ShellContext.Current.CurrentDirectory = Directory.GetCurrentDirectory();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.WriteLine("LoschScript Interactive Shell");
        Console.WriteLine($"Version 1.0, {DateTime.Now.Year}");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;

        while (true)
        {
            Prompt();
        }
    }

    private static void Prompt()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("LS ");
        Console.ForegroundColor = ConsoleColor.Gray;

        Console.Write(ShellContext.FormatPrompt(ShellContext.Current.PromptFormat));
        string input = Console.ReadLine();

        string program = @$"import LoschScript.Shell.Core
import type LoschScript.Shell.Core.UI
import type LoschScript.Shell.Core.IO
import System

Console.WriteLine {input}
";

        string cd = Directory.GetCurrentDirectory();
        string tempDir = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "LShell", $"{DateTime.Now.Ticks}")).FullName;
        string fn = Path.Combine(tempDir, $"{DateTime.Now.Ticks}");

        LSConfig lsconfig = new()
        {
            AssemblyName = "temp"
        };

        Directory.SetCurrentDirectory(tempDir);

        File.WriteAllText(fn, program);

        XmlSerializer xmls = new(typeof(LSConfig));
        using StreamWriter sw = new("lsconfig.xml");
        xmls.Serialize(sw, lsconfig);
        sw.Dispose();

        Helpers.HandleArgs(new string[] { fn });

        Process.Start("temp.exe");

        Directory.SetCurrentDirectory(cd);
    }
}