using Dassie.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Dassie.Meta;

internal class LocalInfo : IEquatable<LocalInfo>
{
    public LocalInfo(string name, LocalBuilder builder, bool isConstant, int index, UnionValue union, int scope = -1)
    {
        if (scope == -1)
            scope = CurrentMethod.CurrentScope;

        Name = name;
        Builder = builder;
        IsConstant = isConstant;
        Index = index;
        Union = union;
        Scope = scope;
    }

    public LocalInfo() { }

    public string Name { get; set; }

    public LocalBuilder Builder { get; set; }

    public bool IsConstant { get; set; }

    public int Index { get; set; }

    public UnionValue Union { get; set; }

    public int Scope { get; set; }

    public override bool Equals(object obj)
    {
        return Equals(obj as LocalInfo);
    }

    public bool Equals(LocalInfo other)
    {
        return other is not null &&
               Name == other.Name &&
               EqualityComparer<LocalBuilder>.Default.Equals(Builder, other.Builder) &&
               IsConstant == other.IsConstant &&
               Index == other.Index &&
               Union.Equals(other.Union);
    }

    public override int GetHashCode()
    {
        int hashCode = -1221698462;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + EqualityComparer<LocalBuilder>.Default.GetHashCode(Builder);
        hashCode = hashCode * -1521134295 + IsConstant.GetHashCode();
        hashCode = hashCode * -1521134295 + Index.GetHashCode();
        hashCode = hashCode * -1521134295 + Union.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(LocalInfo left, LocalInfo right)
    {
        return EqualityComparer<LocalInfo>.Default.Equals(left, right);
    }

    public static bool operator !=(LocalInfo left, LocalInfo right)
    {
        return !(left == right);
    }
}