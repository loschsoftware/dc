using System;

namespace Dassie.Text.Tooltips;

/// <summary>
/// Represents a parameter of a function or method.
/// </summary>
public class Parameter
{
    /// <summary>
    /// The name of the parameter.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The type of the parameter.
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// An optional parameter constraint.
    /// </summary>
    public string Constraint { get; set; }
}