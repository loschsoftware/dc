using Dassie.Core.Properties;
using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dassie.Resources;

/// <summary>
/// Manages and provides string resources.
/// </summary>
internal static partial class StringHelper
{
    private static IResourceProvider<string> _globalProvider;
    private static readonly Dictionary<string, IResourceProvider<string>> _localProviders = [];

    /// <summary>
    /// Initializes the string registry for the current language.
    /// </summary>
    public static void Initialize()
    {
        string languageName = "en-US";

        if (LanguageProperty.Instance.IsRegistered)
            languageName = (string)LanguageProperty.Instance.GetValue();

        if (ExtensionLoader.LocalizationResourceProviders.Any(p => p.Culture == languageName))
        {
            foreach (IResourceProvider<string> provider in ExtensionLoader.LocalizationResourceProviders.Where(p => p.Culture == languageName))
            {
                if (provider.Scope == ResourceScope.Global)
                {
                    AddGlobalProvider(provider);
                    continue;
                }

                IExtension declaringExtension = ExtensionLoader.InstalledExtensions.First(e => e.LocalizationResourceProviders()?.Contains(provider) == true);
                AddLocalProvider(declaringExtension.Metadata?.PackageIdentity, provider);
            }

            if (_globalProvider != null)
                return;
        }

        void NotFound()
        {
            if (languageName != "en-US")
            {
                EmitWarningMessageFormatted(
                    0, 0, 0,
                    DS0268_LanguageNotFound,
                    nameof(StringHelper_LanguageNotFound), [languageName],
                    CompilerExecutableName);
            }

            _globalProvider = DefaultStrings.Instance;
        }

        string probeDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Localization");

        if (!Directory.Exists(probeDir))
        {
            NotFound();
            return;
        }

        string jsonPath = Path.Combine(probeDir, $"strings.{languageName}.json");

        if (!File.Exists(jsonPath))
        {
            NotFound();
            return;
        }

        SetStringSource(jsonPath);
    }

    /// <summary>
    /// Sets the current string source based on a resource provider.
    /// </summary>
    /// <param name="resourceProvider">The resource provider acting as the source of localized strings.</param>
    public static void SetStringSource(IResourceProvider<string> resourceProvider)
    {
        _globalProvider = resourceProvider;
    }

    private static void AddGlobalProvider(IResourceProvider<string> provider)
    {
        if (_globalProvider == null)
        {
            SetStringSource(provider);
            return;
        }

        if (_globalProvider is MutableStringProvider msp)
        {
            msp.Merge(provider);
            return;
        }

        _globalProvider = new MutableStringProvider(_globalProvider);
        AddGlobalProvider(provider);
    }

    private static void AddLocalProvider(string id, IResourceProvider<string> provider)
    {
        if (_localProviders.TryAdd(id, provider))
            return;

        IResourceProvider<string> existingProvider = _localProviders[id];

        if (existingProvider is MutableStringProvider msp)
        {
            msp.Merge(provider);
            return;
        }

        _localProviders[id] = new MutableStringProvider(existingProvider);
        AddLocalProvider(id, provider);
    }

    /// <summary>
    /// Sets the current string source based on a JSON file.
    /// </summary>
    /// <param name="jsonFile">The path to a JSON file containing localized string resources.</param>
    public static void SetStringSource(string jsonFile)
    {
        _globalProvider = new JsonStringProvider(jsonFile);
    }

    /// <summary>
    /// Retrieves a string resource.
    /// </summary>
    /// <param name="id">The resource key to look up.</param>
    /// <returns>The localized string whose key is equal to <paramref name="id"/>.</returns>
    public static string GetString(string id)
    {
        if (_globalProvider == null || _globalProvider.Resources == null || !_globalProvider.Resources.TryGetValue(id, out string str))
        {
            if (DefaultStrings.Instance.Resources.TryGetValue(id, out string defaultStr))
                return defaultStr;

            return id;
        }

        return str;
    }

    private static string StringFormat(string str, params object[] args)
    {
        if (args == null || args.Length == 0)
            return str;

        return string.Format(str, args);
    }

    /// <summary>
    /// Formats a localized string.
    /// </summary>
    /// <param name="id">The key of the string to format.</param>
    /// <param name="args">The arguments used for formatting.</param>
    /// <returns>The formatted localized string whose key is equal to <paramref name="id"/>.</returns>
    public static string Format(string id, params object[] args)
    {
        string str = GetString(id);
        return StringFormat(str, args);
    }

    public static string GetStringLocal(string package, string id)
    {
        if (_localProviders.TryGetValue(package, out IResourceProvider<string> provider))
        {
            if (provider.Resources.TryGetValue(id, out string str))
                return str;
        }

        return GetString(id);
    }

    public static string FormatLocal(string package, string id, params object[] args)
    {
        string str = GetStringLocal(package, id);
        return StringFormat(str, args);
    }
}