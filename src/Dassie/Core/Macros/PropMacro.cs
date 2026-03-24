using Dassie.Configuration;
using Dassie.Extensions;
using System.Collections.Generic;

namespace Dassie.Core.Macros;

internal class PropMacro(DassieConfig config) : IMacro
{
    public string Name => "Prop";

    private readonly List<MacroParameter> _params = [new("PropertyName", true)];
    public List<MacroParameter> Parameters => _params;

    public MacroOptions Options => MacroOptions.None;

    public string Expand(Dictionary<string, string> arguments)
    {
        return Value.formatm(config[arguments["PropertyName"]]) ?? "";
    }
}