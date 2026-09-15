using System;

namespace Dassie.Cli;

internal static class EnvironmentVariableHelpers
{
    public static bool GetBoolEnvVar(string name)
    {
        return Environment.GetEnvironmentVariable(name) is string val && ToBool(val);
    }

    static bool ToBool(string str)
    {
        if (bool.TryParse(str, out bool b))
            return b;

        if (int.TryParse(str, out int i))
            return i > 0;

        return false;
    }
}