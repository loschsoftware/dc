using System;

namespace Dassie.Core.Meta;

/// <summary>
/// Marks a function as constant. Constant functions are evaluated at compile time.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ConstantAttribute : Attribute { }