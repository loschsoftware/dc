using Dassie.Extensions;

namespace Dassie.Core.Actions;

internal class LogBuildAction : IBuildAction
{
    public string Name => "Log";
    public ActionModes SupportedModes => ActionModes.All;

    public int Execute(ActionContext context)
    {
        EmitBuildLogMessage(context.Text, 0);
        return 0;
    }
}