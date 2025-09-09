using Dassie.Configuration;
using System;
using System.Collections.Generic;

namespace Dassie.Extensions;

/// <summary>
/// Provides a default implementation of <see cref="IEnvironmentInfo"/>.
/// </summary>
internal class CompilerEnvironmentInfo : IEnvironmentInfo
{
    internal Func<DassieConfig> ConfigurationFunc { get; set; } = () => null;
    internal Func<IEnumerable<IPackage>> ExtensionsFunc { get; set; } = () => [];

    /// <inheritdoc/>
    public DassieConfig Configuration() => ConfigurationFunc();

    /// <inheritdoc/>
    public IEnumerable<IPackage> InstalledExtensions() => ExtensionsFunc();
}