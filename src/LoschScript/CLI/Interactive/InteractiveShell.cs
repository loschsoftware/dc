using LoschScript.CodeGeneration;
using System;
using System.Diagnostics;
using System.Text;
using static LoschScript.Core.stdin;
using static LoschScript.Core.stdout;

namespace LoschScript.CLI.Interactive;

internal static class InteractiveShell
{
    private static string tmpFile;

    public static int Start()
    {
        tmpFile = Path.GetTempFileName();

        Console.Clear();
        println("LoschScript Interactive Shell");
        println("Type 'exit' to exit the shell or 'clear' to clear the history.");
        println();

        while (true)
        {
            print(">>> ");

            string input = rdstr();

            if (input == "exit")
                break;

            if (input == "clear")
            {
                Console.Clear();
                println("LoschScript Interactive Shell");
                println("Type 'exit' to exit the shell or 'clear' to clear the history.");
                println();

                continue;
            }

            AppendAndExecute(input);
        }

        return 0;
    }

    private static void AppendAndExecute(string input)
    {
        File.AppendAllText(tmpFile, $"{input}{Environment.NewLine}");

        Helpers.HandleArgs(new string[] { tmpFile });
        Process.Start(Path.ChangeExtension(Path.GetFileName(tmpFile), ".exe"));
    }
}