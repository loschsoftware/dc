using Dassie.Cli;
using Dassie.Extensions;
using System.CommandLine.Parsing;
using System.Linq;

namespace Dassie.Core.Actions;

internal class CompilerCommandBuildAction : IBuildAction
{
    public string Name => "CompilerCommand";
    public ActionModes SupportedModes => ActionModes.All;

    public int Execute(ActionContext context)
    {
        if (context.XmlAttributes is null or [] || !context.XmlAttributes.Any(a => a.Name == "Command"))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0090_InvalidDSConfigProperty,
                $"Build action '{Name}': Missing required attribute 'Command'.",
                ProjectConfigurationFileName);

            return -1;
        }

        string commandName = context.XmlAttributes.First(a => a.Name == "Command").Value;
        string args = "";

        if (context.XmlAttributes.Any(a => a.Name == "Arguments"))
            args = context.XmlAttributes.First(a => a.Name == "Arguments").Value;

        return Program.Main([commandName, .. CommandLineParser.SplitCommandLine(args)]);
    }
}