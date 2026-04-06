using Dassie.Extensions;
using Ganss.IO;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Dassie.Core.Actions;

internal class DeleteBuildAction : IBuildAction
{
    public string Name => "Delete";
    public ActionModes SupportedModes => ActionModes.All;

    public int Execute(ActionContext context)
    {
        if (!context.XmlAttributes.Any(a => a.Name == "Pattern"))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0283_BuildActionMissingParameter,
                nameof(StringHelper.IBuildAction_MissingAttribute), [Name, "Pattern"],
                ProjectConfigurationFileName);

            return -1;
        }

        string pattern = context.XmlAttributes.First(a => a.Name == "Pattern").Value;

        IEnumerable<IFileSystemInfo> matches = Glob.Expand(pattern);
        FileSystemHelpers.Delete(matches);
        return 0;
    }
}