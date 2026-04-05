using Dassie.Extensions;
using System.Linq;

namespace Dassie.Core.Actions;

internal class AssignBuildAction : IBuildAction
{
    public string Name => "Assign";
    public ActionModes SupportedModes => ActionModes.All;

    public int Execute(ActionContext context)
    {
        if (!context.XmlAttributes.Any(a => a.Name == "Value"))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0283_BuildActionMissingParameter,
                nameof(StringHelper.IBuildAction_MissingAttribute), [Name, "Value"],
                ProjectConfigurationFileName);

            return -1;
        }

        if (!context.XmlAttributes.Any(a => a.Name == "Macro"))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0283_BuildActionMissingParameter,
                nameof(StringHelper.IBuildAction_MissingAttribute), [Name, "Macro"],
                ProjectConfigurationFileName);

            return -1;
        }

        string macroToSet = context.XmlAttributes.First(a => a.Name == "Macro").Value;
        string value = context.XmlAttributes.First(a => a.Name == "Value").Value;

        context.MacroParser.AddOrOverride(new(null)
        {
            Macro = macroToSet,
            Value = value
        });

        return 0;
    }
}