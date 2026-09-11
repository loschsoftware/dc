using System.Collections.Generic;

namespace Dassie.Extensions.Pack.Resources;

internal class Strings_deDE : IResourceProvider<string>
{
    public string Culture => "de-DE";

    private readonly Dictionary<string, string> _resources = new()
    {
        ["PackExtension_Description"] = "Stellt Werkzeuge zur Erstellung von .dcx-Erweiterungspaketen bereit.",
        ["PackCommand_Description"] = "Erstellt und verpackt .dcx-Erweiterungspakete."
    };

    public Dictionary<string, string> Resources => _resources;
}