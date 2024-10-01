using Dassie.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Dassie.Templates;

/// <summary>
/// Provides templates for commonly used Dassie project types.
/// </summary>
public static class DSTemplates
{
    /// <summary>
    /// Contains a template for a console application.
    /// </summary>
    public static readonly string Console =

@"println ""Hello World!""";

    /// <summary>
    /// Contains a template for a library application.
    /// </summary>
    public static readonly string Library =
@"type Type1 = {
}";

    internal static int CreateStructure(string[] args)
    {
        if (args.Length < 3)
        {
            System.Console.WriteLine("Specify an application type and name.");
            return -1;
        }

        string rootDir = Path.Combine(Directory.GetCurrentDirectory(), args[2]);
        Directory.CreateDirectory(rootDir);

        string srcDir = Directory.CreateDirectory(Path.Combine(rootDir, "src")).FullName;
        string buildDir = Directory.CreateDirectory(Path.Combine(rootDir, "build")).FullName;

        FileStream configFile = File.Create(Path.Combine(rootDir, "dsconfig.xml"));

        XmlSerializerNamespaces ns = new();
        ns.Add("", "");

        XmlSerializer xmls = new(typeof(DassieConfig));
        DassieConfig config = new()
        {
            AssemblyName = args[2]
        };

        switch (args[1])
        {
            case "console":
                {
                    AddSourceFile(Path.Combine(srcDir, "main.ds"), Console);
                    xmls.Serialize(configFile, config, ns);
                    break;
                }
        }

        System.Console.WriteLine($"Built new project in {rootDir} based on template '{args[1]}'.");

        return 0;
    }

    internal static void AddSourceFile(string path, string contents)
    {
        using StreamWriter sw = new(path);
        sw.WriteLine(contents);
    }
}