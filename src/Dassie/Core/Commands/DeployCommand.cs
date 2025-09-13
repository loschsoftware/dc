using Dassie.Configuration;
using Dassie.Configuration.Macros;
using Dassie.Configuration.ProjectGroups;
using Dassie.Extensions;
using System.Linq;
using System.Xml;

namespace Dassie.Core.Commands;

internal class DeployCommand : CompilerCommand
{
    private static DeployCommand _instance;
    public static DeployCommand Instance => _instance ??= new();

    public override string Command => "deploy";

    public override string Description => "Builds and deploys a project group.";

    public override CommandHelpDetails HelpDetails => new()
    {
        Description = Description,
        Usage = ["dc deploy [--ignore-missing] [--fail-fast] [Options]"],
        Remarks = $"This is the primary command for interacting with project groups. The 'deploy' command first builds all component projects and then executes all targets defined in the project group file. A project group is defined using the <ProjectGroup> tag inside of a compiler configuration file ({ProjectConfigurationFileName}).",
        Options =
        [
            ("--ignore-missing", "Ignore missing targets and resume deployment."),
            ("--fail-fast", "Cancel deployment immediately if any target fails."),
            ("Options", "Additional options passed to the compiler for each project being built.")
        ],
        Examples =
        [
            ("dc deploy", "Builds and deploys the project group defined in the current directory."),
            ("dc deploy --ignore-missing", "Builds and deploys the project group, ignoring any missing targets."),
            ("dc deploy --fail-fast", "Builds and deploys the project group, stopping immediately if any target fails."),
            ("dc deploy -l", "Builds and deploys the project group, passing the '-l' flag to the compiler for each project being built.")
        ]
    };

    public override int Invoke(string[] args) => Deploy(args, Directory.GetCurrentDirectory(), false);

    private static int Deploy(string[] args, string baseDir, bool noDeleteTempDirectory)
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
                DS0129_DeployCommandInvalidProjectGroupFile,
                $"Current directory contains no configuration file ({ProjectConfigurationFileName}).",
                ProjectConfigurationFileName);

            return -1;
        }

        if (group == null)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0129_DeployCommandInvalidProjectGroupFile,
                $"Invalid configuration file: '{ProjectConfigurationFileName}' does not define a project group.",
                ProjectConfigurationFileName);

            return -1;
        }

        if (group.Components == null || group.Components.Length == 0)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0130_ProjectGroupNoComponents,
                $"Project group contains no components.",
                ProjectConfigurationFileName);

            return -1;
        }

        bool ignoreMissing = args.Contains("--ignore-missing");
        bool failFast = args.Contains("--fail-fast");

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
                        DS0129_DeployCommandInvalidProjectGroupFile,
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
                tempDir,
                args: args.Except(["--ignore-missing", "--fail-fast"]).ToArray(),
                track: false);

            if (!result)
                return -1;
        }

        MessagePrefix = "";

        if (((group.Targets ??= new()).Targets ??= []).Length == 0)
        {
            EmitWarningMessage(
                0, 0, 0,
                DS0131_ProjectGroupNoTargets,
                "Project group defines no targets.",
                ProjectConfigurationFileName);
        }

        foreach (XmlNode target in group.Targets.Targets)
        {
            if (!ExtensionLoader.DeploymentTargets.Any(t => t.Name == target.Name))
            {
                if (ignoreMissing)
                    continue;

                EmitErrorMessage(
                    0, 0, 0,
                    DS0237_DeploymentTargetNotFound,
                    $"The deployment target '{target.Name}' could not be found.",
                    CompilerExecutableName);

                continue;
            }

            (int exit, string path) = ProjectGroupHelpers.GetExecutableProject(config, false);
            if (exit != 0)
                return exit;

            DassieConfig executableProj = ProjectFileDeserializer.Deserialize(path);
            XmlAttribute[] attribs = new XmlAttribute[target.Attributes.Count];
            target.Attributes.CopyTo(attribs, 0);

            int ret = ExtensionLoader.DeploymentTargets.First(t => t.Name == target.Name).Execute(new(
                tempDir,
                config,
                executableProj,
                attribs.ToList(),
                target.ChildNodes.Cast<XmlNode>().ToList()));

            if (ret != 0)
            {
                string msg = $"Deployment target '{target.Name}' ended with a nonzero exit code.";

                if (failFast)
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0238_DeploymentTargetFailed,
                        msg,
                        CompilerExecutableName);

                    return 238;
                }

                EmitWarningMessage(
                    0, 0, 0,
                    DS0238_DeploymentTargetFailed,
                    msg,
                    CompilerExecutableName);
            }
        }

        if (!noDeleteTempDirectory)
            Directory.Delete(tempDir, true);

        return 0;
    }
}