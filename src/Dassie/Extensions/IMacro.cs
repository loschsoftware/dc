using System;
using System.Collections.Generic;

namespace Dassie.Extensions;

/// <summary>
/// Represents a parameter of a macro.
/// </summary>
/// <param name="Name">The name of the parameter.</param>
/// <param name="IsEager">Wheter or not the parameter is eagerly evaluated. The default value is <see langword="false"/>.</param>
public record MacroParameter(
    string Name,
    bool IsEager = false);

/// <summary>
/// Represents options that modify the behavior of the macro.
/// </summary>
[Flags]
public enum MacroOptions
{
    /// <summary>
    /// No options are set.
    /// </summary>
    None = 0,
    /// <summary>
    /// Specifies that the result of a macro expansion can be cached, preventing the macro from being invoked again.
    /// </summary>
    AllowCaching = 1
}

/// <summary>
/// Defines a mechanism for extensions to provide build system macros.
/// </summary>
public interface IMacro
{
    /// <summary>
    /// The name of the macro.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The parameters of the macro.
    /// </summary>
    public List<MacroParameter> Parameters { get; }

    /// <summary>
    /// Specifies options for the macro.
    /// </summary>
    public MacroOptions Options { get; }

    /// <summary>
    /// The method called when a macro expansion is requested.
    /// </summary>
    /// <param name="arguments">The arguments passed to the macro.</param>
    /// <returns>The expanded value of the current macro.</returns>
    public string Expand(Dictionary<string, string> arguments);
}