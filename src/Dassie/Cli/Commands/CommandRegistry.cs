using Dassie.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Dassie.Cli.Commands;

/// <summary>
/// Acts as a container for all installed compiler commands.
/// </summary>
internal static class CommandRegistry
{
    /// <summary>
    /// A list containing all available compiler commands, predefined as well as external.
    /// </summary>
    public static List<ICompilerCommand> Commands { get; private set; }

    /// <summary>
    /// Initializes the command list.
    /// </summary>
    public static void InitializeDefaults()
    {
        List<IPackage> packages = ExtensionLoader.LoadInstalledExtensions();
        packages.Add(DefaultCommandPackage.Instance);
        Commands = ExtensionLoader.GetAllCommands(packages);
    }

    /// <summary>
    /// Attempts to invoke a command with specific arguments.
    /// </summary>
    /// <param name="name">The name of the command to invoke.</param>
    /// <param name="args">The arguments passed to the command.</param>
    /// <param name="errorCode">The error code of the command.</param>
    /// <returns><see langword="true"/>, if the command was executed. <see langword="false"/>, if the command could not be found.</returns>
    public static bool TryInvoke(string name, string[] args, out int errorCode)
    {
        if (Commands.Any(c => c.Command == name || c.Aliases().Any(a => a == name)))
        {
            ICompilerCommand selectedCommand = Commands.First(c => c.Command == name || c.Aliases().Any(a => a == name));
            errorCode = selectedCommand.Invoke(args);
            return true;
        }

        errorCode = 0;
        return false;
    }
}