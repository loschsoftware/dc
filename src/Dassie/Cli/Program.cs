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

internal class Program
{
    [EntryPoint]
    internal static int Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = Encoding.Unicode;
            ToolPaths.GetOrCreateToolPathsFile();
            CommandRegistry.InitializeDefaults();

            args ??= [];
            if (args.Length == 0)
                return HelpCommand.Instance.Invoke(args);

            string command = args[0];
            if (CommandRegistry.TryInvoke(command, args[1..], out int ret))
                return ret;

            return CompileCommand.Instance.Invoke(args);
        }
        catch (Exception ex)
        {
            if (messages.Count(m => m.Severity == Severity.Error) == 0)
            {
                EmitErrorMessage(0, 0, 0, DS0000_UnexpectedError, $"Unhandled exception of type '{ex.GetType()}'.", "dc");
                Console.WriteLine();
            }

            if (Debugger.IsAttached)
                throw;

            try
            {
                if (ProjectFileDeserializer.DassieConfig.PrintExceptionInfo)
                    Console.WriteLine(ex);
            }
            catch { }
        }
        finally
        {
            foreach (IBuildLogDevice device in BuildLogDevices)
            {
                if (device is IDisposable d)
                    d.Dispose();
            }
        }

        return -1;
    }
}