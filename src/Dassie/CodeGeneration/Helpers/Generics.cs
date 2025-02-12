using Dassie.CodeGeneration.Structure;
using Dassie.Helpers;
using Dassie.Parser;
using System;
using System.Linq;

namespace Dassie.CodeGeneration.Helpers;

internal static class Generics
{
    internal class GenericArgumentContext
    {
        public Type Type { get; set; }
        public object Value { get; set; }
    }

    public static Type[] GetTypes(this GenericArgumentContext[] args) => args.Select(t => t.Type).ToArray();

    public static GenericArgumentContext[] ResolveGenericArgList(DassieParser.Generic_arg_listContext context)
    {
        if (context == null)
            return [];

        GenericArgumentContext[] typeArgs = new GenericArgumentContext[context.generic_argument().Length];
        for (int i = 0; i < context.generic_argument().Length; i++)
        {
            GenericArgumentContext ctx = new();

            if (context.generic_argument()[i].type_name() != null)
                ctx.Type = SymbolResolver.ResolveTypeName(context.generic_argument()[i].type_name());
            else
            {
                Expression expr = ExpressionEvaluator.Instance.Visit(context.generic_argument()[i].expression());

                if (expr == null)
                {
                    // TODO
                    throw new NotImplementedException();
                }

                ctx.Type = expr.Type;
                ctx.Value = expr.Value;
            }

            typeArgs[i] = ctx;
        }

        return typeArgs;
    }
}