using Dassie.Helpers;
using Dassie.Symbols;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Dassie.CodeGeneration;

internal static class InitializedArrayCodeGeneration
{
    private static int _dataIndex;
    private static int _arrayIndex;

    // TODO: Unfinished
    public static Type DefineInlineArray(byte[] data)
    {
        int idx = _arrayIndex++;
        string parentName = SymbolNameGenerator.GetInlineArrayContainerTypeName(idx);
        string childName = SymbolNameGenerator.GetInlineArrayTypeName(idx);

        TypeBuilder parent = Context.Module.DefineType(parentName, TypeAttributes.NotPublic | TypeAttributes.Sealed);
        parent.SetCustomAttribute(new(typeof(CompilerGeneratedAttribute).GetConstructor([]), []));

        TypeBuilder tb = parent.DefineNestedType(childName, TypeAttributes.NestedAssembly | TypeAttributes.ExplicitLayout | TypeAttributes.Sealed, typeof(ValueType),
            PackingSize.Size1,
            data.Length);

        tb.CreateType();

        parent.DefineField($"{SymbolNameGenerator.GeneratedPrefix}$Data", tb, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly)
            .SetOffset(0);

        return parent.CreateType();
    }

    public static FieldBuilder DefineInitializedArray(byte[] data)
    {
        return Context.Module.DefineInitializedData(SymbolNameGenerator.GetInitializedDataFieldName(_dataIndex++), data, FieldAttributes.Public | FieldAttributes.Static);
    }

    public static byte[] Serialize(Array data)
    {
        if (!TypeHelpers.IsNumericType(data.GetType().GetElementType()))
            throw new NotImplementedException();

        List<byte> result = [];
        foreach (object item in data)
        {
            if (item.GetType() == typeof(byte))
            {
                result.Add((byte)item);
                continue;
            }

            MethodInfo method = typeof(BitConverter).GetMethod("GetBytes", BindingFlags.Public | BindingFlags.Static, [item.GetType()]) ?? throw new ArgumentException();
            result.AddRange((byte[])method.Invoke(null, [item]));
        }

        return result.ToArray();
    }
}