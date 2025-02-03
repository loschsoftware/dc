using Dassie.CodeGeneration.Helpers;
using Dassie.Core;
using Dassie.Helpers;
using Dassie.Meta;
using Dassie.Parser;
using Dassie.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using static Dassie.Errors.ErrorMessageHelpers;
using static Dassie.Helpers.TypeHelpers;

#pragma warning disable IDE0305

namespace Dassie.CodeGeneration;

internal static class TypeDeclarationGeneration
{
    public static TypeContext GenerateType(DassieParser.TypeContext context, TypeBuilder enclosingType)
    {
        if (context.Identifier().GetText().Length + (CurrentFile.ExportedNamespace ?? "").Length > 1024)
        {
            EmitErrorMessage(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetText().Length,
                DS0073_TypeNameTooLong,
                "A type name cannot be longer than 1024 characters.");

            return new();
        }

        TypeBuilder tb;

        if (enclosingType == null)
        {
            if (context.nested_type_access_modifier() != null && context.nested_type_access_modifier().Local() != null)
            {
                EmitErrorMessage(
                    context.nested_type_access_modifier().Local().Symbol.Line,
                    context.nested_type_access_modifier().Local().Symbol.Column,
                    context.nested_type_access_modifier().Local().GetText().Length,
                    DS0126_TopLevelTypeLocal,
                    "The 'local' access modifier is only valid for nested types.");
            }

            tb = Context.Module.DefineType(
                GetTypeName(context),
                AttributeHelpers.GetTypeAttributes(context.type_kind(), context.type_access_modifier(), context.nested_type_access_modifier(), context.type_special_modifier(), false));
        }
        else
        {
            tb = enclosingType.DefineNestedType(
                context.Identifier().GetText(),
                AttributeHelpers.GetTypeAttributes(context.type_kind(), context.type_access_modifier(), context.nested_type_access_modifier(), context.type_special_modifier(), true));
        }

        if (Context.Types.Any(t => t.FullName == tb.FullName))
        {
            TypeContext duplicate = Context.Types.First(t => t.FullName == tb.FullName);
            if (duplicate.TypeParameters != null && context.type_parameter_list() != null && duplicate.TypeParameters.Count != context.type_parameter_list().type_parameter().Length)
            {
                EmitErrorMessage(
                    context.Identifier().Symbol.Line,
                    context.Identifier().Symbol.Column,
                    context.Identifier().GetText().Length,
                    DS0120_DuplicateGenericTypeName,
                    $"Currently, the Dassie compiler does not allow creating types of the same name with different type parameters. This functionality might be added in the future. If you desperately need it, consider opening an issue on GitHub.");
            }
            else
            {
                string errMsg = "";
                if (string.IsNullOrEmpty(CurrentFile.ExportedNamespace))
                    errMsg = $"The global namespace";
                else
                    errMsg = $"The namespace '{CurrentFile.ExportedNamespace}'";

                errMsg += $" already contains a definition for the type '{tb.Name}'.";

                EmitErrorMessage(
                    context.Identifier().Symbol.Line,
                    context.Identifier().Symbol.Column,
                    context.Identifier().GetText().Length,
                    DS0119_DuplicateTypeName,
                    errMsg);
            }
        }

        TypeContext tc = new()
        {
            Builder = tb,
            FullName = tb.FullName,
        };

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
                    explicitBaseType = true;
                    parent = type;
                }

