using System;

namespace Dassie.Core;

/// <summary>
/// Represents a type alias with 'newtype' semantics. Newtype aliases are erased to their underlying type at runtime.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class NewTypeAttribute : Attribute { }

/// <summary>
/// Marks a parameter, local variable, return type or field as requiring a value to be of the newtype specified by the type parameter.
/// </summary>
[AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class NewTypeCallSiteAttribute : Attribute
{
    /// <summary>
    /// The type, marked with the <see cref="NewTypeAttribute"/> attribute, which this call site requires.
    /// </summary>
    public Type NewType { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="NewTypeCallSiteAttribute"/> type.
    /// </summary>
    /// <param name="newType">The type, marked with the <see cref="NewTypeAttribute"/> attribute, which this call site requires.</param>
    public NewTypeCallSiteAttribute(Type newType) => NewType = newType;
}