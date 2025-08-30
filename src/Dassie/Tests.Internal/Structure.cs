using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dassie.Tests.Internal;

internal class TestProject
{
    public string Name { get; set; }
    public Assembly Assembly { get; set; }
    public List<TestModule> Modules { get; set; }

    public Status Status => TestHelpers.GetStatus(Modules.Select(c => c.Status));
}

internal class TestModule
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public List<Test> Tests { get; set; }

    public Status Status => TestHelpers.GetStatus(Tests.Select(c => c.Status));
}

internal class Test
{
    public string Name { get; set; }
    public MethodInfo Method { get; set; }
    public List<TestCase> Cases { get; set; }

    private Status? _status = null;
    public Status Status
    {
        get => _status ?? TestHelpers.GetStatus(Cases.Select(c => c.Status));
        set => _status = value;
    }
}

internal class TestCase
{
    public string Parameters { get; set; }
    public Status Status { get; set; }
}

internal enum Status
{
    Passed,
    Failed,
    InProgress,
    Todo
}