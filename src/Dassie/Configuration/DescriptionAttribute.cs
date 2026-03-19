using System;

namespace Dassie.Configuration;

[AttributeUsage(AttributeTargets.Property)]
internal class DescriptionAttribute: Attribute
{
    public string Description { get; set; }
    public DescriptionAttribute(string description) => Description = description;
}