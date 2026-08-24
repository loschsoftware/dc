using System.Text;

namespace Dassie.Model;

internal record TypeParameter : DassieType
{
    public override string ToString()
    {
        return Name;
    }
}

internal record GenericTypeParameter : TypeParameter
{
    public enum TypeParameterVariance
    {
        Invariant,
        Covariant,
        Contravariant
    }

    public TypeParameterVariance Variance { get; private set; }
    public DassieType BaseTypeConstraint { get; private set; }

    public override string ToString()
    {
        StringBuilder suffix = new();

        if (BaseTypeConstraint != null)
        {
            suffix.Append(": ");
            suffix.Append(BaseTypeConstraint.ToString());
        }

        string varianceStr = Variance switch
        {
            TypeParameterVariance.Covariant => "+",
            TypeParameterVariance.Contravariant => "-",
            _ => ""
        };

        return $"{varianceStr}{Name}{suffix}";
    }
}

internal record DependentValueParameter : TypeParameter
{
    public DassieType Type { get; private set; }

    public override string ToString()
    {
        return $"'{Name}: {Type}";
    }
}