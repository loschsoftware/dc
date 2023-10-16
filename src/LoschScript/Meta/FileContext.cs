﻿using LoschScript.Errors;
using LoschScript.Text;
using LoschScript.Text.Regions;
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

    public List<Fragment> Fragments { get; } = new();

    public List<string> Imports { get; } = new()
    {
        // Implicit imports
        "LoschScript.Core"
    };

    public List<string> ImportedTypes { get; } = new()
    {
        // Implicitly imported types ("built-in functions")
        "LoschScript.Core.stdout",
        "LoschScript.Core.stdin",
        "LoschScript.Core.Value",
        "LoschScript.Core.Numerics.NumericSequence",
        "LoschScript.CompilerServices.CodeGeneration"
    };

    public List<(string Name, string Alias)> Aliases { get; } = new()
    {
        ("System.SByte", "int8"),
        ("System.Byte", "uint8"),
        ("System.Int16", "int16"),
        ("System.UInt16", "uint16"),
        ("System.Int32", "int32"),
        ("System.Int32", "int"),
        ("System.UInt32", "uint32"),
        ("System.UInt32", "uint"),
        ("System.Int64", "int64"),
        ("System.UInt64", "uint64"),
        ("System.Single", "float32"),
        ("System.Double", "float64"),
        ("System.Decimal", "decimal"),
        ("System.IntPtr", "native"),
        ("System.UIntPtr", "unative"),
        ("System.Boolean", "bool"),
        ("System.String", "string"),
        ("System.Char", "char"),
        ("System.Void", "null")
    };

    public string ExportedNamespace { get; set; }

    public bool CompilationFailed { get; set; }

    public List<ErrorInfo> Errors { get; } = new();

    public List<FoldingRegion> FoldingRegions { get; } = new();
    
    public List<GuideLine> GuideLines { get; } = new();

    public bool CheckType(Type type) => AvailableTypes.Contains(type);
}