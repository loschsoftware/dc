using Dassie.CodeGeneration.Helpers;
using Dassie.Core;
using Dassie.Meta;
using Dassie.Parser;
using Dassie.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using static Dassie.Messages.MessageHelpers;
using static Dassie.Helpers.TypeHelpers;

#pragma warning disable IDE0305

namespace Dassie.CodeGeneration;

internal static class TypeDeclarationGeneration
{
    public static TypeContext GenerateType(DassieParser.TypeContext context, TypeBuilder enclosingType)
    {
        if (context.Identifier().GetIdentifier().Length + (CurrentFile.ExportedNamespace ?? "").Length > 1024)
        {
            EmitErrorMessageFormatted(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetIdentifier().Length,
                DS0075_MetadataLimitExceeded,
                nameof(StringHelper.TypeDeclarationGeneration_TypeNameTooLong), [context.Identifier().GetIdentifier()[0..32]]);

            return new();
        }

        bool isAlias = context.type_block() != null && context.type_block().type_name() != null;
        TypeBuilder tb;

        if (enclosingType == null)
        {
            tb = Context.Module.DefineType(
                GetTypeName(context),
                AttributeHelpers.GetTypeAttributes(context.type_kind(), context.type_access_modifier(), context.nested_type_access_modifier(), context.type_special_modifier(), false, isAlias));
        }
        else
        {
            tb = enclosingType.DefineNestedType(
                context.Identifier().GetIdentifier(),
                AttributeHelpers.GetTypeAttributes(context.type_kind(), context.type_access_modifier(), context.nested_type_access_modifier(), context.type_special_modifier(), true, isAlias));
        }

        if (Context.Types.Any(t => t.FullName == tb.FullName))
        {
            TypeContext duplicate = Context.Types.First(t => t.FullName == tb.FullName);
            if (duplicate.GenericParameters != null && context.generic_parameter_list() != null && duplicate.GenericParameters.Count != context.generic_parameter_list().generic_parameter().Length)
            {
                EmitErrorMessageFormatted(
                    context.Identifier().Symbol.Line,
                    context.Identifier().Symbol.Column,
                    context.Identifier().GetIdentifier().Length,
                    DS0121_DuplicateGenericTypeName,
                    nameof(StringHelper.TypeDeclarationGeneration_DuplicateGenericTypeName), []);
            }
            else
            {
                if (string.IsNullOrEmpty(CurrentFile.ExportedNamespace))
                {
                    EmitErrorMessageFormatted(
                        context.Identifier().Symbol.Line,
                        context.Identifier().Symbol.Column,
                        context.Identifier().GetIdentifier().Length,
                        DS0120_DuplicateTypeName,
                        nameof(StringHelper.TypeDeclarationGeneration_DuplicateTypeNameGlobal), [tb.Name]);
                }
                else
                {
                    EmitErrorMessageFormatted(
                        context.Identifier().Symbol.Line,
                        context.Identifier().Symbol.Column,
                        context.Identifier().GetIdentifier().Length,
                        DS0120_DuplicateTypeName,
                        nameof(StringHelper.TypeDeclarationGeneration_DuplicateTypeNameInNamespace), [CurrentFile.ExportedNamespace, tb.Name]);
                }
            }
        }

        bool isLocalType = context.type_access_modifier() != null && context.type_access_modifier().Local() != null;

        TypeContext tc = new()
        {
            Builder = tb,
            FullName = tb.FullName,
            IsLocalType = isLocalType
        };

        if (isLocalType)
            CurrentFile.LocalTypes.Add(tc);

        List<Type> attributes = [];

        if (context.attribute() != null)
        {
            foreach ((Type attribType, CustomAttributeBuilder data, _, _, AttributeHelpers.AttributeTarget target) in AttributeHelpers.GetAttributeList(context.attribute(), ExpressionEvaluator.Instance))
            {
                if (attribType != null)
                {
                    if (target == AttributeHelpers.AttributeTarget.Assembly)
                        Context.Assembly.SetCustomAttribute(data);
                    else if (target == AttributeHelpers.AttributeTarget.Module)
                        Context.Module.SetCustomAttribute(data);
                    else
                    {
                        attributes.Add(attribType);
                        tb.SetCustomAttribute(data);
                    }
                }
            }
        }

        if (attributes.Contains(typeof(Union)))
        {
            UnionTypeCodeGeneration.GenerateUnionType(context);
            return TypeContext.Current;
        }

        bool explicitBaseType = false;
        Type parent = typeof(object);
        List<Type> interfaces = [];

        if (context.type_kind().Val() != null)
            parent = typeof(ValueType);

        if (context.inheritance_list() != null)
        {
            foreach (DassieParser.Type_nameContext type in context.inheritance_list().type_name())
            {
                Type association = SymbolResolver.ResolveTypeName(type, noErrors: true);

                if (association == null)
                    tc.UnresolvedAssociatedTypeNames.Add(type);
            }

            List<Type> inherited = GetInheritedTypes(context.inheritance_list(), noErrors: true);

            foreach (Type type in inherited)
            {
                EnsureBaseTypeCompatibility(type, context.type_kind().Val() != null,
                    context.inheritance_list().Start.Line,
                    context.inheritance_list().Start.Column,
                    context.inheritance_list().GetText().Length);

                if (type.IsClass)
                {
                    if (context.type_kind().Module() != null && type != typeof(object))
                    {
                        EmitErrorMessageFormatted(
                            context.inheritance_list().Start.Line,
                            context.inheritance_list().Start.Column,
                            context.inheritance_list().GetText().Length,
                            DS0243_ModuleInheritance,
                            nameof(StringHelper.TypeDeclarationGeneration_ModuleCannotInherit), [TypeName(tb)]);
                    }
                    else
                    {
                        explicitBaseType = true;
                        parent = type;
                    }
                }

                if (type.IsInterface)
                {
                    if (context.type_kind().Module() != null)
                    {
                        EmitErrorMessageFormatted(
                            context.inheritance_list().Start.Line,
                            context.inheritance_list().Start.Column,
                            context.inheritance_list().GetText().Length,
                            DS0243_ModuleInheritance,
                            nameof(StringHelper.TypeDeclarationGeneration_ModuleCannotImplement), [TypeName(tb), TypeName(type)]);
                    }
                    else
                        interfaces.Add(type);
                }
            }
        }

        if (context.type_kind().Template() != null)
            parent = null;

        Type enumerationMarkerType = null;

        foreach (Type attribType in attributes)
        {
            if (attribType != null && attribType.FullName.StartsWith("Dassie.Core.Enumeration"))
            {
                enumerationMarkerType = attribType;

                if (context.type_kind().Ref() != null)
                {
                    EmitErrorMessageFormatted(
                        context.type_kind().Ref().Symbol.Line,
                        context.type_kind().Ref().Symbol.Column,
                        3,
                        DS0143_EnumTypeExplicitlyRef,
                        nameof(StringHelper.TypeDeclarationGeneration_EnumRefInvalid), []);
                }

                if (explicitBaseType && parent != null && parent != typeof(Enum))
                {
                    EmitErrorMessageFormatted(
                        context.inheritance_list().Start.Line,
                        context.inheritance_list().Start.Column,
                        context.inheritance_list().GetText().Length,
                        DS0144_EnumTypeBaseType,
                        nameof(StringHelper.TypeDeclarationGeneration_EnumBaseTypeInvalid), []);
                }

                if (interfaces.Count > 0)
                {
                    EmitErrorMessageFormatted(
                        context.inheritance_list().Start.Line,
                        context.inheritance_list().Start.Column,
                        context.inheritance_list().GetText().Length,
                        DS0145_EnumTypeImplementsTemplate,
                        nameof(StringHelper.TypeDeclarationGeneration_EnumCannotImplement), []);
                }

                parent = typeof(Enum);
            }
        }

        foreach (Type _interface in interfaces)
        {
            tb.AddInterfaceImplementation(_interface);

            try
            {
                foreach (MethodInfo defaultMember in _interface.GetMethods().Where(m => !m.IsAbstract))
                {
                    tc.Methods.Add(new()
                    {
                        Builder = (MethodBuilder)defaultMember
                    });
                }
            }
            catch { }
        }

        if (parent != null)
        {
            TypeContext.Current.Fields.AddRange(InheritanceHelpers.GetInheritedFields(parent));
            tb.SetParent(parent);
        }

        if (enumerationMarkerType != null)
        {
            Type instanceFieldType = null;

            if (enumerationMarkerType.GenericTypeArguments.Length > 0)
                instanceFieldType = enumerationMarkerType.GenericTypeArguments[0];
            else
                instanceFieldType = typeof(int);

            if (!IsIntegerType(instanceFieldType))
            {
                EmitErrorMessageFormatted(
                    context.Identifier().Symbol.Line,
                    context.Identifier().Symbol.Column,
                    context.Identifier().GetIdentifier().Length,
                    DS0141_InvalidEnumerationType,
                    nameof(StringHelper.TypeDeclarationGeneration_InvalidEnumType), [instanceFieldType]);
            }

            tb.DefineField("value__", instanceFieldType,
                FieldAttributes.Public | FieldAttributes.RTSpecialName | FieldAttributes.SpecialName);

            tc.IsEnumeration = true;
            tc.EnumerationBaseType = instanceFieldType;
        }

        // byref-like type ('ref struct' in C#)
        if (context.type_kind().Ampersand() != null)
        {
            tc.IsByRefLike = true;
            AttributeHelpers.AddAttributeToCurrentType(typeof(IsByRefLikeAttribute).GetConstructor([]), []);
        }

        // immutable value type ('readonly struct' in C#)
        if (context.type_kind().Exclamation_Mark() != null)
        {
            tc.IsImmutable = true;
            AttributeHelpers.AddAttributeToCurrentType(typeof(IsReadOnlyAttribute).GetConstructor([]), []);
        }

        tc.ImplementedInterfaces.AddRange(interfaces);
        tc.FilesWhereDefined.Add(CurrentFile.Path);

        if (context.generic_parameter_list() != null)
        {
            if (context.generic_parameter_list().generic_parameter().Length > ushort.MaxValue)
            {
                EmitErrorMessageFormatted(
                    context.generic_parameter_list().Start.Line,
                    context.generic_parameter_list().Start.Column,
                    context.generic_parameter_list().GetText().Length,
                    DS0075_MetadataLimitExceeded,
                    nameof(StringHelper.TypeDeclarationGeneration_TooManyTypeParams), [context.Identifier().GetIdentifier(), ushort.MaxValue]);
            }

            List<GenericParameterContext> typeParamContexts = [];

            foreach (DassieParser.Generic_parameterContext typeParam in context.generic_parameter_list().generic_parameter())
            {
                if (typeParamContexts.Any(p => p.Name == typeParam.Identifier().GetIdentifier()))
                {
                    EmitErrorMessageFormatted(
                        typeParam.Start.Line,
                        typeParam.Start.Column,
                        typeParam.GetText().Length,
                        DS0113_DuplicateTypeParameter,
                        nameof(StringHelper.TypeDeclarationGeneration_DuplicateTypeParam), [typeParam.GetText()]);

                    continue;
                }

                typeParamContexts.Add(BuildTypeParameter(typeParam));
            }

            GenericTypeParameterBuilder[] typeParams = tb.DefineGenericParameters(typeParamContexts.Select(t => t.Name).ToArray());
            foreach (GenericTypeParameterBuilder typeParam in typeParams)
            {
                GenericParameterContext ctx = typeParamContexts.First(c => c.Name == typeParam.Name);
                typeParam.SetGenericParameterAttributes(ctx.Attributes);
                typeParam.SetBaseTypeConstraint(ctx.BaseTypeConstraint);
                typeParam.SetInterfaceConstraints(ctx.InterfaceConstraints.ToArray());

                if (ctx.ValueType != null)
                    typeParam.SetBaseTypeConstraint(ctx.ValueType);

                if (ctx.IsRuntimeValue)
                    typeParam.SetCustomAttribute(new(typeof(RuntimeDependencyAttribute).GetConstructor([]), []));
                else if (ctx.IsCompileTimeConstant)
                    typeParam.SetCustomAttribute(new(typeof(CompileTimeDependencyAttribute).GetConstructor([]), []));

                if (ctx.IsRuntimeValue || ctx.IsCompileTimeConstant)
                {
                    PropertyBuilder dependencyProperty = tb.DefineProperty(ctx.Name, PropertyAttributes.None, ctx.ValueType, []);
                    FieldBuilder backingField = tb.DefineField(SymbolNameGenerator.GetPropertyBackingFieldName(ctx.Name), ctx.ValueType, FieldAttributes.Assembly);
                    backingField.SetCustomAttribute(new(typeof(DependentValueAttribute).GetConstructor([typeof(string)]), [ctx.Name]));

                    MethodBuilder getMethod = tb.DefineMethod($"get_{ctx.Name}",
                        MethodAttributes.Public | MethodAttributes.SpecialName,
                        CallingConventions.Standard,
                        ctx.ValueType,
                        []);

                    ILGenerator getterIL = getMethod.GetILGenerator();
                    getterIL.Emit(OpCodes.Ldarg_0);
                    getterIL.Emit(OpCodes.Ldfld, backingField);
                    getterIL.Emit(OpCodes.Ret);

                    MethodContext curr = CurrentMethod;
                    MethodContext getterCtx = new()
                    {
                        Builder = getMethod
                    };
                    CurrentMethod = curr;

                    dependencyProperty.SetGetMethod(getMethod);
                    tc.Properties.Add(dependencyProperty);
                    tc.Methods.Add(getterCtx);
                    tc.Fields.Add(new()
                    {
                        Builder = backingField,
                        Name = backingField.Name,
                        Attributes = [new DependentValueAttribute(ctx.Name)]
                    });
                }

                ctx.Builder = typeParam;
            }

            tc.GenericParameters = typeParamContexts;
        }

        if (context.parameter_list() != null)
            tc.PrimaryConstructorParameterList = context.parameter_list();

        if (context.type_block() != null)
        {
            // Alias type
            if (context.type_block().type_name() != null)
            {
            if (context.type_special_modifier() != null && context.type_special_modifier().Open() != null)
                {
                    EmitErrorMessageFormatted(
                        context.type_special_modifier().Open().Symbol.Line,
                        context.type_special_modifier().Open().Symbol.Column,
                        context.type_special_modifier().Open().GetText().Length,
                        DS0181_AliasTypeInvalidModifiers,
                        nameof(StringHelper.TypeDeclarationGeneration_AliasOpenModifierInvalid), []);
                }

                if (interfaces.Count > 0)
                {
                    EmitErrorMessageFormatted(
                        context.inheritance_list().Start.Line,
                        context.inheritance_list().Start.Column,
                        context.inheritance_list().GetText().Length,
                        DS0186_AliasTypeImplementsInterface,
                        nameof(StringHelper.TypeDeclarationGeneration_AliasCannotImplement), []);

                    TypeContext.Current.RequiredInterfaceImplementations = [];
                }

                if (explicitBaseType)
                {
                    EmitErrorMessageFormatted(
                        context.inheritance_list().Start.Line,
                        context.inheritance_list().Start.Column,
                        context.inheritance_list().GetText().Length,
                        DS0187_AliasTypeExtendsType,
                        nameof(StringHelper.TypeDeclarationGeneration_AliasCannotExtend), []);
                }

                if (context.generic_parameter_list() != null)
                {
                    EmitErrorMessageFormatted(
                        context.generic_parameter_list().Start.Line,
                        context.generic_parameter_list().Start.Column,
                        context.generic_parameter_list().GetText().Length,
                        DS0188_GenericAliasType,
                        nameof(StringHelper.TypeDeclarationGeneration_AliasCannotBeGeneric), []);
                }

                if (context.attribute().Length > 0)
                {
                    foreach (DassieParser.AttributeContext attribute in context.attribute())
                    {
                        Type attribType = SymbolResolver.ResolveAttributeTypeName(attribute.type_name(), true);

                        if (attribType != null && attribType == typeof(NewTypeAttribute))
                            TypeContext.Current.IsNewType = true;
                    }
                }

                Type aliasedType = SymbolResolver.ResolveTypeName(context.type_block().type_name());

                TypeContext.Current.IsAlias = true;
                TypeContext.Current.AliasedType = aliasedType;

                if (aliasedType != null)
                    tb.SetCustomAttribute(new(typeof(AliasAttribute).GetConstructor([typeof(Type)]), [aliasedType]));
            }
            else
            {
                foreach (DassieParser.TypeContext nestedType in context.type_block().type())
                {
                    GenerateType(nestedType, tb);
                    tc.Children.Add(TypeContext.Current);
                }

                TypeContext.Current = tc;

                //foreach (DassieParser.Type_memberContext member in context.type_block()?.type_member())
                //    Visit(member);
            }
        }

        //foreach (var ctor in TypeContext.Current.Constructors)
        //    HandleConstructor(ctor);

        //if (TypeContext.Current.Constructors.Count == 0 && TypeContext.Current.FieldInitializers.Count > 0)
        //{
        //    ConstructorBuilder cb = TypeContext.Current.Builder.DefineConstructor(
        //        MethodAttributes.Public,
        //        CallingConventions.HasThis,
        //        Type.EmptyTypes);

        //    CurrentMethod = new()
        //    {
        //        ConstructorBuilder = cb,
        //        IL = cb.GetILGenerator()
        //    };

        //    HandleFieldInitializersAndDefaultConstructor();
        //    CurrentMethod.IL.Emit(OpCodes.Ret);
        //}

        //Type t = tb.CreateType();
        //TypeContext.Current.FinishedType = t;

        //if (TypeContext.Current.RequiredInterfaceImplementations.Count > 0)
        //{
        //    foreach (MockMethodInfo method in TypeContext.Current.RequiredInterfaceImplementations)
        //    {
        //        EmitErrorMessage(
        //            context.Identifier().Symbol.Line,
        //            context.Identifier().Symbol.Column,
        //            context.Identifier().Identifier().Length,
        //            DS0156_RequiredInterfaceMembersNotImplemented,
        //            $"The type '{tc.FullName}' does not provide an implementation for the abstract template member '{method.FormatMethod()}'.");
        //    }
        //}

        return TypeContext.Current;
    }
}