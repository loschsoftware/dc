using Dassie.Extensions;
using System.Linq;

namespace Dassie.Core.Actions;

internal class LogBuildAction : IBuildAction
{
    public string Name => "Log";
    public ActionModes SupportedModes => ActionModes.All;

    public int Execute(ActionContext context)
    {
        int.TryParse(context.XmlAttributes.FirstOrDefault(a => a.Name == "MinVerbosity")?.Value, out int verbosity);
        EmitBuildLogMessage(context.Text, verbosity, verbosity > 0);
        return 0;
    }
}