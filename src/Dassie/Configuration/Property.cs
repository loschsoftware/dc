using System;

namespace Dassie.Configuration;

/// <summary>
/// Represents a configuration property.
/// </summary>
/// <param name="Name">The name of the property.</param>
/// <param name="Type">The type of the property.</param>
/// <param name="Default">The default value of the property.</param>
/// <param name="Description">The description of the property.</param>
/// <param name="CanBeCached">Specifies wheter the property value can be cached.</param>
public record Property(
    string Name,
    Type Type,
    object Default = null,
    string Description = null,
    bool CanBeCached = true);