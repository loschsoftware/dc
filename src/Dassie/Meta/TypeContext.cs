using Dassie.Parser;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Dassie.Meta;

internal class TypeContext
{
    public TypeContext()
    {
        Current = this;
    }

    public static TypeContext Current { get; set; }

    public List<(FieldBuilder Field, DassieParser.ExpressionContext Value)> FieldInitializers { get; set; } = new();

    public List<DassieParser.Type_memberContext> Constructors { get; set; } = new();

    public string FullName { get; set; }

    public List<string> FilesWhereDefined { get; } = new();

    public TypeBuilder Builder { get; set; }

    public List<MethodContext> Methods { get; } = new();

    public List<MetaFieldInfo> Fields { get; } = new();

    public List<TypeContext> Children { get; } = new();
}