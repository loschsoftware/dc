using Dassie.Configuration;
using Dassie.Configuration.Macros;
using Dassie.Extensions;

namespace Dassie.Cli.Commands;

internal class TestCommand : ICompilerCommand
{
    private static TestCommand _instance;
    public static TestCommand Instance => _instance ??= new();

    public string Command => "test";

    public string UsageString => "test";

    public string Description => "Runs unit tests defined for the current project or project group.";

    public int Invoke(string[] args)
    {
        DassieConfig config = ProjectFileDeserializer.DassieConfig;
        config ??= new();

        MacroParser parser = new();
        parser.ImportMacros(MacroGenerator.GenerateMacrosForProject(config));
        parser.Normalize(config);

        if (!File.Exists(ProjectConfigurationFileName))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0236_DCTestNoProjectFile,
                $"Current directory contains no configuration file ({ProjectConfigurationFileName}).",
                ProjectConfigurationFileName);

            return -1;
        }

        return 0;
    }
}