using Dassie.Configuration;
using System.Collections.Generic;

namespace Dassie.Extensions;

/// <summary>
/// Represents the environment in which a compiler extension is loaded.
/// </summary>
public interface IEnvironmentInfo
{
    /// <summary>
    /// Retrieves the current compiler configuration.
    /// </summary>
    /// <returns>An instance of <see cref="DassieConfig"/> representing the currently enabled configuration at the time of the call, or <see langword="null"/> if no configuration is set.</returns>
    public DassieConfig Configuration();

    /// <summary>
    /// Retrieves the currently loaded extension packages.
    /// </summary>
    /// <returns>An enumerable of compiler extensions that are currently loaded.</returns>
    public IEnumerable<IPackage> InstalledExtensions();
}