using System;

namespace LoschScript.Meta;

internal class FileContext
{
    public FileContext(string path)
    {
        Path = path;
        AvailableTypes = new(path);
    }

    public TypeRegistry AvailableTypes { get; }

    public string Path { get; }

    public bool CheckType(Type type) => AvailableTypes.Contains(type);
}