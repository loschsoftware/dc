using System.Collections.Generic;
using System.Reflection.Emit;

namespace LoschScript.Meta;

internal class MethodContext
{
    public MethodContext()
    {
        CurrentMethod = this;
    }

    public static MethodContext CurrentMethod { get; set; }

    public ILGenerator IL { get; }

    public List<string> FilesWhereDefined { get; } = new();

    public MethodBuilder Builder { get; set; }
}
