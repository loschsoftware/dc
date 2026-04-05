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
    /// Invokes the designated help command.
    /// </summary>
    /// <param name="args">The arguments passed to the command.</param>
    /// <returns>The return value of the invocation.</returns>
    public static int InvokeHelpCommand(string[] args)
    {
        if (!ExtensionLoader.Commands.Any(c => c.Role == CommandRole.Help))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0284_SpecialCommandInvocationFailed,
                nameof(StringHelper.CommandHandler_NoHelpCommandInstalled), [],
                CompilerExecutableName);

            return -1;
        }

        ICompilerCommand helpCommand = ExtensionLoader.Commands.First(c => c.Role == CommandRole.Help);

        if (ExtensionLoader.Commands.Where(c => c.Role == CommandRole.Help).Count() > 1)
        {
            IPackage containingPackage = ExtensionLoader.InstalledExtensions.First(p => p.Commands().Contains(helpCommand));

            EmitWarningMessageFormatted(
                0, 0, 0,
                DS0284_SpecialCommandInvocationFailed,
                nameof(StringHelper.CommandHandler_MultipleHelpCommandsInstalled), [helpCommand.Command, containingPackage.Metadata.Name],
                CompilerExecutableName);
        }

        return helpCommand.Invoke(args);
    }

    /// <summary>
    /// Invokes the default command.
    /// </summary>
    /// <param name="args">The arguments passed to the command.</param>
    /// <returns>The return value of the invocation.</returns>
    public static int InvokeDefaultCommand(string[] args)
    {
        if (!ExtensionLoader.Commands.Any(c => c.Role == CommandRole.Default))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0284_SpecialCommandInvocationFailed,
                nameof(StringHelper.CommandHandler_NoDefaultCommandInstalled), [],
                CompilerExecutableName);

            return -1;
        }

        ICompilerCommand defaultCommand = ExtensionLoader.Commands.First(c => c.Role == CommandRole.Default);

        if (ExtensionLoader.Commands.Where(c => c.Role == CommandRole.Default).Count() > 1)
        {
            IPackage containingPackage = ExtensionLoader.InstalledExtensions.First(p => p.Commands().Contains(defaultCommand));

            EmitWarningMessageFormatted(
                0, 0, 0,
                DS0284_SpecialCommandInvocationFailed,
                nameof(StringHelper.CommandHandler_MultipleDefaultCommandsInstalled), [defaultCommand.Command, containingPackage.Metadata.Name],
                CompilerExecutableName);
        }

        return defaultCommand.Invoke(args);
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
        if (ExtensionLoader.Commands.Any(c => c.Command == name || c.Aliases.Any(a => a == name)))
        {
            ICompilerCommand selectedCommand = ExtensionLoader.Commands.First(c => c.Command == name || c.Aliases.Any(a => a == name));

            if (args != null && args.Length >= 1 && _helpOptions.Contains(args[0]) && !selectedCommand.Options.HasFlag(CommandOptions.NoHelpRouting))
            {
                errorCode = InvokeHelpCommand([selectedCommand.Command]);
                return true;
            }

            if (selectedCommand.Options.HasFlag(CommandOptions.NoDirectInvocation))
            {
                if (selectedCommand.Role == CommandRole.Default && selectedCommand.Command == "compile")
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0250_DCCompileInvoked,
                        nameof(StringHelper.CommandHandler_DirectInvocationNotSupported_Compile), [],
                        CompilerExecutableName);
                }
                else
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0250_DCCompileInvoked,
                        nameof(StringHelper.CommandHandler_DirectInvocationNotSupported), [selectedCommand.Command],
                        CompilerExecutableName);
                }

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