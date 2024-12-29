using Dassie.Core;
using Dassie.Meta;
using Dassie.Parser;
using System;
using System.Linq;
using System.Reflection;

namespace Dassie.Helpers;

internal static class AttributeHelpers
{
    public static FieldAttributes GetFieldAttributes(DassieParser.Member_access_modifierContext accessModifier, DassieParser.Member_oop_modifierContext oopModifier, DassieParser.Member_special_modifierContext[] specialModifiers, bool isReadOnly)
    {
        FieldAttributes baseAttributes;

        if (accessModifier == null || accessModifier.Global() != null)
            baseAttributes = FieldAttributes.Public;

        else if (accessModifier.Internal() != null)
            baseAttributes = FieldAttributes.Assembly;

        else
            baseAttributes = FieldAttributes.Private;

        if (oopModifier != null && oopModifier.Closed() != null)
        {
            EmitErrorMessage(
                oopModifier.Start.Line,
                oopModifier.Start.Column,
                oopModifier.GetText().Length,
                DS0052_InvalidAccessModifier,
                "The modifier 'closed' is not supported by this element.");
        }

        foreach (var modifier in specialModifiers)
        {
            if (modifier.Static() != null)
                baseAttributes |= FieldAttributes.Static;

            else if (modifier.Literal() != null)
            {
                baseAttributes |= FieldAttributes.Literal;

                if (!baseAttributes.HasFlag(FieldAttributes.Static))
                    baseAttributes |= FieldAttributes.Static;
            }

            else
            {
                EmitErrorMessage(
                modifier.Start.Line,
                modifier.Start.Column,
                modifier.GetText().Length,
                DS0052_InvalidAccessModifier,
                $"The modifier '{modifier.GetText()}' is not supported by this element.");
            }
        }

        if (TypeContext.Current.Builder.IsSealed && TypeContext.Current.Builder.IsAbstract && specialModifiers.Any(s => s.Static() != null))
        {
            EmitMessage(
                specialModifiers.First(s => s.GetText() == "static").Start.Line,
                specialModifiers.First(s => s.GetText() == "static").Start.Column,
                specialModifiers.First(s => s.GetText() == "static").GetText().Length,
                DS0058_RedundantModifier,
                "The 'static' modifier is implicit for module members and can be omitted.");
        }

        if (TypeContext.Current.Builder.IsSealed && TypeContext.Current.Builder.IsAbstract && !baseAttributes.HasFlag(FieldAttributes.Static))
            baseAttributes |= FieldAttributes.Static;

        if (isReadOnly)
            baseAttributes |= FieldAttributes.InitOnly;

        return baseAttributes;
    }

