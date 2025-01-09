using Dassie.Core;
using Dassie.Helpers;
using Dassie.Meta;
using Dassie.Parser;
using Dassie.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dassie.CodeGeneration;

internal static class UnionTypeCodeGeneration
{
    public static void GenerateUnionType(DassieParser.TypeContext context)
    {
        TypeContext current = TypeContext.Current;
        TypeContext.Current.IsUnion = true;
        TypeContext.Current.Builder.SetCustomAttribute(new(typeof(Union).GetConstructor([]), []));

        if (context.type_block() == null || context.type_block().type_member() == null)
            return;

        foreach (DassieParser.Type_memberContext member in context.type_block().type_member())
        {
            GenerateUnionTagType(member);
            TypeContext.Current = current;
        }

        TypeContext.Current.FinishedType = TypeContext.Current.Builder.CreateType();
    }

    public static void GenerateUnionTagType(DassieParser.Type_memberContext context)
    {
        TypeContext tc = TypeContext.Current;
        string typeName = context.Identifier().GetText();

        TypeBuilder tagType = TypeContext.Current.Builder.DefineNestedType(typeName, TypeAttributes.NestedPublic | TypeAttributes.SpecialName);
        tagType.SetParent(TypeContext.Current.Builder);

        string isPropName = $"Is{typeName}";
        string isPropBackingFieldName = SymbolNameGenerator.GetPropertyBackingFieldName(isPropName);

        FieldBuilder isPropBackingField = tc.Builder.DefineField(isPropBackingFieldName, typeof(bool), FieldAttributes.Private);
        PropertyBuilder isProp = tc.Builder.DefineProperty(isPropName, PropertyAttributes.None, typeof(bool), []);
        tc.Properties.Add(isProp);

        MethodBuilder isPropGetter = tc.Builder.DefineMethod($"get_{isPropName}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, typeof(bool), []);
        ILGenerator ilIsPropGet = isPropGetter.GetILGenerator();
        ilIsPropGet.Emit(OpCodes.Ldarg_0);
        ilIsPropGet.Emit(OpCodes.Ldfld, isPropBackingField);
        ilIsPropGet.Emit(OpCodes.Ret);
        isProp.SetGetMethod(isPropGetter);

        TypeContext childContext = new()
        {
            FullName = typeName,
            Builder = tagType,

        };

        tc.Children.Add(childContext);

        if (context.parameter_list() != null)
        {
            List<FieldInfo> fields = [];

            foreach (DassieParser.ParameterContext param in context.parameter_list().parameter())
            {
                string paramName = param.Identifier().GetText();
                string fieldName = SymbolNameGenerator.GetPropertyBackingFieldName(paramName);
                Type paramType = SymbolResolver.ResolveTypeName(param.type_name());

                FieldBuilder backingField = tagType.DefineField(fieldName, paramType, FieldAttributes.Private);
                PropertyBuilder prop = tagType.DefineProperty(paramName, PropertyAttributes.None, paramType, []);
                childContext.Properties.Add(prop);

                MethodBuilder getter = tagType.DefineMethod($"get_{paramName}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, paramType, []);
                ILGenerator ilGet = getter.GetILGenerator();
                ilGet.Emit(OpCodes.Ldarg_0);
                ilGet.Emit(OpCodes.Ldfld, backingField);
                ilGet.Emit(OpCodes.Ret);
                prop.SetGetMethod(getter);

                if (param.Var() != null)
                {
                    MethodBuilder setter = tagType.DefineMethod($"set_{paramName}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, typeof(void), [paramType]);
                    ILGenerator ilSet = setter.GetILGenerator();
                    ilSet.Emit(OpCodes.Ldarg_0);
                    ilSet.Emit(OpCodes.Ldarg_1);
                    ilSet.Emit(OpCodes.Stfld, backingField);
                    ilSet.Emit(OpCodes.Ret);
                    prop.SetSetMethod(setter);
                }

                fields.Add(backingField);
            }

            ConstructorBuilder cb = tagType.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, fields.Select(f => f.FieldType).ToArray());
            ILGenerator il = cb.GetILGenerator();

            MethodContext current = CurrentMethod;
            childContext.ConstructorContexts.Add(new()
            {
                IL = il,
                ConstructorBuilder = cb
            });

            EmitLdarg(0);
            EmitLdcI4(1);
            il.Emit(OpCodes.Stfld, isPropBackingField);

            for (int i = 0; i < fields.Count; i++)
            {
                EmitLdarg(0);
                EmitLdarg(i + 1);
                il.Emit(OpCodes.Stfld, fields[i]); // Going through the property is unnecessary since the setters never have side effects
            }

            if (!tagType.IsValueType)
            {
                EmitLdarg(0);
                il.Emit(OpCodes.Call, typeof(object).GetConstructor([]));
            }

            il.Emit(OpCodes.Ret);
            CurrentMethod = current;
        }

        childContext.FinishedType = tagType.CreateType();
    }
}