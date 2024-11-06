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

    public int Invoke(string[] args)
    {
        if (File.Exists("dsconfig.xml"))
        {
            LogOut.Write("The file dsconfig.xml already exists. Overwrite [Y/N]? ");
            string input = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine();

            if (!input.Equals("y", StringComparison.OrdinalIgnoreCase))
                return -1;
        }

        using StreamWriter configWriter = new("dsconfig.xml");

        XmlSerializerNamespaces ns = new();
        ns.Add("", "");

        XmlSerializer xmls = new(typeof(DassieConfig));
        xmls.Serialize(configWriter, new DassieConfig(), ns);

        LogOut.WriteLine("Created dsconfig.xml using default values.");
        return 0;
    }
}