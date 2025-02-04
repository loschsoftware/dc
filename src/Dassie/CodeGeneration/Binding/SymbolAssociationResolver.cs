using Dassie.Errors;
using Dassie.Helpers;
using Dassie.Meta;
using Dassie.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Dassie.CodeGeneration.Binding;

internal static class SymbolAssociationResolver
{
    public static void ResolveType(TypeContext context)
    {
        if (context.UnresolvedAssociatedTypeNames != null)
        {
            foreach (DassieParser.Type_nameContext typeName in context.UnresolvedAssociatedTypeNames)
            {
                Type type = SymbolResolver.ResolveTypeName(typeName);
                ErrorMessageHelpers.EnsureBaseTypeCompatibility(type, context.Builder.IsValueType,
                    typeName.Start.Line,
                    typeName.Start.Column,
                    typeName.GetText().Length);

                if (type.IsClass)
                    context.Builder.SetParent(type);

                if (type.IsInterface)
                {
                    context.ImplementedInterfaces.Add(type);
                    context.Builder.AddInterfaceImplementation(type);
                }
            }
        }
    }

    public static void ResolveMethodSignature(MethodContext context)
    {
        if (context.UnresolvedReturnType && context.ReturnTypeName != null)
        {
            Type ret = SymbolResolver.ResolveTypeName(context.ReturnTypeName);
            context.Builder.SetReturnType(ret);
        }

        if (context.ParameterTypeNames != null && context.ParameterTypeNames.Count > 0)
        {
            List<Type> parameters = [];

            foreach (DassieParser.Type_nameContext name in context.ParameterTypeNames)
                parameters.Add(SymbolResolver.ResolveTypeName(name));

            context.Builder.SetParameters(parameters.ToArray());
        }
    }
}