                if (type.IsInterface)
                    interfaces.Add(type);
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
                    EmitErrorMessage(
                        context.type_kind().Ref().Symbol.Line,
                        context.type_kind().Ref().Symbol.Column,
                        3,
                        DS0142_EnumTypeExplicitlyRef,
                        "The modifier 'ref' is invalid for enumeration types. Enumerations are always value types.");
                }

                if (explicitBaseType && parent != null && parent != typeof(Enum))
                {
                    EmitErrorMessage(
                        context.inheritance_list().Start.Line,
                        context.inheritance_list().Start.Column,
                        context.inheritance_list().GetText().Length,
                        DS0143_EnumTypeBaseType,
                        "The only allowed base type for enumerations is 'System.Enum'.");
                }

                if (interfaces.Count > 0)
                {
                    EmitErrorMessage(
                        context.inheritance_list().Start.Line,
                        context.inheritance_list().Start.Column,
                        context.inheritance_list().GetText().Length,
                        DS0144_EnumTypeImplementsTemplate,
                        "Enumeration types cannot implement templates.");
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

        tc.IsEnumeration = enumerationMarkerType != null;

        if (enumerationMarkerType != null)
        {
            AttributeHelpers.AddAttributeToCurrentType(enumerationMarkerType.GetConstructor([]), []);
            Type instanceFieldType = null;

            if (enumerationMarkerType.GenericTypeArguments.Length > 0)
                instanceFieldType = enumerationMarkerType.GenericTypeArguments[0];
            else
                instanceFieldType = typeof(int);

            if (!IsIntegerType(instanceFieldType))
            {
                EmitErrorMessage(
                    context.Identifier().Symbol.Line,
                    context.Identifier().Symbol.Column,
                    context.Identifier().GetText().Length,
                    DS0140_InvalidEnumerationType,
                    $"Invalid enumeration type '{instanceFieldType}'. The only allowed types are int8, uint8, int16, uint16, int, uint, int32, uint32, int64, uint64, native, unative.");
            }

            tb.DefineField("value__", instanceFieldType,
                FieldAttributes.Public | FieldAttributes.RTSpecialName | FieldAttributes.SpecialName);
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

        if (context.type_parameter_list() != null)
        {
            List<TypeParameterContext> typeParamContexts = [];

            foreach (DassieParser.Type_parameterContext typeParam in context.type_parameter_list().type_parameter())
            {
                if (typeParamContexts.Any(p => p.Name == typeParam.Identifier().GetText()))
                {
                    EmitErrorMessage(
                        typeParam.Start.Line,
                        typeParam.Start.Column,
                        typeParam.GetText().Length,
                        DS0112_DuplicateTypeParameter,
                        $"Duplicate type parameter '{typeParam.GetText()}'.");

                    continue;
                }

                typeParamContexts.Add(BuildTypeParameter(typeParam));
            }

            GenericTypeParameterBuilder[] typeParams = tb.DefineGenericParameters(typeParamContexts.Select(t => t.Name).ToArray());
            foreach (GenericTypeParameterBuilder typeParam in typeParams)
            {
                TypeParameterContext ctx = typeParamContexts.First(c => c.Name == typeParam.Name);
                typeParam.SetGenericParameterAttributes(ctx.Attributes);
                typeParam.SetBaseTypeConstraint(ctx.BaseTypeConstraint);
                typeParam.SetInterfaceConstraints(ctx.InterfaceConstraints.ToArray());

                ctx.Builder = typeParam;
            }

            tc.TypeParameters = typeParamContexts;
        }

        if (context.parameter_list() != null)
            tc.PrimaryConstructorParameterList = context.parameter_list();

        if (context.type_block() != null)
        {
            // Alias type
            if (context.type_block().type_name() != null)
            {
                if (context.attribute().Length > 0)
                {
                    EmitErrorMessage(
                        context.attribute()[0].Start.Line,
                        context.attribute()[0].Start.Column,
                        context.attribute()[0].GetText().Length,
                        DS0179_AttributesOnAliasType,
                        "An alias type cannot use attributes.");
                }

                if (context.type_special_modifier() != null && context.type_special_modifier().Open() != null)
                {
                    EmitErrorMessage(
                        context.type_special_modifier().Open().Symbol.Line,
                        context.type_special_modifier().Open().Symbol.Column,
                        context.type_special_modifier().Open().GetText().Length,
                        DS0180_AliasTypeInvalidModifiers,
                        "The 'open' modifier is invalid on alias types.");
                }

                if (interfaces.Count > 0)
                {
                    EmitErrorMessage(
                        context.inheritance_list().Start.Line,
                        context.inheritance_list().Start.Column,
                        context.inheritance_list().GetText().Length,
                        DS0185_AliasTypeImplementsInterface,
                        "Type aliases cannot implement templates.");

                    TypeContext.Current.RequiredInterfaceImplementations = [];
                }

                if (explicitBaseType)
                {
                    EmitErrorMessage(
                        context.inheritance_list().Start.Line,
                        context.inheritance_list().Start.Column,
                        context.inheritance_list().GetText().Length,
                        DS0186_AliasTypeExtendsType,
                        "Type aliases cannot explicitly set their base type.");
                }

                if (context.type_parameter_list() != null)
                {
                    EmitErrorMessage(
                        context.type_parameter_list().Start.Line,
                        context.type_parameter_list().Start.Column,
                        context.type_parameter_list().GetText().Length,
                        DS0187_GenericAliasType,
                        "Type aliases cannot define generic type parameters.");
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
        //            context.Identifier().GetText().Length,
        //            DS0156_RequiredInterfaceMembersNotImplemented,
        //            $"The type '{tc.FullName}' does not provide an implementation for the abstract template member '{method.FormatMethod()}'.");
        //    }
        //}

        return TypeContext.Current;
    }
}