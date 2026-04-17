using Dassie.Extensions;
using Dassie.Scripting;

namespace Dassie.Core.Actions;

internal class CodeBuildAction : IBuildAction
{
    public string Name => "Code";
    public ActionModes SupportedModes => ActionModes.All;

    public int Execute(ActionContext context) => ScriptRunner.Execute(context.Text);
}