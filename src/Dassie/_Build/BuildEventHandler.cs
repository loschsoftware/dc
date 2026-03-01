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
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0262_DCBuildInvalidActionName,
                    nameof(StringHelper.BuildEventHandler_BuildActionNotFound), [name],
                    CompilerExecutableName);

                continue;
            }

            IBuildAction action = ExtensionLoader.BuildActions.First(a => a.Name == name);
            string eventKind;
            ActionModes currentMode;

            if (isPreBuildEvent)
            {
                currentMode = ActionModes.PreBuildEvent;
                eventKind = StringHelper.BuildEventHandler_BuildActionModePreBuild;
            }
            else
            {
                currentMode = ActionModes.PostBuildEvent;
                eventKind = StringHelper.BuildEventHandler_BuildActionModePostBuild;
            }

            if (!action.SupportedModes.HasFlag(currentMode))
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0263_BuildActionInvalidMode,
                    nameof(StringHelper.BuildEventHandler_BuildActionInvalidMode), [name, eventKind],
                    CompilerExecutableName);

                continue;
            }

            int ret = action.Execute(new(
                command.ChildNodes.Cast<XmlNode>().ToList(),
                command.Attributes.Cast<XmlAttribute>().ToList(),
                command.InnerText,
                currentMode,
                buildEvent.Name));

            string errId = nameof(StringHelper.BuildEventHandler_BuildActionFailed);
            object[] errArgs = [name, buildEvent.CommandNodes.IndexOf(command) + 1, ret];

            if (ret != 0)
            {
                if (buildEvent.Critical)
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0264_BuildActionFailed,
                        errId, errArgs,
                        CompilerExecutableName);

                    return ret;
                }

                EmitWarningMessageFormatted(
                    0, 0, 0,
                    DS0264_BuildActionFailed,
                    errId, errArgs,
                    CompilerExecutableName);
            }
        }

        return 0;
    }
}