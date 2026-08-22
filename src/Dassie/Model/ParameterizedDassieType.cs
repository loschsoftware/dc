using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dassie.Model;

internal record ParameterizedDassieType : DassieType
{
    public ParameterizedDassieType() { }
    public ParameterizedDassieType(DassieType source) : base(source) { }

    public DassieType[] GenericTypeArguments { get; set; }
    public DependentValue[] DependentValues { get; set; }
    public Predicate[] Predicates { get; set; }

    public override string ToString()
    {
        IEnumerable<TypeParameter> parameters = Parameters ?? [];
        List<GenericTypeParameter> typeParams = parameters.Where(p => p is GenericTypeParameter).Cast<GenericTypeParameter>().ToList();
        List<DependentValueParameter> dependentValues = parameters.Where(p => p is DependentValueParameter).Cast<DependentValueParameter>().ToList();

        StringBuilder argList = new();

        if (parameters.Any() || Predicates?.Length > 0)
        {
            argList.Append('[');

            foreach (TypeParameter param in parameters)
            {
                string value = "";
                if (param is GenericTypeParameter tparam)
                    value = GenericTypeArguments[typeParams.IndexOf(tparam)].ToString();
                else if (param is DependentValueParameter vparam)
                    value = DependentValues[dependentValues.IndexOf(vparam)].ToString();

                argList.Append(value);

                if (param != parameters.Last() || Predicates?.Length > 0)
                    argList.Append(", ");
            }

            for (int i = 0; i < Predicates?.Length; i++)
            {
                argList.Append(Predicates[i].ToString());

                if (i < Predicates.Length - 1)
                    argList.Append(", ");
            }

            argList.Append(']');
        }

        return $"{FullName}{argList}";
    }
}