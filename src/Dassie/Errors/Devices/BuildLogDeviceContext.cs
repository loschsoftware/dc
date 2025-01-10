using Dassie.Extensions;
using System.Collections.Generic;

namespace Dassie.Errors.Devices;

/// <summary>
/// Defines the invocation context of a build log device.
/// </summary>
/// <param name="Device">The device to use.</param>
/// <param name="Attributes">The XML attributes passed to the device.</param>
/// <param name="Elements">The XML elements passed to the device.</param>
public record BuildLogDeviceContext(
    IBuildLogDevice Device,
    Dictionary<string, object> Attributes,
    Dictionary<string, object> Elements);