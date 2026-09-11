using System.Collections.Generic;

namespace Dassie.Extensions.Pack.Resources;

internal class Strings_enUS : IResourceProvider<string>
{
    public string Culture => "en-US";

    private readonly Dictionary<string, string> _resources = new()
    {
        ["PackExtension_Description"] = "Provides tools for building and packing .dcx extension packages.",
        ["PackCommand_Description"] = "Builds and packs .dcx extension packages.",
    };

    public Dictionary<string, string> Resources => _resources;
}