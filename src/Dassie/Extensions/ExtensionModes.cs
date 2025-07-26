using System;

namespace Dassie.Extensions;

/// <summary>
/// Represents the modes that extensions can be loaded in.
/// </summary>
[Flags]
public enum ExtensionModes
{
    /// <summary>
    /// The extension can be loaded in global mode.
    /// </summary>
    Global = 1,
    /// <summary>
    /// The extension can be loaded in transient mode.
    /// </summary>
    Transient = 2
}