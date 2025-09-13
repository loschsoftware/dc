using System.Collections.Generic;

namespace Dassie.Extensions;

/// <summary>
/// Provides an abstract base class for commands implementing <see cref="ICompilerCommand"/>.
/// </summary>
public abstract class CompilerCommand : ICompilerCommand
{
    /// <inheritdoc/>
    public abstract string Command { get; }

    /// <inheritdoc/>
    public abstract string Description { get; }

    /// <inheritdoc/>
    public virtual List<string> Aliases => [];

    /// <inheritdoc/>
    public virtual CommandHelpDetails HelpDetails => null;

    /// <inheritdoc/>
    public virtual CommandOptions Options => CommandOptions.None;

    /// <inheritdoc/>
    public abstract int Invoke(string[] args);
}