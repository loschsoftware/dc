using Dassie.Cli.Commands;
using Dassie.Configuration;
using Dassie.Core;
using Dassie.Errors;
using Dassie.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Dassie.Cli;

/// <summary>
/// Acts as a container for the application entry point.
/// </summary>
internal class Program
{
    /// <summary>
    /// The compiler entry point.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the compiler.</param>
    /// <returns>The exit code of the process.</returns>
    [EntryPoint]
    internal static int Main(string[] args)
    {
        int exit = -1;

        try
        {
            Console.OutputEncoding = Encoding.Unicode;
            ToolPaths.GetOrCreateToolPathsFile();

            args ??= [];
            if (args.Length == 0)
                exit = HelpCommand.Instance.Invoke(args);
            else
            {
                string command = args[0];
                if (CommandHandler.TryInvoke(command, args[1..], out int ret))
                    exit = ret;
                else
                    exit = CompileCommand.Instance.Invoke(args);
            }
        }
        catch (Exception ex)
        {
            bool verbose = EmitBuildLogMessage($"Unhandled exception occured. {ex}", 2);

            if (!Messages.Any(m => m.Severity == Severity.Error))
                EmitErrorMessage(0, 0, 0, DS0000_UnknownError, $"Unhandled exception of type '{ex.GetType()}'.", CompilerExecutableName);

            ConsoleHelper.PrintException(ex, verbose);

            if (Debugger.IsAttached)
                throw;
        }

        Exit(exit);
        return exit;
    }

    /// <summary>
    /// Unloads all extensions and exits the application.
    /// </summary>
    /// <param name="errorCode">The application exit code.</param>
    public static void Exit(int errorCode)
    {
        if (errorCode == (int)DS0233_CompilationTerminated)
        {
            int msgCount = Messages.Count(e => e.Severity == Severity.Error);
            Context.Configuration.MaxErrors = 0;

            EmitMessage(
                0, 0, 0,
                DS0233_CompilationTerminated,
                $"Compilation terminated after {msgCount} error{(msgCount > 1 ? "s" : "")}.",
                CompilerExecutableName);
        }

        ExtensionLoader.UnloadAll();
        Environment.Exit(errorCode);
    }
}