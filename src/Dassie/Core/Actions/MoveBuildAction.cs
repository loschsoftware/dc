using Dassie.Extensions;
using Ganss.IO;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Dassie.Core.Actions;

internal class MoveBuildAction : IBuildAction
{
    public string Name => "Move";
    public ActionModes SupportedModes => ActionModes.All;

    public int Execute(ActionContext context)
    {
        if (!context.XmlAttributes.Any(a => a.Name == "From"))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0283_BuildActionMissingParameter,
                nameof(StringHelper.IBuildAction_MissingAttribute), [Name, "From"],
                ProjectConfigurationFileName);

            return -1;
        }

        if (!context.XmlAttributes.Any(a => a.Name == "To"))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0283_BuildActionMissingParameter,
                nameof(StringHelper.IBuildAction_MissingAttribute), [Name, "To"],
                ProjectConfigurationFileName);

            return -1;
        }

        string from = context.XmlAttributes.First(a => a.Name == "From").Value;
        string to = context.XmlAttributes.First(a => a.Name == "To").Value;

        IEnumerable<IFileSystemInfo> matches = Glob.Expand(from);
        FileSystemHelpers.CopyOrMove(matches, to, false, false, true);
        return 0;
    }
}