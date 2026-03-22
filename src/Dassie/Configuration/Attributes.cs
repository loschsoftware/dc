using System;

namespace Dassie.Configuration;

[AttributeUsage(AttributeTargets.Property)]
internal class DescriptionAttribute : Attribute
{
    public string Description { get; set; }
    public DescriptionAttribute(string description) => Description = description;
}

/// <summary>
/// Marks a property as being part of the Dassie configuration system.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigPropertyAttribute : Attribute;