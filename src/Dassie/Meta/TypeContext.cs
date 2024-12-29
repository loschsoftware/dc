using Dassie.Parser;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Dassie.Meta;

internal class TypeContext
{
    public TypeContext()
    {
        Current = this;

        if (!Context.Types.Contains(this))
            Context.Types.Add(this);
    }

    public static TypeContext Current { get; set; }

    public List<(FieldBuilder Field, DassieParser.ExpressionContext Value)> FieldInitializers { get; set; } = [];

    public List<DassieParser.Type_memberContext> Constructors { get; set; } = [];

    public string FullName { get; set; }

    public List<string> FilesWhereDefined { get; } = [];

    public TypeBuilder Builder { get; set; }

    public List<MethodContext> Methods { get; } = [];

    public List<MetaFieldInfo> Fields { get; } = [];

    public List<PropertyBuilder> Properties { get; } = [];

    public List<MethodContext> ConstructorContexts { get; } = [];

    public List<TypeContext> Children { get; } = [];

    public List<TypeParameterContext> TypeParameters { get; set; } = [];

    public List<Type> ImplementedInterfaces { get; } = [];

    public bool IsEnumeration { get; set; }

    public bool IsByRefLike { get; set; }

    public bool IsImmutable { get; set; }

    public List<MethodInfo> RequiredInterfaceImplementations { get; set; } = [];
}