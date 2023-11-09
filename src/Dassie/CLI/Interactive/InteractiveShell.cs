using Dassie.CodeGeneration;
using System;
using System.Diagnostics;
using System.Text;
using static Dassie.Core.stdin;
using static Dassie.Core.stdout;

namespace Dassie.CLI.Interactive;

internal static class InteractiveShell
{
    private static string tmpFile;

    public static int Start()
    {
        tmpFile = Path.GetFileName(Path.GetTempFileName());

        Console.Clear();
        println("Dassie Interactive Shell");
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
                println("Dassie Interactive Shell");
                println("Type 'exit' to exit the shell or 'clear' to clear the history.");
                println();

                continue;
            }

            AppendAndExecute(input);
        }

        File.Delete(tmpFile);

        if (File.Exists(Path.ChangeExtension(tmpFile, ".exe")))
            File.Delete(Path.ChangeExtension(tmpFile, ".exe"));

        return 0;
    }

    private static void AppendAndExecute(string input)
    {
        File.AppendAllText(tmpFile, $"{input}{Environment.NewLine}");

        CliHelpers.HandleArgs(new string[] { tmpFile });
        Process.Start(Path.ChangeExtension(tmpFile, ".exe"));
    }
}