using Dassie.Errors;
using Dassie.Errors.Devices;
using System;
using System.Collections.Generic;

namespace Dassie.Extensions;

/// <summary>
/// Defines a mechanism for redirecting Dassie compiler logs to different outputs.
/// </summary>
public interface IBuildLogDevice
{
    /// <summary>
    /// The name of the build log device.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The method called when a compiler message is emitted.
    /// </summary>
    /// <param name="error">The compiler message that was emitted.</param>
    /// <param name="attributes">The XML attribute values passed to the log device.</param>
    /// <param name="elements">The XML element values passed to the log device.</param>
    public void Log(ErrorInfo error, Dictionary<string, object> attributes, Dictionary<string, object> elements);

    /// <summary>
    /// A dictionary of XML attributes that can be applied to the build log device.
    /// </summary>
    public virtual Dictionary<string, Type> Attributes => [];

    /// <summary>
    /// A dictionary of XML elements that can be used on the build log device.
    /// </summary>
    public virtual Dictionary<string, Type> Elements => [];
}