using Dassie.Configuration;
using Dassie.Configuration.Global;
using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <inheritdoc/>
    public Dictionary<string, object> GlobalConfiguration()
    {
        return GlobalConfigManager.Properties.Select(t => new KeyValuePair<string, object>(t.Key, t.Value)).ToDictionary();
    }
}