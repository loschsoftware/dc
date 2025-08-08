using System;

namespace Dassie.Core;

/// <summary>
/// Marks a function as a predicate for a type constraint.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class PredicateAttribute : Attribute { }