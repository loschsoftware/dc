using Dassie.Core.Commands;
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
        if (GetHelpCommand() is ICompilerCommand helpCommand)
            return helpCommand.Invoke(args);

        return -1;
    }

    private static ICompilerCommand _helpCommand;
    private static ICompilerCommand GetHelpCommand()
    {
        if (_helpCommand != null)
            return _helpCommand;

        if (!ExtensionLoader.Commands.Any(c => c.Role == CommandRole.Help))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0284_SpecialCommandInvocationFailed,
                nameof(StringHelper.CommandHandler_NoHelpCommandInstalled), [],
                CompilerExecutableName);

            return null;
        }

        ICompilerCommand helpCommand = ExtensionLoader.Commands.First(c => c.Role == CommandRole.Help);
        _helpCommand = helpCommand;

        if (ExtensionLoader.Commands.Count(c => c.Role == CommandRole.Help) > 1)
        {
            IPackage containingPackage = ExtensionLoader.InstalledExtensions.First(p => p.Commands().Contains(helpCommand));

            EmitWarningMessageFormatted(
                0, 0, 0,
                DS0284_SpecialCommandInvocationFailed,
                nameof(StringHelper.CommandHandler_MultipleHelpCommandsInstalled), [helpCommand.Command, containingPackage.Metadata.Name],
                CompilerExecutableName);
        }

        return helpCommand;
    }

    private static int InvokeHelpCommand(ICompilerCommand command)
    {
        if (GetHelpCommand() is ICompilerCommand helpCommand)
        {
            if (helpCommand == HelpCommand.Instance)
                return HelpCommand.DisplayHelpForCommand(command);

            return InvokeHelpCommand([command.Command]);
        }

        return -1;
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

    private static bool MatchCommandName(ICompilerCommand command, string name) => command.Command == name || command.Aliases.Contains(name);

    /// <summary>
    /// Attempts to invoke a command with specific arguments.
    /// </summary>
    /// <param name="name">The name of the command to invoke.</param>
    /// <param name="args">The arguments passed to the command.</param>
    /// <param name="errorCode">The error code of the command.</param>
    /// <returns><see langword="true"/>, if the command was executed. <see langword="false"/>, if the command could not be found.</returns>
    public static bool TryInvoke(string name, string[] args, out int errorCode)
    {
        if (ExtensionLoader.Commands.Any(c => MatchCommandName(c, name)))
        {
            ICompilerCommand selectedCommand = ExtensionLoader.Commands.First(c => MatchCommandName(c, name));
            return TryInvoke(selectedCommand, args, out errorCode);
        }

        errorCode = 0;
        return false;
    }

    private static bool TryInvoke(ICompilerCommand command, string[] args, out int errorCode)
    {
        if (args != null && args.Length >= 1 && _helpOptions.Contains(args[0]) && !command.Options.HasFlag(CommandOptions.NoHelpRouting))
        {
            errorCode = InvokeHelpCommand(command);
            return true;
        }

        if (command.Options.HasFlag(CommandOptions.NoDirectInvocation))
        {
            if (command.Role == CommandRole.Default && command.Command == "compile")
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
                    nameof(StringHelper.CommandHandler_DirectInvocationNotSupported), [command.Command],
                    CompilerExecutableName);
            }

            errorCode = 250;
            return true;
        }

        if (args.Length > 0 && command.Subcommands != null)
        {
            if (command.Subcommands.Any(c => MatchCommandName(c, args[0])))
            {
                ICompilerCommand subCommand = command.Subcommands.First(c => MatchCommandName(c, args[0]));
                return TryInvoke(subCommand, args[1..], out errorCode);
            }
            else if (command.Options.HasFlag(CommandOptions.ErrorOnInvalidSubcommand))
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0291_InvalidSubcommand,
                    nameof(StringHelper.CommandHandler_InvalidSubcommand),
                    [command.Command, args[0]],
                    CompilerExecutableName);

                errorCode = 291;
                return true;
            }
        }

        errorCode = command.Invoke(args);
        return true;
    }
}