using System;

namespace Dassie.Core;

/// <summary>
/// Marks a field as an auto-implemented property.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class AutoAttribute : Attribute { }