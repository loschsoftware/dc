using System.Collections.Generic;
using System.Reflection.Emit;

namespace LoschScript.Meta;

internal class MethodContext
{
    public MethodContext()
    {
        Current = this;
    }

    public static MethodContext Current { get; set; }

    public List<string> FilesWhereDefined { get; } = new();

    public MethodBuilder Builder { get; set; }
}
