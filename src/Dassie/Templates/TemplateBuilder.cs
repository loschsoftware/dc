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
        string prevWorkingDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(rootDir);
        DassieConfig config = null;

        if (selectedTemplate.Entries.Any(t => t is ProjectFile))
            config = (selectedTemplate.Entries.First(t => t is ProjectFile) as ProjectFile).Content;

        SerializeContents(selectedTemplate.Entries ?? [], rootDir, config);
        WriteLine(StringHelper.Format(nameof(StringHelper.TemplateBuilder_ProjectBuilt), rootDir, args[0]));
        Directory.SetCurrentDirectory(prevWorkingDir);
        return 0;
    }

    private static void SerializeContents(IEnumerable<ProjectTemplateEntry> entries, string baseDir, DassieConfig config)
    {
        foreach (ProjectTemplateEntry entry in entries)
        {
            if (entry is ProjectFile p)
            {
                DassieConfig cfg = p.Content ?? DassieConfig.Default;
                ProjectFileSerializer.Serialize(cfg, Path.Combine(baseDir, ProjectConfigurationFileName));
                continue;
            }

            if (entry is ProjectTemplateFile f)
            {
                using StreamWriter sw = new(Path.Combine(baseDir, f.Name));
                MacroParser mp = new(config);
                sw.Write(mp.Expand(f.FormattedContent).Value);
                continue;
            }

            ProjectTemplateDirectory dir = entry as ProjectTemplateDirectory;
            string subDir = Directory.CreateDirectory(Path.Combine(baseDir, dir.Name)).FullName;
            SerializeContents(dir.Children ?? [], subDir, config);
        }
    }
}