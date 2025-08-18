using System;

namespace Dassie.Tests;

/// <summary>
/// Marks a function as a unit test.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestAttribute : Attribute { }