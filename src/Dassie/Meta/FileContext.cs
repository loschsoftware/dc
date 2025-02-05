using Antlr4.Runtime;
using Dassie.Errors;
using Dassie.Parser;
using Dassie.Text;
using Dassie.Text.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Reflection.Emit;

namespace Dassie.Meta;

internal class FileContext
{
    public FileContext(string path)
    {
        Path = path;
        AvailableTypes = new(path);
    }

    public int Index => Context.Files.IndexOf(this);

    public static FileContext CurrentFile { get; set; }

    public TypeRegistry AvailableTypes { get; }

    public string Path { get; }

    public List<Fragment> Fragments { get; } = new();

    public List<string> Imports { get; } = new()
    {
        // Implicit imports
        "Dassie.Core"
    };

    public List<string> ImportedTypes { get; } = new()
    {
        // Implicitly imported types ("built-in functions")
        "Dassie.Core.Assert",
        "Dassie.Core.stdout",
        "Dassie.Core.stdin",
        "Dassie.Core.Value",
        "Dassie.Core.Process",
        "Dassie.Core.Numerics.NumericSequence",
        "Dassie.CompilerServices.CodeGeneration"
    };

    public List<(string Name, string Alias)> Aliases { get; } = [];

    public string ExportedNamespace { get; set; }

    public bool CompilationFailed { get; set; }

    public List<ErrorInfo> Errors { get; } = new();

    public List<FoldingRegion> FoldingRegions { get; } = new();

    public List<GuideLine> GuideLines { get; } = new();

    public bool CheckType(Type type) => AvailableTypes.Contains(type);

    public Dictionary<string, Dictionary<string, string>> FunctionParameterConstraints { get; } = [];

    public ISymbolDocumentWriter SymbolDocumentWriter { get; set; }

    public ICharStream CharStream { get; set; }

    public TypeBuilder LocalTopLevelFunctionContainerType { get; set; }
    public List<DassieParser.Type_memberContext> LocalTopLevelFunctions { get; set; } = [];
}