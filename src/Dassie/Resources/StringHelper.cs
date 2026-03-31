using Dassie.Core.Properties;
using Dassie.Extensions;
using System;
using System.Linq;

namespace Dassie.Resources;

/// <summary>
/// Manages and provides string resources.
/// </summary>
internal static partial class StringHelper
{
    private static IResourceProvider<string> _provider;

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
            SetStringSource(ExtensionLoader.LocalizationResourceProviders.First(p => p.Culture == languageName));
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

            _provider = DefaultStrings.Instance;
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
        _provider = resourceProvider;
    }

    /// <summary>
    /// Sets the current string source based on a JSON file.
    /// </summary>
    /// <param name="jsonFile">The path to a JSON file containing localized string resources.</param>
    public static void SetStringSource(string jsonFile)
    {
        _provider = new JsonStringProvider(jsonFile);
    }

    /// <summary>
    /// Retrieves a string resource.
    /// </summary>
    /// <param name="id">The resource key to look up.</param>
    /// <returns>The localized string whose key is equal to <paramref name="id"/>.</returns>
    public static string GetString(string id)
    {
        if (_provider == null || _provider.Resources == null || !_provider.Resources.TryGetValue(id, out string str))
        {
            if (DefaultStrings.Instance.Resources.TryGetValue(id, out string defaultStr))
                return defaultStr;

            return id;
        }

        return str;
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

        if (args == null || args.Length == 0)
            return str;

        return string.Format(str, args);
    }
}