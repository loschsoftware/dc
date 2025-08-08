using Dassie.Parser;
using System;
using System.Collections.Generic;

namespace Dassie.Meta;

/// <summary>
/// Represents a type in the Dassie type system.
/// </summary>
internal class TypeSymbol
{
    /// <summary>
    /// The underlying .NET type.
    /// </summary>
    public Type RawType { get; set; }

    /// <summary>
    /// A list of dependent values.
    /// </summary>
    public List<(TypeSymbol Type, object Value)> Dependencies { get; set; } = [];

    /// <summary>
    /// An optional value constraint.
    /// </summary>
    public DassieParser.PredicateContext Predicate { get; set; }

    public string UnresolvedMethodTypeParameterName { get; set; }
    public string UnresolvedTypeParameterName { get; set; }

    public static implicit operator Type(TypeSymbol type) => type.RawType;
    public static implicit operator TypeSymbol(Type type) => new() { RawType = type };
}