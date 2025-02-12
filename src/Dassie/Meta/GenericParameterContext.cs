using Dassie.Parser;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Dassie.Meta;

internal class GenericParameterContext
{
    public string Name { get; set; }
    public GenericTypeParameterBuilder Builder { get; set; }
    public GenericParameterAttributes Attributes { get; set; }
    public Type BaseTypeConstraint { get; set; }
    public List<Type> InterfaceConstraints { get; set; } = [];

    public bool IsCompileTimeConstant { get; set; }
    public bool IsRuntimeValue { get; set; }
    public Type ValueType { get; set; }
    public DassieParser.Parameter_constraintContext ValueConstraint { get; set; }
}