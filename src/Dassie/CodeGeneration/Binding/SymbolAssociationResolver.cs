using Dassie.Core;
using Dassie.Errors;
using Dassie.Meta;
using Dassie.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    public static void SetMethodSignature(MethodContext context)
    {
        // TODO: Also handle constructors and operators here

        if (context.ConstructorBuilder != null)
            return;

        if (context.IsCustomOperator)
            return;

        if (context.ParserRule == null)
            return;

        EmitBuildLogMessage($"Finalizing method signature of '{context.Builder.DeclaringType.FullName}::{context.Builder.Name}':", 3);

        TypeContext typeCtx = TypeContext.Current;
        TypeContext.Current = Context.Types.First(t => t.Methods.Contains(context));

        MethodContext methodCtx = CurrentMethod;
        CurrentMethod = context;

        Type ret = typeof(void);
        (Type, CustomAttributeBuilder)[] retAttribs = [];
        Type[] retModReq = [];
        Type[] retModOpt = [];

        if (context.ReturnTypeName != null)
        {
            ret = SymbolResolver.ResolveTypeName(context.ReturnTypeName);

            if (TypeHelpers.IsNewTypeAlias(ret))
            {
                CustomAttributeBuilder cab = new(typeof(NewTypeCallSiteAttribute).GetConstructor([typeof(Type)]), [ret]);
                retAttribs = [(typeof(NewTypeCallSiteAttribute), cab)];
                ret = TypeHelpers.GetAliasType(ret);
            }
        }

        List<Type> paramTypes = [];
        List<(Type, CustomAttributeBuilder)[]> paramCustomAttributes = [];
        List<Type[]> paramModReq = [];
        List<Type[]> paramModOpt = [];

        if (context.ParameterTypeNames != null && context.ParameterTypeNames.Count > 0)
        {
            foreach (DassieParser.Type_nameContext name in context.ParameterTypeNames)
            {
                Type paramType = SymbolResolver.ResolveTypeName(name);

                if (!TypeHelpers.IsNewTypeAlias(paramType))
                {
                    paramTypes.Add(paramType);
                    paramCustomAttributes.Add([]);
                    paramModReq.Add([]);
                    paramModOpt.Add([]);
                    continue;
                }

                paramTypes.Add(TypeHelpers.GetAliasType(paramType));
                CustomAttributeBuilder cab = new(typeof(NewTypeCallSiteAttribute).GetConstructor([typeof(Type)]), [paramType]);
                paramCustomAttributes.Add([(typeof(NewTypeCallSiteAttribute), cab)]);

                paramModReq.Add([]);
                paramModOpt.Add([]);
            }
        }

        context.Builder.SetSignature(
            ret,
            retModReq,
            retModOpt,
            paramTypes.ToArray(),
            paramModReq.ToArray(),
            paramModOpt.ToArray());

        ParameterBuilder returnBuilder = context.Builder.DefineParameter(
            0, ParameterAttributes.None, null);

        foreach ((_, CustomAttributeBuilder cab) in retAttribs)
            returnBuilder.SetCustomAttribute(cab);

        var paramList = Visitor.ResolveParameterList(context.ParserRule.parameter_list(), true);
        for (int i = 0; i < paramTypes.Count; i++)
        {
            ParameterBuilder pb = context.Builder.DefineParameter(
                context.ParameterIndex++ + 1,
                AttributeHelpers.GetParameterAttributes(paramList[i].Context.parameter_modifier(), paramList[i].Context.Equals() != null),
                paramList[i].Context.Identifier().GetIdentifier());

            foreach ((_, CustomAttributeBuilder cab) in paramCustomAttributes[i])
                pb.SetCustomAttribute(cab);

            context.Parameters.Add(new(paramList[i].Context.Identifier().GetIdentifier(), paramTypes[i], pb, pb.Position, paramList[i].Context.Var() != null)
            {
                ModReq = paramModReq[i].ToList(),
                ModOpt = paramModOpt[i].ToList()
            });
        }

        if (CurrentMethod.Builder.IsStatic)
        {
            foreach (var _param in CurrentMethod.Parameters)
            {
                if (_param.Index > 0)
                    _param.Index--;
            }
        }

        EmitBuildLogMessage($"    - Return type: {ret.FullName}", 3);
        EmitBuildLogMessage($"    - Return type attributes: [{string.Join(", ", retAttribs.Select(c => c.Item1.FullName))}]", 3);
        EmitBuildLogMessage($"    - Return type required custom modifiers: [{string.Join(", ", retModReq.Select(t => t.FullName))}]", 3);
        EmitBuildLogMessage($"    - Return type optional custom modifiers: [{string.Join(", ", retModReq.Select(t => t.FullName))}]", 3);
        EmitBuildLogMessage($"    - Parameter types: [{string.Join(", ", paramTypes.Select(t => t.FullName))}]", 3);
        EmitBuildLogMessage($"    - Parameter type attributes: [{string.Join(", ", paramCustomAttributes.Select(t => $"[{string.Join(", ", t.Select(t => t.Item1.FullName))}]"))}]", 3);
        EmitBuildLogMessage($"    - Parameter type required custom modifiers: [{string.Join(", ", paramModReq.Select(t => $"[{string.Join(", ", t.Select(t => t.FullName))}]"))}]", 3);
        EmitBuildLogMessage($"    - Parameter type optional custom modifiers: [{string.Join(", ", paramModOpt.Select(t => $"[{string.Join(", ", t.Select(t => t.FullName))}]"))}]", 3);

        TypeContext.Current = typeCtx;
        CurrentMethod = methodCtx;
    }
}