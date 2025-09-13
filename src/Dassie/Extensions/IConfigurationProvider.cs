using Dassie.Configuration;

namespace Dassie.Extensions;

/// <summary>
/// Defines a mechanism to pass configuration data to the compiler without using project files (dsconfig.xml).
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// The name of the configuration provider.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The project configuration encapsulated by this configuration provider.
    /// </summary>
    public DassieConfig Configuration { get; }
}