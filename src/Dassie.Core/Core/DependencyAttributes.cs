using System;

namespace Dassie.Core;

/// <summary>
/// Marks a generic parameter as a compile-time dependency.
/// </summary>
[AttributeUsage(AttributeTargets.GenericParameter)]
public class CompileTimeDependencyAttribute : Attribute { }

/// <summary>
/// Marks a generic parameter as a runtime dependency.
/// </summary>
[AttributeUsage(AttributeTargets.GenericParameter)]
public class RuntimeDependencyAttribute : Attribute { }

/// <summary>
/// Marks a field as a dependent value.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class DependentValueAttribute : Attribute
{
    /// <summary>
    /// The generic parameter name.
    /// </summary>
    public string ParameterName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DependentValueAttribute"/> class.
    /// </summary>
    /// <param name="paramName">The name of the dependent value.</param>
    public DependentValueAttribute(string paramName) => ParameterName = paramName;
}