using Dassie.Extensions;
using System;
using System.Collections.Generic;

namespace Dassie.Core.Macros;

internal class PropMacro : IMacro
{
    private static PropMacro _instance;
    public static PropMacro Instance => _instance ??= new();

    public string Name => "Prop";

    private readonly List<MacroParameter> _params = [new("PropertyName")];
    public List<MacroParameter> Parameters => _params;

    public string Expand(Dictionary<string, string> arguments)
    {
        return "";
    }
}