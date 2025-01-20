using System;

namespace Dassie.Core;

/// <summary>
/// Applied by the Dassie compiler to alias types.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate)]
public class AliasAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the type that is aliased by the type marked with <see cref="AliasAttribute"/>.
    /// </summary>
    public Type AliasedType { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="AliasAttribute"/>.
    /// </summary>
    /// <param name="aliasedType">The aliased type.</param>
    public AliasAttribute(Type aliasedType) => AliasedType = aliasedType;
}