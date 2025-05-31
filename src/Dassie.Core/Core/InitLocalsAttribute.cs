using System;

namespace Dassie.Core;

/// <summary>
/// Used to configure the compiler behavior for zero-initialization of local variables of methods.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class InitLocalsAttribute : Attribute
{
    /// <summary>
    /// A boolean value indicating wheter the method marked with the <see cref="InitLocalsAttribute"/> attribute should include the <c>.locals init</c> IL directive. If this attribute is not applied, the default value <see langword="true"/> is used implicitly on all methods.
    /// </summary>
    public bool InitLocals { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InitLocalsAttribute"/> type with the specified value.
    /// </summary>
    /// <param name="initLocals">A boolean value indicating wheter the method marked with the <see cref="InitLocalsAttribute"/> attribute should include the <c>.locals init</c> IL directive.</param>
    public InitLocalsAttribute(bool initLocals)
    {
        InitLocals = initLocals;
    }
}