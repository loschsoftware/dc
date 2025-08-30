using Dassie.Extensions;
using System.Linq;

namespace Dassie.Cli.Commands;

/// <summary>
/// Looks up and invokes compiler commands.
/// </summary>
internal static class CommandHandler
{
    private static readonly string[] _helpOptions = ["-h", "--help", "/help", "/?"];

    /// <summary>
    /// Attempts to invoke a command with specific arguments.
    /// </summary>
    /// <param name="name">The name of the command to invoke.</param>
    /// <param name="args">The arguments passed to the command.</param>
    /// <param name="errorCode">The error code of the command.</param>
    /// <returns><see langword="true"/>, if the command was executed. <see langword="false"/>, if the command could not be found.</returns>
    public static bool TryInvoke(string name, string[] args, out int errorCode)
    {
        if (ExtensionLoader.Commands.Any(c => c.Command == name || c.Aliases().Any(a => a == name)))
        {
            ICompilerCommand selectedCommand = ExtensionLoader.Commands.First(c => c.Command == name || c.Aliases().Any(a => a == name));

            if (args.Any(a => _helpOptions.Contains(a)))
            {
                errorCode = HelpCommand.DisplayHelpForCommand(selectedCommand);
                return true;
            }

            if (selectedCommand.Command == "compile")
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0250_DCCompileInvoked,
                    $"The command 'compile' cannot be executed directly. To compile Dassie source files, use 'dc <Files>'.",
                    CompilerExecutableName);

                errorCode = 250;
                return true;
            }

            errorCode = selectedCommand.Invoke(args);
            return true;
        }

        errorCode = 0;
        return false;
    }
}