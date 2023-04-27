using System;
using System.Collections.Generic;

namespace LoschScript.Meta;

internal class TypeRegistry
{
    public TypeRegistry(string file)
    {
        File = file;
    }

    public string File { get; }

    public List<Type> AvailableTypes { get; } = new();

    public bool Contains(Type type) => AvailableTypes.Contains(type);
}