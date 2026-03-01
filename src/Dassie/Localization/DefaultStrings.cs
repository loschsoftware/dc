using System.Collections.Generic;

namespace Dassie.Localization;

internal static class DefaultStrings
{
    public const string DefaultLanguage = "en-US";

    public static Dictionary<string, string> Strings { get; } = new()
    {
        ["A"] = "B"
    };
}