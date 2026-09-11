using System.Collections.Generic;

namespace Dassie.Resources;

/// <summary>
/// Provides localization data for extensions.
/// </summary>
public class LocalizationProvider
{
    internal readonly string _packageId;
    
    internal LocalizationProvider(string packageId)
    {
        _packageId = packageId;
    }

    internal static Dictionary<string, LocalizationProvider> _providers = [];

    /// <summary>
    /// Creates or retrieves an instance of <see cref="LocalizationProvider"/> to provide localization
    /// data from the specified extension package.
    /// </summary>
    /// <param name="packageId">The ID of the extension package providing the localization resources.</param>
    /// <returns>An instance of <see cref="LocalizationProvider"/> for the specified packag.e</returns>
    public static LocalizationProvider ForPackage(string packageId)
    {
        if (_providers.TryGetValue(packageId, out LocalizationProvider provider))
            return provider;

        provider = new(packageId);
        _providers.Add(packageId, provider);
        return provider;
    }

    /// <summary>
    /// Retrieves a string resource.
    /// </summary>
    /// <param name="id">The resource key to look up.</param>
    /// <returns>The localized string whose local key is equal to <paramref name="id"/>.</returns>
    public string GetString(string id)
    {
        return StringHelper.GetStringLocal(_packageId, id);
    }

    /// <summary>
    /// Formats a localized string.
    /// </summary>
    /// <param name="id">The key of the string to format.</param>
    /// <param name="args">The arguments used for formatting.</param>
    /// <returns>The formatted localized string whose local key is equal to <paramref name="id"/>.</returns>
    public string Format(string id, params object[] args)
    {
        return StringHelper.FormatLocal(_packageId, id, args);
    }

    /// <summary>
    /// Retrieves a string resource from the global resource provider.
    /// </summary>
    /// <param name="id">The resource key to look up.</param>
    /// <returns>The localized string whose local key is equal to <paramref name="id"/>.</returns>
    public static string GetStringGlobal(string id)
    {
        return StringHelper.GetString(id);
    }

    /// <summary>
    /// Formats a localized string from the global resource provider.
    /// </summary>
    /// <param name="id">The key of the string to format.</param>
    /// <param name="args">The arguments used for formatting.</param>
    /// <returns>The formatted localized string whose local key is equal to <paramref name="id"/>.</returns>
    public static string FormatGlobal(string id, params object[] args)
    {
        return StringHelper.Format(id, args);
    }
}