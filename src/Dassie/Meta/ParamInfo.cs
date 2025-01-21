using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Dassie.Meta;

internal class ParamInfo : IEquatable<ParamInfo>
{
    public ParamInfo(string name, Type type, ParameterBuilder builder, int index, bool isMutable)
    {
        Name = name;
        Type = type;
        Builder = builder;
        Index = index;
        IsMutable = isMutable;
    }

    public ParamInfo() { }

    public string Name { get; set; }

    public bool IsMutable { get; set; }

    public Type Type { get; set; }

    public ParameterBuilder Builder { get; set; }

    public int Index { get; set; }

    public override bool Equals(object obj)
    {
        return Equals(obj as ParamInfo);
    }

    public bool Equals(ParamInfo other)
    {
        return other is not null &&
               Name == other.Name &&
               EqualityComparer<Type>.Default.Equals(Type, other.Type) &&
               EqualityComparer<ParameterBuilder>.Default.Equals(Builder, other.Builder) &&
               Index == other.Index;
    }

    public override int GetHashCode()
    {
        int hashCode = -2046921342;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(Type);
        hashCode = hashCode * -1521134295 + EqualityComparer<ParameterBuilder>.Default.GetHashCode(Builder);
        hashCode = hashCode * -1521134295 + Index.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(ParamInfo left, ParamInfo right)
    {
        return EqualityComparer<ParamInfo>.Default.Equals(left, right);
    }

    public static bool operator !=(ParamInfo left, ParamInfo right)
    {
        return !(left == right);
    }
}