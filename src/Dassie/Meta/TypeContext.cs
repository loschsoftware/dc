using Dassie.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public static TypeContext GetForType(TypeBuilder tb)
    {
        if (!Context.Types.Any(t => t.Builder == tb))
            return null;

        return Context.Types.First(t => t.Builder == tb);
    }

    public MethodContext GetMethod(DassieParser.Type_memberContext member)
        => Methods.FirstOrDefault(m => m.ParserRule == member);

    public MetaFieldInfo GetField(DassieParser.Type_memberContext member)
        => Fields.FirstOrDefault(m => m.ParserRule == member);

    public static TypeContext Current { get; set; }

    public List<(FieldBuilder Field, DassieParser.ExpressionContext Value)> FieldInitializers { get; set; } = [];

    public List<DassieParser.Type_memberContext> Constructors { get; set; } = [];

    public string FullName { get; set; }

    public DassieParser.TypeContext ParserRule { get; set; }

    public List<string> FilesWhereDefined { get; } = [];

    public TypeBuilder Builder { get; set; }

    public Type FinishedType { get; set; } = null;

    public List<MethodContext> Methods { get; set; } = [];

    public List<MetaFieldInfo> Fields { get; } = [];

    public List<PropertyBuilder> Properties { get; set; } = [];

    public List<MethodContext> ConstructorContexts { get; } = [];

    public List<TypeContext> Children { get; } = [];

    public List<GenericParameterContext> GenericParameters { get; set; } = [];

    public bool IsDependentType => GenericParameters.Any(p => p.IsRuntimeValue || p.IsCompileTimeConstant);

    public List<Type> ImplementedInterfaces { get; } = [];

    public bool IsEnumeration { get; set; }
    public Type EnumerationBaseType { get; set; }

    public bool IsByRefLike { get; set; }

    public bool IsImmutable { get; set; }
    
    public bool IsUnion { get; set; }

    public List<MockMethodInfo> RequiredInterfaceImplementations { get; set; } = [];

    public bool ContainsCustomOperators { get; set; }

    public List<(ConstructorInfo Constructor, object[] Data)> Attributes { get; set; } = [];

    public bool IsAlias { get; set; }

    public Type AliasedType { get; set; }

    public bool IsNewType { get; set; }

    public List<DassieParser.Type_nameContext> UnresolvedAssociatedTypeNames { get; set; } = [];

    public DassieParser.Parameter_listContext PrimaryConstructorParameterList { get; set; }

    public bool IsLocalType { get; set; }
}