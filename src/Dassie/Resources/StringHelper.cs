using Dassie.Extensions;

namespace Dassie.Resources;

internal static class StringHelper
{
    private static IResourceProvider<string> _provider;

    public static void SetStringSource(IResourceProvider<string> resourceProvider)
    {
        _provider = resourceProvider;
    }

    public static string GetString(string id)
    {
        if (_provider.Resources.TryGetValue(id, out string str))
            return str;

        return id;
    }

    public static string Format(string id, params object[] args)
    {
        string str = GetString(id);

        if (args == null || args.Length == 0)
            return str;

        return string.Format(str, args);
    }
}