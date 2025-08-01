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

internal class Program
{
    [EntryPoint]
    internal static int Main(string[] args)
    {
        int exit = -1;

        try
        {
            Console.OutputEncoding = Encoding.Unicode;
            ToolPaths.GetOrCreateToolPathsFile();
            CommandRegistry.InitializeDefaults();

            args ??= [];
            if (args.Length == 0)
                exit = HelpCommand.Instance.Invoke(args);
            else
            {
                string command = args[0];
                if (CommandRegistry.TryInvoke(command, args[1..], out int ret))
                    exit = ret;
                else 
                    exit = CompileCommand.Instance.Invoke(args);
            }
        }
        catch (Exception ex)
        {
            bool verbose = EmitBuildLogMessage($"Unhandled exception occured. {ex}", 2);

            if (!Messages.Any(m => m.Severity == Severity.Error))
                EmitErrorMessage(0, 0, 0, DS0000_UnknownError, $"Unhandled exception of type '{ex.GetType()}'.", "dc");

#if PRINT_EXCEPTION_INFO
            TextWriterBuildLogDevice.ErrorOut.WriteLine(ex);
#else
            if (!verbose)
            {
                try
                {
                    if (ProjectFileDeserializer.DassieConfig.PrintExceptionInfo)
                        TextWriterBuildLogDevice.ErrorOut.WriteLine(ex);
                }
                catch { }
            }
#endif

            if (Debugger.IsAttached)
                throw;
        }

        ExtensionLoader.UnloadAll();
        return exit;
    }
}