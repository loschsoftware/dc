using Dassie.Configuration;
using Dassie.Configuration.Macros;
using Dassie.Configuration.ProjectGroups;
using Dassie.Extensions;
using Dassie.Helpers;
using Microsoft.VisualBasic.FileIO;
using DirectoryTarget = Dassie.Configuration.Build.Targets.Directory;

namespace Dassie.Cli.Commands;

internal class DeployCommand : ICompilerCommand
{
    private static DeployCommand _instance;
    public static DeployCommand Instance => _instance ??= new();

    public string Command => "deploy";

    public string UsageString => "deploy";

    public string Description => "Builds and deploys a project group.";

    public int Invoke(string[] args)
    {
        DassieConfig config = ProjectFileDeserializer.DassieConfig;
        config ??= new();

        MacroParser parser = new();
        parser.Normalize(config);

        ProjectGroup group = config.ProjectGroup;

        if (!File.Exists(ProjectConfigurationFileName))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0128_DeployCommandInvalidProjectGroupFile,
                $"Current directory contains no configuration file ({ProjectConfigurationFileName}).",
                ProjectConfigurationFileName);

            return -1;
        }

        if (group == null)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0128_DeployCommandInvalidProjectGroupFile,
                $"Invalid configuration file: '{ProjectConfigurationFileName}' does not define a project group.",
                ProjectConfigurationFileName);

            return -1;
        }

        if (group.Components == null || group.Components.Length == 0)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0129_ProjectGroupNoComponents,
                $"Project group contains no components.",
                ProjectConfigurationFileName);

            return -1;
        }

        string tempDir = Path.Combine(Directory.GetCurrentDirectory(), TemporaryBuildDirectoryName);
        Directory.CreateDirectory(tempDir);

        foreach (Component component in group.Components)
        {
            if (component is ProjectGroupComponent)
            {
                continue;
            }

            Project project = (Project)component;
            bool result = ReferenceHandler.HandleProjectReference(
                reference: new()
                {
                    CopyToOutput = true,
                    ProjectFile = project.Path
                },
                currentConfig: new() { References = [] },
                destDir: tempDir);

            if (!result)
                return -1;
        }

        MessagePrefix = "";

        if ((group.Targets ??= []).Length == 0)
        {
            EmitWarningMessage(
                0, 0, 0,
                DS0130_ProjectGroupNoTargets,
                "Project group defines no targets.",
                ProjectConfigurationFileName);
        }

        foreach (DeploymentTarget target in group.Targets)
        {
            // TODO: Add more targets and handle them elsewhere

            if (target is DirectoryTarget dir)
                FileSystem.CopyDirectory(tempDir, dir.Path, true);
        }

        Directory.Delete(tempDir, true);
        return 0;
    }
}