    public static (MethodAttributes, MethodImplAttributes) GetMethodAttributes(DassieParser.Member_access_modifierContext accessModifier, DassieParser.Member_oop_modifierContext oopModifier, DassieParser.Member_special_modifierContext[] specialModifiers, DassieParser.AttributeContext[] attribs)
    {
        MethodAttributes baseAttributes;
        MethodImplAttributes implementationFlags = MethodImplAttributes.Managed;

        if (accessModifier == null || accessModifier.Global() != null)
            baseAttributes = MethodAttributes.Public;

        else if (accessModifier.Internal() != null)
            baseAttributes = MethodAttributes.Assembly;

        else
            baseAttributes = MethodAttributes.Private;

        bool isStatic = false;
        foreach (var modifier in specialModifiers)
        {
            if (modifier.Static() != null)
            {
                baseAttributes |= MethodAttributes.Static;
                isStatic = true;

                if (oopModifier != null && oopModifier.Closed() != null)
                {
                    EmitMessage(
                        oopModifier.Closed().Symbol.Line,
                        oopModifier.Closed().Symbol.Column,
                        oopModifier.Closed().GetText().Length,
                        DS0058_RedundantModifier,
                        "Redundant modifier 'closed'.");
                }
            }

            if (modifier.Extern() != null)
                baseAttributes |= MethodAttributes.PinvokeImpl;

            if (modifier.Abstract() != null)
                baseAttributes |= MethodAttributes.Abstract;

            if (modifier.Inline() != null)
                implementationFlags |= MethodImplAttributes.AggressiveInlining;
        }

        if (TypeContext.Current.Builder.IsSealed && TypeContext.Current.Builder.IsAbstract && baseAttributes.HasFlag(MethodAttributes.Static))
        {
            EmitMessage(
                specialModifiers.First(s => s.GetText() == "static").Start.Line,
                specialModifiers.First(s => s.GetText() == "static").Start.Column,
                specialModifiers.First(s => s.GetText() == "static").GetText().Length,
                DS0058_RedundantModifier,
                "Redundant modifier 'static'.");
        }

        if (!isStatic && (oopModifier == null || oopModifier.Closed() == null))
            baseAttributes |= MethodAttributes.Virtual;

        if (oopModifier != null && oopModifier.Closed() != null)
            baseAttributes |= MethodAttributes.Final;

        if (TypeContext.Current.Builder.IsSealed && TypeContext.Current.Builder.IsAbstract && !baseAttributes.HasFlag(MethodAttributes.Static))
        {
            baseAttributes |= MethodAttributes.Static;
            baseAttributes &= ~MethodAttributes.Virtual;
        }

        if (TypeContext.Current.Builder.IsInterface)
        {
            baseAttributes |= MethodAttributes.HideBySig;
            baseAttributes |= MethodAttributes.NewSlot;
        }

        if (attribs != null && attribs.Length > 0)
        {
            bool disableErrorWriter = Disabled;
            Disabled = true;

            foreach (DassieParser.AttributeContext attrib in attribs)
            {
                Type attribType = SymbolResolver.ResolveTypeName(attrib.type_name());

                if (attribType == typeof(RuntimeImplemented))
                    implementationFlags |= MethodImplAttributes.Runtime;

                if (attribType == typeof(HideBySig) && !baseAttributes.HasFlag(MethodAttributes.HideBySig))
                    baseAttributes |= MethodAttributes.HideBySig;

                if (attribType == typeof(NewSlot) && !baseAttributes.HasFlag(MethodAttributes.NewSlot))
                    baseAttributes |= MethodAttributes.NewSlot;
            }

            Disabled = false;
        }

        return (baseAttributes, implementationFlags);
    }

    public static ParameterAttributes GetParameterAttributes(DassieParser.Parameter_modifierContext modifier, bool hasDefault)
    {
        ParameterAttributes baseAttributes = ParameterAttributes.None;

        if (modifier != null && modifier.Ampersand_Greater() != null)
            baseAttributes = ParameterAttributes.In;
        else if (modifier != null && modifier.Less_Ampersand() != null)
            baseAttributes = ParameterAttributes.Out;

        if (hasDefault)
            baseAttributes |= ParameterAttributes.Optional;

        return baseAttributes;
    }

    public static TypeAttributes GetTypeAttributes(DassieParser.Type_kindContext typeKind, DassieParser.Type_access_modifierContext typeAccess, DassieParser.Nested_type_access_modifierContext nestedTypeAccess, DassieParser.Type_special_modifierContext modifiers, bool isNested)
    {
        TypeAttributes baseAttributes = TypeAttributes.Class;

        if (typeKind.Template() != null)
            baseAttributes = TypeAttributes.Interface | TypeAttributes.Abstract | TypeAttributes.BeforeFieldInit;

        if (isNested)
            baseAttributes |= TypeAttributes.NestedPublic;
        else
            baseAttributes |= TypeAttributes.Public;

        if (typeKind.Module() != null)
            baseAttributes |= TypeAttributes.Abstract | TypeAttributes.Sealed;

        else if (!baseAttributes.HasFlag(TypeAttributes.Interface) && (modifiers == null || modifiers.Open() == null))
            baseAttributes |= TypeAttributes.Sealed;

        if (typeAccess != null)
        {
            if (typeAccess.Global() != null)
                baseAttributes |= TypeAttributes.Public;
            else
                baseAttributes |= TypeAttributes.NotPublic;
        }
        else if (nestedTypeAccess != null)
        {
            if (nestedTypeAccess.Local() != null)
                baseAttributes |= TypeAttributes.NestedPrivate;

            else if (nestedTypeAccess.Protected() != null && nestedTypeAccess.Internal() != null)
                baseAttributes |= TypeAttributes.NestedFamORAssem;

            else if (nestedTypeAccess.Protected() != null)
                baseAttributes |= TypeAttributes.NestedFamily;

            else if (nestedTypeAccess.type_access_modifier().Global() != null)
                baseAttributes |= TypeAttributes.NestedPublic;

            else
                baseAttributes |= TypeAttributes.NestedAssembly;
        }

        return baseAttributes;
    }
}