using System;

namespace LoschScript.Core;

/// <summary>
/// Specifies the entry point of an assembly, which is usually a function called '<c>Main</c>'.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class EntryPointAttribute : Attribute { }