using Dassie.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Dassie.Meta;

internal class MetaFieldInfo : IEquatable<MetaFieldInfo>
{
    public MetaFieldInfo(string name, FieldBuilder builder, UnionValue union)
    {
        Name = name;
        Builder = builder;
        Union = union;
    }

    public MetaFieldInfo() { }

    public string Name { get; set; }

    public FieldInfo Builder { get; set; }

    public UnionValue Union { get; set; }

    public object ConstantValue { get; set; } = null;

    public bool IsFunctionPointer { get; set; }

    public MethodInfo FunctionPointerTarget { get; set; }

    public override bool Equals(object obj)
    {
        return Equals(obj as MetaFieldInfo);
    }

    public bool Equals(MetaFieldInfo other)
    {
        return other is not null &&
               Name == other.Name &&
               EqualityComparer<FieldInfo>.Default.Equals(Builder, other.Builder) &&
               Union.Equals(other.Union);
    }

    public override int GetHashCode()
    {
        int hashCode = 1355297754;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + EqualityComparer<FieldInfo>.Default.GetHashCode(Builder);
        hashCode = hashCode * -1521134295 + Union.GetHashCode();
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