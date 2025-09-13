using Dassie.Deployment;

namespace Dassie.Extensions;

/// <summary>
/// Defines a target used by the 'dc deploy' command.
/// </summary>
public interface IDeploymentTarget
{
    /// <summary>
    /// The name of the deployment target.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The method called when the deployment target is executed.
    /// </summary>
    /// <param name="context">The context of the deployment.</param>
    /// <returns>An exit code indicating wheter the operation was successful.</returns>
    public int Execute(DeploymentContext context);
}