using System;

namespace Dassie.Core;

/// <summary>
/// Declares a function to be a CLR global function contained in the special <c>&lt;Module&gt;</c> type.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ClrGlobalFunctionAttribute : Attribute { }