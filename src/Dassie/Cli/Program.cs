using Dassie.Cli.Commands;
using Dassie.Configuration;
using Dassie.Core;
using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            List<IPackage> extensions = ExtensionLoader.LoadInstalledExtensions();
            Dictionary<string, Func<string[], int>> customCommands = ExtensionLoader.GetAllCommands(extensions);
            Dictionary<string, string> commandDescriptions = ExtensionLoader.GetCommandDescriptions(extensions);

            List<ICompilerCommand> defaultCommands = DefaultCommandManager.DefaultCommands;
            HelpCommand helpCommand = new(commandDescriptions);
            defaultCommands.Add(helpCommand);

            args ??= [];
            if (args.Length == 0)
                return helpCommand.Invoke(args);

            string command = args[0];
            if (customCommands.TryGetValue(command, out Func<string[], int> cmd))
                return cmd(args[1..]);

            else if (defaultCommands.Any(c => c.Command == command || c.Aliases().Any(a => a == command)))
            {
                ICompilerCommand selectedCommand = defaultCommands.First(c => c.Command == command || c.Aliases().Any(a => a == command));
                return selectedCommand.Invoke(args);
            }

            string[] helpCommands = ["?", "-h", "--help", "-?", "/?", "/help"];
            if (helpCommands.Contains(command.ToLowerInvariant()))
                return helpCommand.Invoke([]);

            return CompileCommand.Compile(args);
        }
        catch (Exception ex)
        {
            if (messages.Count == 0)
            {
                EmitErrorMessage(0, 0, 0, DS0000_UnexpectedError, $"Unhandled exception of type '{ex.GetType()}'.", "dc.exe");
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