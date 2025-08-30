using System.Collections.Generic;

namespace Dassie.Extensions;

/// <summary>
/// Represents the information displayed on a command's help page.
/// </summary>
public class CommandHelpDetails
{
    /// <summary>
    /// A short description of the command. Most of the time, this is the same as <see cref="ICompilerCommand.Description"/>.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// A list of usage strings for the command. Most of the time, this list has only a single entry that represents the command with all its flags and options.
    /// </summary>
    public List<string> Usage { get; set; }

    /// <summary>
    /// A list of options, along with their description.
    /// </summary>
    public List<(string Option, string Description)> Options { get; set; }

    /// <summary>
    /// Further, more detailed information regarding the command.
    /// </summary>
    public string Remarks { get; set; }

    /// <summary>
    /// A list of custom help sections.
    /// </summary>
    public List<(string Heading, string Text)> CustomSections { get; set; }
}