using System;

namespace Dassie.Extensions;

/// <summary>
/// Specifies options for compiler commands.
/// </summary>
[Flags]
public enum CommandOptions
{
    /// <summary>
    /// No options set.
    /// </summary>
    None = 0,
    /// <summary>
    /// The command is hidden, that is, it does not appear in the list of available commands.
    /// </summary>
    Hidden = 1,
    /// <summary>
    /// If set, the command system does not route invocations of the command with "--help" arguments to the standard command help system.
    /// </summary>
    NoHelpRouting = 2
}