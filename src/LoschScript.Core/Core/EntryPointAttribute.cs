using System;

namespace LoschScript.Core;

/// <summary>
/// Marks the entry point of an assembly, which is most commonly a function called Main.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class EntryPointAttribute : Attribute { }