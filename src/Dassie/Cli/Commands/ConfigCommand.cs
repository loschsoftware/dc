using Dassie.Configuration;
using Dassie.Extensions;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Dassie.Cli.Commands;

internal class ConfigCommand : ICompilerCommand
{
    private static ConfigCommand _instance;
    public static ConfigCommand Instance => _instance ??= new();

    public string Command => "config";

    public string UsageString => "config";

    public string Description => "Creates a new dsconfig.xml file with default values.";

    public CommandHelpDetails HelpDetails() => new()
    {
        Description = Description,
        Usage = ["dc config"],
        Remarks = "This command provides an alternative way of initializing a project to the 'dc new' command. It is primarily useful when creating a project out of an existing file structure of source files.",
    };

    public int Invoke(string[] args)
    {
        if (args.Length > 0)
        {
            foreach (string arg in args)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0212_UnexpectedArgument,
                    $"Unexpected argument '{arg}'.",
                    CompilerExecutableName);
            }

            return -1;
        }

        if (File.Exists(ProjectConfigurationFileName))
        {
            LogOut.Write("The file dsconfig.xml already exists. Overwrite [Y/N]? ");
            string input = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine();

            if (!input.Equals("y", StringComparison.OrdinalIgnoreCase))
                return -1;
        }

        using StreamWriter configWriter = new(ProjectConfigurationFileName);

        XmlSerializerNamespaces ns = new();
        ns.Add("", "");

        XmlSerializer xmls = new(typeof(DassieConfig));
        xmls.Serialize(configWriter, new DassieConfig(), ns);

        LogOut.WriteLine("Created dsconfig.xml using default values.");
        return 0;
    }
}