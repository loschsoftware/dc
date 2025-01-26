using System;

namespace Dassie.Core;

/// <summary>
/// Marks a method as pure.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PureAttribute : Attribute { }