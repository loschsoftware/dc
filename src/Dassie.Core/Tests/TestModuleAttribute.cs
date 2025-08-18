using System;

namespace Dassie.Tests;

/// <summary>
/// Marks a module that acts as a container for unit tests.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TestModuleAttribute : Attribute { }