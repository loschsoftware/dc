using Dassie.Configuration;
using Dassie.Configuration.Macros;
using Dassie.Extensions;
using System;
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
    private static void RecursiveDelete(DirectoryInfo baseDir)
    {
        if (!baseDir.Exists)
            return;

        foreach (var dir in baseDir.EnumerateDirectories())
            RecursiveDelete(dir);

        baseDir.Delete(true);
    }

    public static int CreateStructure(string[] args)
    {
        IEnumerable<IProjectTemplate> availableTemplates = ExtensionLoader.ProjectTemplates;

        void PrintAvailableTemplates()
        {
            int templateNameWidth = availableTemplates.Select(t => t.Name).Append("Template Name").Select(n => n.Length).Max();
            WriteLine($"The following project templates are available:{Environment.NewLine}");

            string tableHeader = $"{"Template Name".PadRight(templateNameWidth)}\t\tPackage";
            int headerWidth = tableHeader.Length + ExtensionLoader.InstalledExtensions.Where(t => t.ProjectTemplates()?.Length > 0).Select(p => p.Metadata.Name).Append("Preinstalled").Select(p => p.Length).Max();

            WriteLine(tableHeader);
            WriteLine(new('-', headerWidth));

            foreach (IProjectTemplate template in availableTemplates.OrderBy(t => t.Name))
            {
                string packageName;

                if (ExtensionLoader.InstalledExtensions.Any(p => p.ProjectTemplates() != null && p.ProjectTemplates().Any(t => t.GetType() == template.GetType())))
                    packageName = ExtensionLoader.InstalledExtensions.First(p => p.ProjectTemplates() != null && p.ProjectTemplates().Any(t => t.GetType() == template.GetType())).Metadata.Name;
                else
                    packageName = "Preinstalled";

                WriteLine($"{template.Name.PadRight(templateNameWidth)}\t\t{packageName}");
            }
        }

        if (args.Length < 2)
        {
            EmitErrorMessage(0, 0, 0, DS0205_DCNewInvalidArguments, "Project template and name expected.", CompilerExecutableName);
            WriteLine("");
            PrintAvailableTemplates();
            return -1;
        }

        if (!availableTemplates.Any(t => string.Compare(t.Name, args[0], !t.IsCaseSensitive()) == 0))
        {
            EmitErrorMessage(0, 0, 0, DS0205_DCNewInvalidArguments, $"The project template '{args[0]}' is not installed.", CompilerExecutableName);
            WriteLine("");
            PrintAvailableTemplates();
            return -1;
        }

        IProjectTemplate selectedTemplate = availableTemplates.First(t => string.Compare(t.Name, args[0], !t.IsCaseSensitive()) == 0);

        string rootDir = Path.Combine(Directory.GetCurrentDirectory(), args[1]);

        if (Directory.Exists(rootDir) && Directory.GetFileSystemEntries(rootDir).Length > 0)
        {
            if (args.Any(a => a == "-f" || a == "--force"))
                RecursiveDelete(new(rootDir));
            else
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0206_DCNewNonEmptyDirectory,
                    $"The project directory '{rootDir}' already exists and is not empty. Use the '-f' flag to delete the existing directory.",
                    CompilerExecutableName);

                return -1;
            }
        }

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