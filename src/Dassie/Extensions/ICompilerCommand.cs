namespace Dassie.Extensions;

/// <summary>
/// Defines an external command used to add additional features to the Dassie compiler.
/// </summary>
public interface ICompilerCommand
{
    /// <summary>
    /// The name used to invoke the command in the console.
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// A usage hint displayed in the compiler help screen.
    /// </summary>
    public string UsageString { get; }

    /// <summary>
    /// A short description of the command.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The method that is executed when the command is invoked.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the command, excluding the command name itself.</param>
    /// <returns>The exit code.</returns>
    public int Invoke(string[] args);
}