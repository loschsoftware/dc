using Dassie.Cli.Commands;
using Dassie.Configuration;
using Dassie.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

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
            if (messages.Count == 0)
            {
                EmitErrorMessage(0, 0, 0, DS0000_UnexpectedError, $"Unhandled exception of type '{ex.GetType()}'.", "dc");
                Console.WriteLine();
            }

            if (Debugger.IsAttached)
                throw;

            if (File.Exists("dsconfig.xml"))
            {
                try
                {
                    XmlSerializer xmls = new(typeof(DassieConfig));
                    using StreamReader sr = new("dsconfig.xml");

                    DassieConfig config = (DassieConfig)xmls.Deserialize(sr);

                    if (config.PrintExceptionInfo)
                        Console.WriteLine(ex);
                }
                catch { }
            }

            return -1;
        }
    }
}