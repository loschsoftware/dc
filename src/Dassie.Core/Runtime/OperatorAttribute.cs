using System;

namespace Dassie.Runtime;

/// <summary>
/// Specifies that a method is a custom operator.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class OperatorAttribute : Attribute { }

/// <summary>
/// Specifies that a module contains custom operators.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ContainsCustomOperatorsAttribute : Attribute { }