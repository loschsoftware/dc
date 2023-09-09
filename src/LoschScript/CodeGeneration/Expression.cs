using System;

namespace LoschScript.CodeGeneration;

internal class Expression
{
    public Expression(Type type, dynamic value)
    {
        Type = type;
        Value = value;
    }

    public Expression() { }

    public Type Type { get; set; }

    public dynamic Value { get; set; }
}