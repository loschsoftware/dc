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
/// Specifies the location of a macro invocation.
/// </summary>
/// <param name="Document">The document the macro was invoked in.</param>
/// <param name="Line">The line the macro was invoked on.</param>
/// <param name="Column">The column of the first character of the macro invocation.</param>
public record MacroInvocationInfo(
    string Document,
    int Line,
    int Column);

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
    /// <param name="info">The location information of the macro invocation.</param>
    /// <returns>The expanded value of the current macro.</returns>
    public string Expand(Dictionary<string, string> arguments, MacroInvocationInfo info);
}