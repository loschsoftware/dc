using Dassie.Configuration;
using Dassie.Configuration.Macros;
using Dassie.Extensions;
using System.Linq;
using System.Xml;

namespace Dassie.Build;

internal static class BuildEventHandler
{
    public static int ExecuteBuildEvent(BuildEvent buildEvent, DassieConfig config, bool isPreBuildEvent)
    {
        if (buildEvent.CommandNodes is null or [])
            return 0;

        Context ??= new();
        Context.Configuration = config;
        Context.ConfigurationPath ??= ProjectConfigurationFileName;

        MacroParser parser = new(config);
        parser.SetMacroDefinitions(ProjectFileSerializer.MacroDefinitions);

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

            XmlElement expanded = ExpandElement(command, parser);

            int ret = action.Execute(new(
                expanded.ChildNodes.Cast<XmlNode>().ToList(),
                expanded.Attributes.Cast<XmlAttribute>().ToList(),
                expanded.InnerText,
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

    private static XmlElement ExpandElement(XmlElement source, MacroParser parser)
    {
        XmlDocument doc = new();
        XmlElement clone = (XmlElement)doc.ImportNode(source, true);
        doc.AppendChild(clone);

        ExpandNodeRecursive(clone, parser);
        return clone;
    }

    private static void ExpandNodeRecursive(XmlNode node, MacroParser parser)
    {
        if (node is XmlText or XmlCDataSection)
        {
            node.Value = parser.Expand(node.Value ?? "").Value;
            return;
        }

        if (node is XmlElement elem)
        {
            foreach (XmlAttribute attribute in elem.Attributes.Cast<XmlAttribute>())
                attribute.Value = parser.Expand(attribute.Value ?? "").Value;
        }

        foreach (XmlNode child in node.ChildNodes.Cast<XmlNode>())
            ExpandNodeRecursive(child, parser);
    }
}