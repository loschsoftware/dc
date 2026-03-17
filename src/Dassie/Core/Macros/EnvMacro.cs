using Dassie.Extensions;
using System;
using System.Collections.Generic;

namespace Dassie.Core.Macros;

internal class EnvMacro : IMacro
{
    private static EnvMacro _instance;
    public static EnvMacro Instance => _instance ??= new();

    public string Name => "Env";

    private readonly List<MacroParameter> _params = [new("Variable")];
    public List<MacroParameter> Parameters => _params;

    public string Expand(Dictionary<string, string> arguments)
    {
        return Environment.GetEnvironmentVariable(arguments["Variable"].Trim());
    }
}