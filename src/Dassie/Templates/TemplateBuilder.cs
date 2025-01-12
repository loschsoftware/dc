using Dassie.Configuration;
using Dassie.Configuration.Macros;
using Dassie.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Dassie.Templates;

/// <summary>
/// Provides mechanisms to create Dassie projects based on a project template.
/// </summary>
internal static class TemplateBuilder
{
    public static int CreateStructure(string[] args)
    {
        if (args.Length < 2)
        {
            WriteLine("Specify a project template and name.");
            return -1;
        }

        IEnumerable<IProjectTemplate> availableTemplates = ExtensionLoader.ProjectTemplates;

        if (!availableTemplates.Any(t => string.Compare(t.Name, args[0], !t.IsCaseSensitive()) == 0))
        {
            WriteLine($"The project template '{args[0]}' is not installed. Installed templates are: {string.Join(", ", availableTemplates.Select(t => $"'{t.Name}'"))}.");
            return -1;
        }

        IProjectTemplate selectedTemplate = availableTemplates.First(t => string.Compare(t.Name, args[0], !t.IsCaseSensitive()) == 0);

        string rootDir = Path.Combine(Directory.GetCurrentDirectory(), args[1]);
        Directory.CreateDirectory(rootDir);

        DassieConfig config = null;

        if (selectedTemplate.Entries.Any(t => t is ProjectFile))
            config = (selectedTemplate.Entries.First(t => t is ProjectFile) as ProjectFile).Content;

        MacroParser parser = new(true);
        parser.ImportMacros(new()
        {
            ["projectname"] = args[1],
            ["projectdir"] = rootDir
        });

        parser.Normalize(config);

        XmlSerializerNamespaces ns = new();
        ns.Add("", "");

        foreach (ProjectTemplateEntry entry in selectedTemplate.Entries ?? [])
        {
            if (entry is ProjectFile p)
            {
                DassieConfig cfg = p.Content ?? new();
                parser.Normalize(cfg);

                using StreamWriter sw = new(Path.Combine(rootDir, ProjectConfigurationFileName));
                XmlSerializer xmls = new(typeof(DassieConfig));
                xmls.Serialize(sw, cfg, ns);
                continue;
            }

            if (entry is ProjectTemplateFile f)
            {
                using StreamWriter sw = new(Path.Combine(rootDir, f.Name));
                sw.Write(parser.Normalize(f.FormattedContent ?? ""));
                continue;
            }

            ProjectTemplateDirectory dir = entry as ProjectTemplateDirectory;
            string subDir = Directory.CreateDirectory(Path.Combine(rootDir, dir.Name)).FullName;
            BuildDirectoryStructure(dir, subDir, parser);
        }

        WriteLine($"Built new project in {rootDir} based on template '{args[0]}'.");
        return 0;
    }

    private static void BuildDirectoryStructure(ProjectTemplateDirectory dir, string baseDir, MacroParser parser)
    {
        XmlSerializerNamespaces ns = new();
        ns.Add("", "");

        foreach (ProjectTemplateEntry child in dir.Children ?? [])
        {
            if (child is ProjectFile p)
            {
                DassieConfig cfg = p.Content ?? new();
                parser.Normalize(cfg);

                using StreamWriter sw = new(Path.Combine(baseDir, ProjectConfigurationFileName));
                XmlSerializer xmls = new(typeof(DassieConfig));
                xmls.Serialize(sw, cfg, ns);
                continue;
            }

            if (child is ProjectTemplateFile f)
            {
                using StreamWriter sw = new(Path.Combine(baseDir, f.Name));
                sw.Write(parser.Normalize(f.FormattedContent ?? ""));
                continue;
            }

            ProjectTemplateDirectory sub = child as ProjectTemplateDirectory;
            string newDir = Directory.CreateDirectory(Path.Combine(baseDir, sub.Name)).FullName;
            BuildDirectoryStructure(sub, newDir, parser);
        }
    }
}