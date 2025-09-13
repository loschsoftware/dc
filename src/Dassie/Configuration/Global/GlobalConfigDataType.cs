namespace Dassie.Configuration.Global;

/// <summary>
/// Represents the data type of a global configuration property.
/// </summary>
/// <param name="BaseType">The data type of the property, or the element type of a list.</param>
/// <param name="IsList">Wheter or not the data type represents a list.</param>
public record GlobalConfigDataType(
    GlobalConfigBaseType BaseType,
    bool IsList);