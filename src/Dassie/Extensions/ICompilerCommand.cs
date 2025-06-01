using System.Collections.Generic;

namespace Dassie.Extensions;

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
    /// A list of alternative command names.
    /// </summary>
    public virtual List<string> Aliases() => [];

    /// <summary>
    /// A usage hint displayed in the compiler help screen.
    /// </summary>
    public string UsageString { get; }

    /// <summary>
    /// A short description of the command.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Represents the information displayed on the command's help page.
    /// </summary>
    /// <returns>Returns <see langword="null"/> by default.</returns>
    public virtual CommandHelpDetails HelpDetails() => null;

    /// <summary>
    /// An extended help text displayed when using the 'dc help &lt;Command&gt;' command.
    /// </summary>
    public virtual string Help() => "";

    /// <summary>
    /// Determines wheter or not the command is hidden, which makes it not visible on the help page.
    /// </summary>
    /// <returns>Returns <see langword="false"/> by default.</returns>
    public virtual bool Hidden() => false;

    /// <summary>
    /// The method that is executed when the command is invoked.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the command, excluding the command name itself.</param>
    /// <returns>The exit code.</returns>
    public int Invoke(string[] args);
}