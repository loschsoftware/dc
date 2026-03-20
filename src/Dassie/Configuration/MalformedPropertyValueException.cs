using System;

namespace Dassie.Configuration;

/// <summary>
/// Represents an exception caused by an invalid property value.
/// </summary>
/// <param name="propertyName">The name of the property causing the exception.</param>
/// <param name="innerException">The inner exception.</param>
public class MalformedPropertyValueException(string propertyName, Exception innerException)
    : Exception($"Malformed property value for '{propertyName}'.", innerException)
{
    /// <summary>
    /// The name of the property causing the exception.
    /// </summary>
    public string PropertyName { get; } = propertyName;
}