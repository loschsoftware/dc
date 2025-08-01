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

    public CommandHelpDetails HelpDetails() => new()
    {
        Description = Description,
        Usage = ["dc deploy"],
        Remarks = $"This is the primary command for interacting with project groups. The 'deploy' command first builds all component projects and then executes all targets defined in the project group file. A project group is defined using the <ProjectGroup> tag inside of a compiler configuration file ({ProjectConfigurationFileName}).",
        Options = []
    };

    public int Invoke(string[] args) => Deploy(args, Directory.GetCurrentDirectory(), false);

    private static int Deploy(string[] args, string baseDir, bool noDeleteTempDirectory)
    {
        if (args.Length > 0)
        {
            foreach (string arg in args)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0211_UnexpectedArgument,
                    $"Unexpected argument '{arg}'.",
                    CompilerExecutableName);
            }

            return -1;
        }

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

        string tempDir = Path.Combine(baseDir, TemporaryBuildDirectoryName);
        Directory.CreateDirectory(tempDir);

        foreach (Component component in group.Components)
        {
            if (component is ProjectGroupComponent pg)
            {
                string workingDir = Directory.GetCurrentDirectory();
                pg.Path = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFullPath(pg.Path));

                if (File.Exists(pg.Path))
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(pg.Path));

                else if (Directory.Exists(pg.Path))
                    Directory.SetCurrentDirectory(pg.Path);

                else
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0128_DeployCommandInvalidProjectGroupFile,
                        $"Component project group '{pg.Path}' could not be found.",
                        ProjectConfigurationFileName);

                    return -1;
                }

                Deploy(args, baseDir, true);
                Directory.SetCurrentDirectory(workingDir);
                continue;
            }

            Project project = (Project)component;
            ProjectReference reference = new()
            {
                CopyToOutput = true,
                ProjectFile = project.Path
            };

            DassieConfig projectConfig = new() { References = [] };

            bool result = ReferenceHandler.HandleProjectReference(
                reference,
                projectConfig,
                tempDir);

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

        if (!noDeleteTempDirectory)
            Directory.Delete(tempDir, true);

        return 0;
    }
}