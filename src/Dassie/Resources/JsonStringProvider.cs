using Dassie.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Dassie.Resources;

internal class JsonStringProvider : IResourceProvider<string>
{
    public JsonStringProvider(string jsonFile)
    {
        if (Path.GetExtension(Path.GetFileNameWithoutExtension(jsonFile)) is string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName))
                cultureName = "en-US";

            _culture = cultureName[1..];
        }
        else
            _culture = new("en-US");

        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(jsonFile));

        List<(string, string)> resourceEntries = doc.RootElement
            .EnumerateObject()
            .Select(p => (p.Name, p.Value.ToString()))
            .ToList();

        foreach ((string key, string value) in resourceEntries)
            _strings.Add(key, value);
    }

    private readonly Dictionary<string, string> _strings = [];
    private readonly string _culture;

    public Dictionary<string, string> Resources => _strings;
    public string Culture => _culture;
}