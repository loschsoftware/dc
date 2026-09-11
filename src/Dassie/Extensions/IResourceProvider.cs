using System.Collections.Generic;

namespace Dassie.Extensions;

/// <summary>
/// Specifies the scope of a resource provider.
/// </summary>
public enum ResourceScope
{
    /// <summary>
    /// Resources are only available from within the current extension.
    /// </summary>
    Local,
    /// <summary>
    /// Resources are globally available.
    /// </summary>
    Global
}

/// <summary>
/// Defines a mechanism to define and override compiler resources, such as localization strings.
/// </summary>
public interface IResourceProvider<TRes>
{
    /// <summary>
    /// The culture of the resource provider.
    /// </summary>
    public string Culture { get; }

    /// <summary>
    /// The resources defined by the resource provider, stored as a dictionary with string keys.
    /// </summary>
    public Dictionary<string, TRes> Resources { get; }

    /// <summary>
    /// The scope of the resource provider.
    /// </summary>
    public virtual ResourceScope Scope => ResourceScope.Local;
}