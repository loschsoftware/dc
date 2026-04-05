using Dassie.Extensions;
using System;
using System.Linq;

namespace Dassie.Core.Actions;

internal class InputBuildAction : IBuildAction
{
    public string Name => "Input";
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

        if (context.XmlAttributes.Any(a => a.Name == "Prompt"))
            Console.Write(context.XmlAttributes.First(a => a.Name == "Prompt").Value);

        string input = Console.ReadLine();

        string macroToSet = context.XmlAttributes.First(a => a.Name == "Macro").Value;
        context.MacroParser.AddOrOverride(new(null)
        {
            Macro = macroToSet,
            Value = input
        });

        return 0;
    }
}