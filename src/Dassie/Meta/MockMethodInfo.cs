using System;
using System.Collections.Generic;

namespace Dassie.Meta;

// Required because MSFT devs are too incompetent to support
// TypeBuilder.GetMethods() on constructed generic types...

internal class MockMethodInfo
{
    public Type DeclaringType { get; set; }
    public string Name { get; set; }
    public Type ReturnType { get; set; }
    public List<Type> Parameters { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsGenericMethod { get; set; }
    public List<Type> GenericTypeArguments { get; set; }
}