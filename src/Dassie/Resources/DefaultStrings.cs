using Dassie.Extensions;
using System.Collections.Generic;

namespace Dassie.Resources;

internal class DefaultStrings : IResourceProvider<string>
{
    private static DefaultStrings _instance;
    public static DefaultStrings Instance => _instance ??= new();

    public Dictionary<string, string> Resources => new()
    {
        // TODO: Fill this out at some point
    };
}