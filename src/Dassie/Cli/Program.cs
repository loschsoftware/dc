using Dassie.Cli.Commands;
using Dassie.Configuration.Global;
using Dassie.Core;
using Dassie.Core.Commands;
using Dassie.Extensions;
using Dassie.Messages;
using Dassie.Messages.Devices;
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
            ExtensionLoader.Initialize();
            GlobalConfigManager.Initialize();
            StringHelper.Initialize();

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
            bool verbose = EmitBuildLogMessageFormatted(nameof(StringHelper.Program_UnhandledExceptionBuildLogMessage), [ex], 2);

            if (!EmittedMessages.Any(m => m.Severity == Severity.Error))
                EmitErrorMessageFormatted(0, 0, 0, DS0001_UnknownError, nameof(StringHelper.Program_UnhandledExceptionError), [ex.GetType()], CompilerExecutableName);

            ConsoleHelper.PrintException(ex, verbose);

            if (Debugger.IsAttached)
                throw;
        }

        if (EmittedMessages.Any(m => m.Severity == Severity.Error))
            exit = (int)EmittedMessages.First(m => m.Severity == Severity.Error).Code;

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