using LoschScript.Errors;
using System;
using System.Collections.Generic;

namespace LoschScript.Meta;

internal class FileContext
{
    public FileContext(string path)
    {
        Path = path;
        AvailableTypes = new(path);
    }

    public static FileContext CurrentContext { get; set; }

    public TypeRegistry AvailableTypes { get; }

    public string Path { get; }

    public bool CompilationFailed { get; set; }

    public List<ErrorInfo> Errors { get; } = new();

    public bool CheckType(Type type) => AvailableTypes.Contains(type);
}