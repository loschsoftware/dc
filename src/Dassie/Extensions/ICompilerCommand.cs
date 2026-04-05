using System.Collections.Generic;

namespace Dassie.Extensions;

/// <summary>
/// Specifies the role of a command in the runtime system.
/// </summary>
public enum CommandRole
{
    /// <summary>
    /// Specifies no special role.
    /// </summary>
    None,
    /// <summary>
    /// Specifies that this command acts as the default help provider.
    /// </summary>
    Help,
    /// <summary>
    /// Specifies that this command acts as the default command
    /// that is invoked if no command name is specified.
    /// </summary>
    Default
}

/// <summary>
/// Defines a command used to add additional features to the Dassie compiler.
/// </summary>
public interface ICompilerCommand
{
    /// <summary>
    /// The name used to invoke the command in the console.
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// A short description of the command.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// A list of alternative command names.
    /// </summary>
    public virtual List<string> Aliases => [];

    /// <summary>
    /// Represents the information displayed on the command's help page.
    /// </summary>
    /// <returns>Returns <see langword="null"/> by default.</returns>
    public virtual CommandHelpDetails HelpDetails => null;

    /// <summary>
    /// Advanced options for the command within the command loading system.
    /// </summary>
    /// <returns>The options specified for the command.</returns>
    public virtual CommandOptions Options => CommandOptions.None;

    /// <summary>
    /// Specifies the role of the command.
    /// </summary>
    public virtual CommandRole Role => CommandRole.None;

    /// <summary>
    /// The method that is executed when the command is invoked.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the command, excluding the command name itself.</param>
    /// <returns>The exit code.</returns>
    public int Invoke(string[] args);
}