using System;

namespace Dassie.Core;

/// <summary>
/// Marks a field as an event.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class EventAttribute : Attribute { }