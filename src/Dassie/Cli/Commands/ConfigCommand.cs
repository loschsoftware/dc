using Dassie.Configuration;
using System.IO;
using System.Xml.Serialization;
using System;

namespace Dassie.Cli.Commands;

internal static partial class CliCommands
{
    public static int BuildDassieConfig()
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