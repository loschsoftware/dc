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

    public override string Description => StringHelper.DeployCommand_Description;

    public override CommandHelpDetails HelpDetails => new()
    {
        Description = Description,
        Usage = ["dc deploy [--ignore-missing] [--fail-fast] [Options]"],
        Remarks = StringHelper.Format(nameof(StringHelper.DeployCommand_Remarks), ProjectConfigurationFileName),
        Options =
        [
            ("--ignore-missing", StringHelper.DeployCommand_IgnoreMissingOption),
            ("--fail-fast", StringHelper.DeployCommand_FailFastOption),
            ("Options", StringHelper.DeployCommand_OptionsOption)
        ],
        Examples =
        [
            ("dc deploy", StringHelper.DeployCommand_Example1),
            ("dc deploy --ignore-missing", StringHelper.DeployCommand_Example2),
            ("dc deploy --fail-fast", StringHelper.DeployCommand_Example3),
            ("dc deploy -l", StringHelper.DeployCommand_Example4)
        ]
    };

    public override int Invoke(string[] args) => Deploy(args, Directory.GetCurrentDirectory(), false);

    private static int Deploy(string[] args, string baseDir, bool noDeleteTempDirectory)
    {
        DassieConfig config = ProjectFileDeserializer.DassieConfig;
        ProjectGroup group = config.ProjectGroup;

        if (!File.Exists(ProjectConfigurationFileName))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0129_DeployCommandInvalidProjectGroupFile,
                nameof(StringHelper.DeployCommand_NoConfigurationFileInCurrentDirectory), [ProjectConfigurationFileName],
                ProjectConfigurationFileName);

            return -1;
        }

        if (group == null)
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0129_DeployCommandInvalidProjectGroupFile,
                nameof(StringHelper.DeployCommand_ConfigurationFileDefinesNoProjectGroup), [ProjectConfigurationFileName],
                ProjectConfigurationFileName);

            return -1;
        }

        if (group.Components == null || group.Components.Length == 0)
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0130_ProjectGroupNoComponents,
                nameof(StringHelper.DeployCommand_NoComponents), [],
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
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0129_DeployCommandInvalidProjectGroupFile,
                        nameof(StringHelper.DeployCommand_ComponentProjectGroupNotFound), [pg.Path],
                        ProjectConfigurationFileName);

                    return -1;
                }

                Deploy(args, baseDir, true);
                Directory.SetCurrentDirectory(workingDir);
                continue;
            }

            Project project = (Project)component;
            ProjectReference reference = new(null)
            {
                CopyToOutput = true,
                ProjectFile = project.Path
            };

            DassieConfig projectConfig = new(null) { References = [] };

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

        if (((group.Targets ??= new(null)).Targets ??= []).Length == 0)
        {
            EmitWarningMessageFormatted(
                0, 0, 0,
                DS0131_ProjectGroupNoTargets,
                nameof(StringHelper.DeployCommand_NoTargets), [],
                ProjectConfigurationFileName);
        }

        foreach (XmlNode target in group.Targets.Targets)
        {
            if (!ExtensionLoader.DeploymentTargets.Any(t => t.Name == target.Name))
            {
                if (ignoreMissing)
                    continue;

                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0237_DeploymentTargetNotFound,
                    nameof(StringHelper.DeployCommand_DeploymentTargetNotFound), [target.Name],
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
                string msg = nameof(StringHelper.DeployCommand_DeploymentTargetNonzeroExit);
                object[] msgArgs = [target.Name];

                if (failFast)
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0238_DeploymentTargetFailed,
                        msg, msgArgs,
                        CompilerExecutableName);

                    return 238;
                }

                EmitWarningMessageFormatted(
                    0, 0, 0,
                    DS0238_DeploymentTargetFailed,
                    msg, msgArgs,
                    CompilerExecutableName);
            }
        }

        if (!noDeleteTempDirectory)
            Directory.Delete(tempDir, true);

        return 0;
    }
}