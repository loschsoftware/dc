using System;

namespace Dassie.Core;

/// <summary>
/// Marker attribute to declare a type as a union.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class Union : Attribute { }