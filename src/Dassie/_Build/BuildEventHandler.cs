using Dassie.Configuration;
using Dassie.Extensions;
using System.Linq;
using System.Xml;

namespace Dassie.Build;

internal static class BuildEventHandler
{
    public static int ExecuteBuildEvent(BuildEvent buildEvent, bool isPreBuildEvent)
    {
        if (buildEvent.CommandNodes is null or [])
            return 0;

        foreach (XmlElement command in buildEvent.CommandNodes)
        {
            string name = command.Name;

            if (!ExtensionLoader.BuildActions.Any(a => a.Name == name))
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0262_DCBuildInvalidActionName,
                    $"The build action '{name}' could not be found.",
                    CompilerExecutableName);

                continue;
            }

            IBuildAction action = ExtensionLoader.BuildActions.First(a => a.Name == name);
            string eventKind;
            ActionModes currentMode;

            if (isPreBuildEvent)
            {
                currentMode = ActionModes.PreBuildEvent;
                eventKind = "pre-build";
            }
            else
            {
                currentMode = ActionModes.PostBuildEvent;
                eventKind = "post-build";
            }

            if (!action.SupportedModes.HasFlag(currentMode))
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0263_BuildActionInvalidMode,
                    $"The build action '{name}' cannot be executed as part of a {eventKind} event.",
                    CompilerExecutableName);

                continue;
            }

            int ret = action.Execute(new(
                command.ChildNodes.Cast<XmlNode>().ToList(),
                command.Attributes.Cast<XmlAttribute>().ToList(),
                command.InnerText,
                currentMode,
                buildEvent.Name));

            string errMsg = $"The build action '{name}' ({buildEvent.CommandNodes.IndexOf(command) + 1}) ended with nonzero exit code {ret}.";

            if (ret != 0)
            {
                if (buildEvent.Critical)
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0264_BuildActionFailed,
                        errMsg,
                        CompilerExecutableName);

                    return ret;
                }

                EmitWarningMessage(
                    0, 0, 0,
                    DS0264_BuildActionFailed,
                    errMsg,
                    CompilerExecutableName);
            }
        }

        return 0;
    }
}