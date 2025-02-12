using Dassie.Parser;
using Dassie.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Dassie.Meta;

internal class MetaFieldInfo : IEquatable<MetaFieldInfo>
{
    public MetaFieldInfo(string name, FieldBuilder builder)
    {
        Name = name;
        Builder = builder;
    }

    public MetaFieldInfo() { }

    public string Name { get; set; }

    public FieldInfo Builder { get; set; }

    public object ConstantValue { get; set; } = null;

    public DassieParser.Type_memberContext ParserRule { get; set; }

    public List<Attribute> Attributes { get; set; } = [];

    public override bool Equals(object obj)
    {
        return Equals(obj as MetaFieldInfo);
    }

    public bool Equals(MetaFieldInfo other)
    {
        return other is not null &&
               Name == other.Name &&
               EqualityComparer<FieldInfo>.Default.Equals(Builder, other.Builder);
    }

    public override int GetHashCode()
    {
        int hashCode = 1355297754;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + EqualityComparer<FieldInfo>.Default.GetHashCode(Builder);
        return hashCode;
    }

    public static bool operator ==(MetaFieldInfo left, MetaFieldInfo right)
    {
        return EqualityComparer<MetaFieldInfo>.Default.Equals(left, right);
    }

    public static bool operator !=(MetaFieldInfo left, MetaFieldInfo right)
    {
        return !(left == right);
    }
}