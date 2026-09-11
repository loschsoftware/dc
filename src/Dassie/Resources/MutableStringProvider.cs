using Dassie.Extensions;
using System.Collections.Generic;

namespace Dassie.Resources;

internal class MutableStringProvider : IResourceProvider<string>
{
    public MutableStringProvider(IResourceProvider<string> baseProvider)
    {
        _culture = baseProvider.Culture;
        _resources = baseProvider.Resources;
    }

    private readonly string _culture;
    public string Culture => _culture;

    private readonly Dictionary<string, string> _resources;
    public Dictionary<string, string> Resources => _resources;

    public void Merge(IResourceProvider<string> provider)
    {
        // Add proper validation for culture and duplicate keys later, if it proves necessary

        foreach (KeyValuePair<string, string> res in provider.Resources)
        {
            if (_resources.ContainsKey(res.Key))
                continue;

            _resources.Add(res.Key, res.Value);
        }
    }
}