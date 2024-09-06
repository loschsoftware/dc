using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Dassie.Meta;

internal class TypeParameterContext
{
    public string Name { get; set; }
    public GenericTypeParameterBuilder Builder { get; set; }
    public GenericParameterAttributes Attributes { get; set; }
    public Type BaseTypeConstraint { get; set; }
    public List<Type> InterfaceConstraints { get; set; } = [];
}