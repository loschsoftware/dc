using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dassie.Meta;

internal class MockMethodInfo
{
    public Type DeclaringType { get; set; }
    public string Name { get; set; }
    public Type ReturnType { get; set; }
    public List<Type> Parameters { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsGenericMethod { get; set; }
    public List<Type> GenericTypeArguments { get; set; }
    public MethodInfo Builder { get; set; }
}