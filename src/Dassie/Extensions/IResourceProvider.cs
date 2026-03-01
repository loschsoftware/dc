using System.Collections.Generic;
using System.Globalization;

namespace Dassie.Extensions;

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
}