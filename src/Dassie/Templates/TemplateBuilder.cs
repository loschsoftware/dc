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

    private static void PrintAvailableTemplates()
    {
        int templateNameWidth = ExtensionLoader.ProjectTemplates.Select(t => t.Name).Append(StringHelper.TemplateBuilder_TableHeaderTemplateName).Select(n => n.Length).Max();
        WriteLine(StringHelper.Format(nameof(StringHelper.TemplateBuilder_AvailableTemplatesHeader), Environment.NewLine));

        string tableHeader = $"{StringHelper.TemplateBuilder_TableHeaderTemplateName.PadRight(templateNameWidth)}\t\t{StringHelper.TemplateBuilder_TableHeaderPackage}";
        int headerWidth = tableHeader.Length + ExtensionLoader.InstalledExtensions.Where(t => t.ProjectTemplates()?.Length > 0).Select(p => p.Metadata.Name).Append(StringHelper.TemplateBuilder_Preinstalled).Select(p => p.Length).Max();

        WriteLine(tableHeader);
        WriteLine(new('-', headerWidth));

        foreach (IProjectTemplate template in ExtensionLoader.ProjectTemplates.OrderBy(t => t.Name))
        {
            string packageName = "";

            if (ExtensionLoader.InstalledExtensions.Any(p => p.ProjectTemplates() != null && p.ProjectTemplates().Any(t => t.GetType() == template.GetType())))
                packageName = ExtensionLoader.InstalledExtensions.First(p => p.ProjectTemplates() != null && p.ProjectTemplates().Any(t => t.GetType() == template.GetType())).Metadata.Name;

            if (packageName == "Core")
                packageName = StringHelper.TemplateBuilder_Preinstalled;

            WriteLine($"{template.Name.PadRight(templateNameWidth)}\t\t{packageName}");
        }
    }

    public static int CreateStructure(string[] args)
    {
        if (args.Any(a => a == "--list-templates"))
        {
            PrintAvailableTemplates();
            return 0;
        }

        IEnumerable<IProjectTemplate> availableTemplates = ExtensionLoader.ProjectTemplates;

        if (args.Length < 2)
        {
            EmitErrorMessageFormatted(0, 0, 0, DS0206_DCNewInvalidArguments, nameof(StringHelper.TemplateBuilder_TemplateAndNameExpected), [], CompilerExecutableName);
            return -1;
        }

        if (!availableTemplates.Any(t => string.Compare(t.Name, args[0], !t.IsCaseSensitive()) == 0))
        {
            EmitErrorMessageFormatted(0, 0, 0, DS0206_DCNewInvalidArguments, nameof(StringHelper.TemplateBuilder_TemplateNotInstalled), [args[0]], CompilerExecutableName);
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
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0207_DCNewNonEmptyDirectory,
                    nameof(StringHelper.TemplateBuilder_DirectoryNotEmpty), [rootDir],
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
            ["ProjectName"] = args[1],
            ["ProjectDir"] = rootDir
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

        WriteLine(StringHelper.Format(nameof(StringHelper.TemplateBuilder_ProjectBuilt), rootDir, args[0]));
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