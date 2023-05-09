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
    
    public static FileContext CurrentFile { get; set; }

    public TypeRegistry AvailableTypes { get; }

    public string Path { get; }

    public List<string> Imports { get; } = new()
    {
        // Implicit imports
        "LoschScript.Core"
    };
    
    public List<string> ImportedTypes { get; } = new();

    public List<(string Name, string Alias)> Aliases { get; } = new();

    public string ExportedNamespace { get; set; }

    public bool CompilationFailed { get; set; }

    public List<ErrorInfo> Errors { get; } = new();

    public bool CheckType(Type type) => AvailableTypes.Contains(type);
}