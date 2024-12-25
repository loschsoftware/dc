using System;

namespace Dassie.Core;

/// <summary>
/// Marker attribute to declare a type as an enumeration.
/// </summary>
/// <typeparam name="T">The type of the enumeration members.</typeparam>
[AttributeUsage(AttributeTargets.Class)]
public class Enumeration<T> : Attribute { }

/// <summary>
/// Marker attribute to declare a type as an Int32 enumeration.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class Enumeration : Attribute { }