using Dassie.Errors;
using Dassie.Helpers;
using Dassie.Meta;
using Dassie.Parser;
using System;
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

        if (!context.Builder.IsInterface)
        {
            context.RequiredInterfaceImplementations = context.ImplementedInterfaces
                .SelectMany(t =>
                {
                    if (!t.IsConstructedGenericType)
                        return t.GetInterfaces().Append(t);

                    return t.GetGenericTypeDefinition().GetInterfaces().Append(t);
                })
                .SelectMany(t =>
                {
                    if (!t.IsConstructedGenericType)
                    {
                        return t.GetMethods().Select(m => new MockMethodInfo()
                        {
                            Name = m.Name,
                            ReturnType = m.ReturnType,
                            Parameters = m.GetParameters().Select(p => p.ParameterType).ToList(),
                            IsAbstract = m.IsAbstract,
                            DeclaringType = t,
                            IsGenericMethod = m.IsGenericMethod,
                            GenericTypeArguments = m.GetGenericArguments().ToList(),
                            Builder = m
                        });
                    }

                    Type[] typeArgs = t.GenericTypeArguments;

                    return t.GetGenericTypeDefinition().GetMethods().Select(m =>
                    {
                        MockMethodInfo method = new()
                        {
                            Name = m.Name,
                            IsAbstract = m.IsAbstract,
                            Parameters = [],
                            DeclaringType = t,
                            IsGenericMethod = m.IsGenericMethod,
                            GenericTypeArguments = m.GetGenericArguments().ToList(),
                            Builder = TypeBuilder.GetMethod(t, m)
                        };

                        if (!m.ReturnType.IsGenericTypeParameter)
                            method.ReturnType = m.ReturnType;
                        else
                            method.ReturnType = typeArgs[m.ReturnType.GenericParameterPosition];

                        foreach (Type param in m.GetParameters().Select(p => p.ParameterType))
                        {
                            if (!param.IsGenericTypeParameter)
                                method.Parameters.Add(param);
                            else
                                method.Parameters.Add(typeArgs[param.GenericParameterPosition]);
                        }

                        return method;
                    });
                })
                .Where(m => m.IsAbstract)
                .Distinct()
                .ToList();
        }
    }

    public static void ResolveMethodSignature(MethodContext context)
    {

    }
}