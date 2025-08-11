using Dassie.Meta;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Dassie.CodeGeneration;

/// <summary>
/// Utility class that calls <c>CreateType()</c> on all <see cref="TypeBuilder"/> objects created by the compiler and determines the ordering in which the types should be created.
/// </summary>
internal static class TypeFinalizer
{
    public static void CreateTypes(List<TypeContext> typeContexts)
    {
        HashSet<TypeBuilder> createdTypes = [];
        HashSet<TypeBuilder> visiting = [];
        Dictionary<TypeBuilder, TypeContext> typeContextMap = [];

        foreach (TypeContext context in typeContexts)
            typeContextMap[context.Builder] = context;

        foreach (TypeContext context in typeContexts)
        {
            if (!createdTypes.Contains(context.Builder))
            {
                if (!CreateTypeRecursive(context.Builder, createdTypes, visiting, typeContextMap))
                    return;
            }
        }
    }

    private static bool CreateTypeRecursive(
        TypeBuilder typeBuilder,
        HashSet<TypeBuilder> createdTypes,
        HashSet<TypeBuilder> visiting,
        Dictionary<TypeBuilder, TypeContext> typeContextMap)
    {
        if (createdTypes.Contains(typeBuilder))
            return true;

        if (visiting.Contains(typeBuilder))
        {
            TypeContext context = typeContextMap[typeBuilder];

            EmitErrorMessage(
                context.ParserRule.Identifier().Symbol.Line,
                context.ParserRule.Identifier().Symbol.Column,
                context.ParserRule.Identifier().GetIdentifier().Length,
                DS0193_CircularReference,
                $"Circular base type dependency involving '{context.FullName}' and '{context.Builder.BaseType.FullName}'.");

            return false;
        }

        visiting.Add(typeBuilder);

        Type baseType = typeBuilder.BaseType;
        if (baseType is TypeBuilder baseTypeBuilder && typeContextMap.ContainsKey(baseTypeBuilder))
        {
            if (!CreateTypeRecursive(baseTypeBuilder, createdTypes, visiting, typeContextMap))
                return false;
        }

        typeBuilder.CreateType();
        createdTypes.Add(typeBuilder);
        visiting.Remove(typeBuilder);

        return true;
    }
}