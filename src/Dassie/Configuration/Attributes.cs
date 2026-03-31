using System;

namespace Dassie.Configuration;

/// <summary>
/// Sets the description of a property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DescriptionAttribute : Attribute
{
    /// <summary>
    /// The description of the property.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="DescriptionAttribute"/> class with the specified description.
    /// </summary>
    /// <param name="description">The description of the property.</param>
    public DescriptionAttribute(string description) => Description = description;
}

/// <summary>
/// Marks a property as being part of the Dassie configuration system.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigPropertyAttribute : Attribute;

/// <summary>
/// Marks a property as explicit, which causes it to always be serialized,
/// even if the property is set to its default value.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ExplicitAttribute : Attribute;