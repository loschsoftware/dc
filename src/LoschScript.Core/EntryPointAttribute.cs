using System;

namespace LoschScript.Core;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class EntryPointAttribute : Attribute { }