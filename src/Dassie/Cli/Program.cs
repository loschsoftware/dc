using Dassie.Cli.Commands;
using Dassie.Configuration;
using Dassie.Core;
using Dassie.Errors;
using Dassie.Errors.Devices;
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
                EmitErrorMessage(0, 0, 0, DS0001_UnknownError, $"An internal compiler error or limitation was encountered. Unhandled exception of type '{ex.GetType()}'.", CompilerExecutableName);

            ConsoleHelper.PrintException(ex, verbose);

            if (Debugger.IsAttached)
                throw;
        }

        if (Messages.Any(m => m.Severity == Severity.Error))
            exit = (int)Messages.First(m => m.Severity == Severity.Error).ErrorCode;

        Exit(exit);
        return exit;
    }

    /// <summary>
    /// Unloads all extensions and exits the application.
    /// </summary>
    /// <param name="errorCode">The application exit code.</param>
    public static void Exit(int errorCode)
    {
        ExtensionLoader.UnloadAll();
        TextWriterBuildLogDevice.InfoOut?.Dispose();
        TextWriterBuildLogDevice.WarnOut?.Dispose();
        TextWriterBuildLogDevice.ErrorOut?.Dispose();
        Environment.Exit(errorCode);
    }
}