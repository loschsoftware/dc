using Dassie.Core.Properties;
using Dassie.Extensions;
using System;
using System.Linq;

namespace Dassie.Resources;

internal static partial class StringHelper
{
    private static IResourceProvider<string> _provider;

    public static void Initialize()
    {
        string languageName = (string)LanguageProperty.Instance.GetValue();

        if (ExtensionLoader.LocalizationResourceProviders.Any(p => p.Culture == languageName))
        {
            SetStringSource(ExtensionLoader.LocalizationResourceProviders.First(p => p.Culture == languageName));
            return;
        }

        void NotFound()
        {
            EmitWarningMessageFormatted(
                0, 0, 0,
                DS0268_LanguageNotFound,
                nameof(StringHelper_LanguageNotFound), [languageName],
                CompilerExecutableName);

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

    public static void SetStringSource(IResourceProvider<string> resourceProvider)
    {
        _provider = resourceProvider;
    }

    public static void SetStringSource(string jsonFile)
    {
        _provider = new JsonStringProvider(jsonFile);
    }

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

    public static string Format(string id, params object[] args)
    {
        string str = GetString(id);

        if (args == null || args.Length == 0)
            return str;

        return string.Format(str, args);
    }
}