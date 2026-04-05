using Dassie.Extensions;
using System.Linq;

namespace Dassie.Core.Actions;

internal class ReadFileBuildAction : IBuildAction
{
    public string Name => "ReadFile";
    public ActionModes SupportedModes => ActionModes.All;

    public int Execute(ActionContext context)
    {
        if (!context.XmlAttributes.Any(a => a.Name == "Macro"))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0283_BuildActionMissingParameter,
                nameof(StringHelper.IBuildAction_MissingAttribute), [Name, "Macro"],
                ProjectConfigurationFileName);

            return -1;
        }

        if (!context.XmlAttributes.Any(a => a.Name == "Path"))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0283_BuildActionMissingParameter,
                nameof(StringHelper.IBuildAction_MissingAttribute), [Name, "Path"],
                ProjectConfigurationFileName);

            return -1;
        }

        string macroToSet = context.XmlAttributes.First(a => a.Name == "Macro").Value;
        string path = context.XmlAttributes.First(a => a.Name == "Path").Value;

        if (!File.Exists(path))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0024_InvalidFileReference,
                nameof(StringHelper.IBuildAction_FileNotFound), [Name, path],
                ProjectConfigurationFileName);

            return -1;
        }

        context.MacroParser.AddOrOverride(new(null)
        {
            Macro = macroToSet,
            Value = File.ReadAllText(path)
        });

        return 0;
    }
}