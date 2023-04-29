using System.Collections.Generic;

namespace LoschScript.Meta;

internal class TypeContext
{
    public string FullName { get; set; }

    public List<string> FilesWhereDefined { get; } = new();
}