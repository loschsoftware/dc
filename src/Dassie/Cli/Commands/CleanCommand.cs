﻿using Dassie.Configuration.Macros;
using Dassie.Configuration;
using Dassie.Extensions;
using Dassie.Configuration.ProjectGroups;

namespace Dassie.Cli.Commands;

internal class CleanCommand : ICompilerCommand
{
    private static CleanCommand _instance;
    public static CleanCommand Instance => _instance ??= new();

    public string Command => "clean";

    public string UsageString => "clean";

    public string Description => "Clears build artifacts and temporary files of a project or project group.";

    public int Invoke(string[] args)
    {
        if (!File.Exists(ProjectConfigurationFileName))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0128_DeployCommandInvalidProjectGroupFile,
                $"Current directory contains no configuration file ({ProjectConfigurationFileName}).",
                ProjectConfigurationFileName);

            return -1;
        }

        DassieConfig config = ProjectFileDeserializer.DassieConfig;
        config ??= new();

        if (config.ProjectGroup == null)
        {
            CleanProject(Directory.GetCurrentDirectory());
            return 0;
        }

        foreach (Component component in config.ProjectGroup.Components ?? [])
        {
            if (component is ProjectGroupComponent)
                continue;

            Project proj = (Project)component;
            proj.Path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), proj.Path));
            if (Directory.Exists(proj.Path))
                proj.Path = Path.Combine(proj.Path, ProjectConfigurationFileName);

            if (!File.Exists(proj.Path))
                continue;

            CleanProject(Path.GetDirectoryName(proj.Path));
        }

        return 0;
    }

    private static void CleanProject(string baseDir)
    {
        string workingDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(baseDir);
        ProjectFileDeserializer.Reload();

        DassieConfig config = ProjectFileDeserializer.DassieConfig;
        config ??= new();

        MacroParser parser = new();
        parser.ImportMacros(MacroGenerator.GenerateMacrosForProject(config));
        parser.Normalize(config);

        if (Directory.Exists(config.BuildOutputDirectory))
            Directory.Delete(config.BuildOutputDirectory, true);

        if (Directory.Exists(TemporaryBuildDirectoryName))
            Directory.Delete(TemporaryBuildDirectoryName, true);

        Directory.SetCurrentDirectory(workingDir);
    }
}