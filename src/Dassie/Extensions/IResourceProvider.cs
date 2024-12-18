using System.Collections.Generic;

namespace Dassie.Extensions;

/// <summary>
/// Defines a mechanism to define and override compiler resources, such as localization strings.
/// </summary>
public interface IResourceProvider<TRes>
{
    /// <summary>
    /// The resources defined by the resource provider, stored as a dictionary with string keys.
    /// </summary>
    public Dictionary<string, TRes> Resources { get; }
}