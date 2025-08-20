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
            EmitErrorMessage(
                0, 0, 0,
                DS0239_DirectoryTargetPathRequired,
                $"Path required for 'Directory' deployment target.",
                CompilerExecutableName);

            return 239;
        }

        FileSystem.CopyDirectory(context.SourceDirectory, context.Elements[0].InnerText, true);
        return 0;
    }
}