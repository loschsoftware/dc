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
using System.Runtime.CompilerServices;

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

    internal static readonly Dictionary<Type[], Type> _createdUnionTypes = [];

    public static Type GenerateInlineUnionType(Type[] fields)
    {
        foreach (var existingUnion in _createdUnionTypes)
        {
            if (existingUnion.Key.SequenceEqual(fields))
                return existingUnion.Value;
        }

        TypeBuilder tb = Context.Module.DefineType(SymbolNameGenerator.GetInlineUnionTypeName(_createdUnionTypes.Count), TypeAttributes.Public | TypeAttributes.Sealed);
        tb.SetCustomAttribute(new(typeof(Union).GetConstructor([]), []));
        tb.SetCustomAttribute(new(typeof(CompilerGeneratedAttribute).GetConstructor([]), []));

        ConstructorBuilder defConstructor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, []);
        ILGenerator conIL = defConstructor.GetILGenerator();
        conIL.Emit(OpCodes.Ldarg_0);
        conIL.Emit(OpCodes.Call, typeof(object).GetConstructor([]));
        conIL.Emit(OpCodes.Ret);

        List<(Type Type, MethodInfo IsGetter, MethodInfo Getter)> tags = [];

        foreach (Type field in fields)
        {
            string propName = field.FullName;
            string isPropName = $"Is{field.FullName}";
            FieldBuilder propBackingField = tb.DefineField(SymbolNameGenerator.GetPropertyBackingFieldName(propName), field, FieldAttributes.Private);
            FieldBuilder isBackingField = tb.DefineField(SymbolNameGenerator.GetPropertyBackingFieldName(isPropName), typeof(bool), FieldAttributes.Private);
            PropertyBuilder prop = tb.DefineProperty(propName, PropertyAttributes.None, field, []);
            PropertyBuilder isProp = tb.DefineProperty(isPropName, PropertyAttributes.None, typeof(bool), []);

            MethodBuilder getter = tb.DefineMethod($"get_{propName}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, field, []);
            ILGenerator getterIL = getter.GetILGenerator();
            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, propBackingField);
            getterIL.Emit(OpCodes.Ret);
            prop.SetGetMethod(getter);

            MethodBuilder setter = tb.DefineMethod($"set_{propName}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, typeof(void), [field]);
            ILGenerator setterIL = setter.GetILGenerator();
            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ldarg_1);
            setterIL.Emit(OpCodes.Stfld, propBackingField);
            setterIL.Emit(OpCodes.Ret);
            prop.SetSetMethod(setter);

            MethodBuilder isPropGetter = tb.DefineMethod($"get_{isPropName}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, typeof(bool), []);
            ILGenerator isPropGetterIL = isPropGetter.GetILGenerator();
            isPropGetterIL.Emit(OpCodes.Ldarg_0);
            isPropGetterIL.Emit(OpCodes.Ldfld, isBackingField);
            isPropGetterIL.Emit(OpCodes.Ret);
            isProp.SetGetMethod(isPropGetter);

            MethodBuilder convertIntoTypeMethod = tb.DefineMethod("op_Implicit", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName, tb, [field]);
            ILGenerator convertIntoIL = convertIntoTypeMethod.GetILGenerator();
            convertIntoIL.Emit(OpCodes.Newobj, defConstructor);
            convertIntoIL.Emit(OpCodes.Dup);
            convertIntoIL.Emit(OpCodes.Dup);
            convertIntoIL.Emit(OpCodes.Ldc_I4_1);
            convertIntoIL.Emit(OpCodes.Stfld, isBackingField);
            convertIntoIL.Emit(OpCodes.Ldarg_0);
            convertIntoIL.Emit(OpCodes.Call, setter);
            convertIntoIL.Emit(OpCodes.Ret);

            MethodBuilder convertFromTypeMethod = tb.DefineMethod("op_Implicit", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName, field, [tb]);
            ILGenerator convertFromIL = convertFromTypeMethod.GetILGenerator();
            convertFromIL.Emit(OpCodes.Ldarg_0);
            convertFromIL.Emit(OpCodes.Call, getter);
            convertFromIL.Emit(OpCodes.Ret);

            tags.Add((field, isPropGetter, getter));
        }

        MethodBuilder getValueMethod = tb.DefineMethod("GetValue", MethodAttributes.Public, CallingConventions.HasThis, typeof(object), []);
        ILGenerator getValueIL = getValueMethod.GetILGenerator();
        getValueIL.DeclareLocal(typeof(object));
        Label endLabel = getValueIL.DefineLabel();

        foreach ((Type type, MethodInfo isGetter, MethodInfo getter) in tags)
        {
            Label nextTypeLabel = getValueIL.DefineLabel();

            getValueIL.Emit(OpCodes.Ldarg_0);
            getValueIL.Emit(OpCodes.Call, isGetter);
            getValueIL.Emit(OpCodes.Brfalse, nextTypeLabel);

            getValueIL.Emit(OpCodes.Ldarg_0);
            getValueIL.Emit(OpCodes.Call, getter);

            if (type.IsValueType)
                getValueIL.Emit(OpCodes.Box, type);

            getValueIL.Emit(OpCodes.Stloc_0);
            getValueIL.Emit(OpCodes.Br, endLabel);

            getValueIL.MarkLabel(nextTypeLabel);
        }

        getValueIL.MarkLabel(endLabel);
        getValueIL.Emit(OpCodes.Ldloc_0);
        getValueIL.Emit(OpCodes.Ret);

        MethodBuilder toStringMethod = tb.DefineMethod("ToString", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, CallingConventions.HasThis, typeof(string), []);
        ILGenerator toStringIL = toStringMethod.GetILGenerator();
        toStringIL.Emit(OpCodes.Ldarg_0);
        toStringIL.Emit(OpCodes.Call, getValueMethod);
        toStringIL.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
        toStringIL.Emit(OpCodes.Ret);

        Type union = tb.CreateType();
        _createdUnionTypes.Add(fields, union);
        return union;
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