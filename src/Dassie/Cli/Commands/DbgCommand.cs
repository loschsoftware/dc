using Dassie.Extensions;
using System;

namespace Dassie.Cli.Commands;

internal class DbgCommand : ICompilerCommand
{
    private static DbgCommand _instance;
    public static DbgCommand Instance => _instance ??= new();

    public string Command => "dbg";

    public string UsageString => "dbg [Command]";

    public string Description => "Commands used for debugging and testing the compiler. To be used by compiler developers.";

    public bool Hidden() => true;

    public int Invoke(string[] args)
    {
        if (args == null || args.Length == 0)
            return 0;

        if (args[0] == "fail")
            throw new Exception("dbg fail");

        return 0;
    }
}