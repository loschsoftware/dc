using System.Collections.Generic;

namespace Dassie.Extensions;

/// <summary>
/// Defines a mechanism for extensions to provide build system macros.
/// </summary>
public interface IMacro
{
    /// <summary>
    /// The name of the macro.
    /// </summary>
    public string Macro { get; }

    /// <summary>
    /// The method called when a macro expansion is requested.
    /// </summary>
    /// <returns>The expanded value of the current macro.</returns>
    public string Expand();
}