using Dassie.Configuration;
using Dassie.Extensions;
using System;
using System.IO;
using System.Linq;
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
        if (args.Length < 2)
        {
            System.Console.WriteLine("Specify an application type and name.");
            return -1;
        }

        if (ExtensionLoader.InstalledExtensions.Select(p => p.ProjectTemplates()).SelectMany(p => p).Any(t => t.Name == args[0]))
        {
            CreateStructure(ExtensionLoader.InstalledExtensions.Select(p => p.ProjectTemplates()).SelectMany(p => p).First(t => t.Name == args[0]));
            return 0;
        }

        string[] templates = ["console", "library"];
        if (!templates.Contains(args[0]))
        {
            System.Console.WriteLine($"The project template '{args[0]}' does not exist. Valid values are 'console' and 'library'.");
            return -1;
        }

        string rootDir = Path.Combine(Directory.GetCurrentDirectory(), args[1]);
        Directory.CreateDirectory(rootDir);

        string srcDir = Directory.CreateDirectory(Path.Combine(rootDir, "src")).FullName;
        string buildDir = Directory.CreateDirectory(Path.Combine(rootDir, "build")).FullName;

        FileStream configFile = File.Create(Path.Combine(rootDir, ProjectConfigurationFileName));

        XmlSerializerNamespaces ns = new();
        ns.Add("", "");

        XmlSerializer xmls = new(typeof(DassieConfig));
        DassieConfig config = new()
        {
            FormatVersion = DassieConfig.CurrentFormatVersion,
            AssemblyName = args[1]
        };

        switch (args[0])
        {
            case "console":
                AddSourceFile(Path.Combine(srcDir, "main.ds"), Console);
                break;

            case "library":
                AddSourceFile(Path.Combine(srcDir, "Type1.ds"), Library);
                config.ApplicationType = ApplicationType.Library;
                break;
        }

        xmls.Serialize(configFile, config, ns);

        System.Console.WriteLine($"Built new project in {rootDir} based on template '{args[1]}'.");

        return 0;
    }

    internal static void AddSourceFile(string path, string contents)
    {
        using StreamWriter sw = new(path);
        sw.WriteLine(contents);
    }

    internal static void CreateStructure(IProjectTemplate template)
    {
        throw new NotImplementedException();
    }
}