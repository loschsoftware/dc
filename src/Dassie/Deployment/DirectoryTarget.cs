using Dassie.Extensions;
using Microsoft.VisualBasic.FileIO;

namespace Dassie.Deployment;

internal class DirectoryTarget : IDeploymentTarget
{
    public string Name { get; set; } = "Directory";

    public int Execute(DeploymentContext context)
    {
        if (context.Elements == null || context.Elements.Count == 0)
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0239_DirectoryTargetPathRequired,
                nameof(StringHelper.DirectoryTarget_PathRequired), [],
                CompilerExecutableName);

            return 239;
        }

        FileSystem.CopyDirectory(context.SourceDirectory, context.Elements[0].InnerText, true);
        return 0;
    }
}