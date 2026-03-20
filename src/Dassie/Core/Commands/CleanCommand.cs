using Dassie.Configuration;
using Dassie.Configuration.Macros;
using Dassie.Configuration.ProjectGroups;
using Dassie.Extensions;

namespace Dassie.Core.Commands;

internal class CleanCommand : CompilerCommand
{
    private static CleanCommand _instance;
    public static CleanCommand Instance => _instance ??= new();

    public override string Command => "clean";

    public override string Description => StringHelper.CleanCommand_Description;

    public override CommandHelpDetails HelpDetails => new()
    {
        Description = Description,
        Usage = ["dc clean"],
        Remarks = StringHelper.CleanCommand_Remarks,
        Options = [],
        Examples =
        [
            ("dc clean", StringHelper.CleanCommand_Example)
        ]
    };

    public override int Invoke(string[] args)
    {
        if (args.Length > 0)
        {
            foreach (string arg in args)
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0212_UnexpectedArgument,
                    nameof(StringHelper.CleanCommand_UnexpectedArgument), [arg],
                    CompilerExecutableName);
            }

            return -1;
        }

        if (!File.Exists(ProjectConfigurationFileName))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0129_DeployCommandInvalidProjectGroupFile,
                nameof(StringHelper.CleanCommand_MissingProjectFile), [ProjectConfigurationFileName],
                ProjectConfigurationFileName);

            return -1;
        }

        DassieConfig config = ProjectFileDeserializer.DassieConfig;
        config ??= new(PropertyStore.Empty_Todo);

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
        config ??= new(PropertyStore.Empty_Todo);

        MacroParser_Legacy parser = new();
        parser.ImportMacros(MacroGenerator.GenerateMacrosForProject(config));
        parser.Normalize(config);

        if (Directory.Exists(config.BuildDirectory))
            Directory.Delete(config.BuildDirectory, true);

        if (Directory.Exists(TemporaryBuildDirectoryName))
            Directory.Delete(TemporaryBuildDirectoryName, true);

        Directory.SetCurrentDirectory(workingDir);
    }
}