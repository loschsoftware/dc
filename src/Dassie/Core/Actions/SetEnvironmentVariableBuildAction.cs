using Dassie.Extensions;
using System;
using System.Linq;

namespace Dassie.Core.Actions;

internal class SetEnvironmentVariableBuildAction : IBuildAction
{
    public string Name => "SetEnvironmentVariable";
    public ActionModes SupportedModes => ActionModes.All;

    public int Execute(ActionContext context)
    {
        if (!context.XmlAttributes.Any(a => a.Name == "Var"))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0275_SetEnvironmentVariableMissingAttribute,
                nameof(StringHelper.SetEnvironmentVariableBuildAction_MissingRequiredAttribute), ["Var"],
                ProjectConfigurationFileName);

            return -1;
        }

        if (!context.XmlAttributes.Any(a => a.Name == "Value"))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0275_SetEnvironmentVariableMissingAttribute,
                nameof(StringHelper.SetEnvironmentVariableBuildAction_MissingRequiredAttribute), ["Value"],
                ProjectConfigurationFileName);

            return -1;
        }

        Environment.SetEnvironmentVariable(
            context.XmlAttributes.First(a => a.Name == "Var").Value,
            context.XmlAttributes.First(a => a.Name == "Value").Value);

        return 0;
    }
}