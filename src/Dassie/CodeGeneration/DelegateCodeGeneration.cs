using Dassie.Meta;
using Dassie.Symbols;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Dassie.CodeGeneration;

internal static class DelegateCodeGeneration
{
    public static Type CreateDelegateType(Type returnType, Type[] paramTypes)
    {
        string typeName = SymbolNameGenerator.GetDelegateTypeName(returnType, paramTypes);

        if (Context.Types.Any(t => t.FullName == typeName))
            return Context.Types.First(t => t.FullName == typeName).FinishedType;

        TypeBuilder delegateType = Context.Module.DefineType(typeName, TypeAttributes.NotPublic);
        delegateType.SetCustomAttribute(new(typeof(CompilerGeneratedAttribute).GetConstructor([]), []));
        delegateType.SetParent(typeof(MulticastDelegate));

        ConstructorBuilder cb = delegateType.DefineConstructor(
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            CallingConventions.HasThis,
            [typeof(object), typeof(nint)]);

        MethodBuilder invokeMethod = delegateType.DefineMethod(
            "Invoke",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
            CallingConventions.HasThis,
            returnType,
            paramTypes);

        cb.SetImplementationFlags(MethodImplAttributes.Runtime);
        invokeMethod.SetImplementationFlags(MethodImplAttributes.Runtime);

        Type finishedType = delegateType.CreateType();

        TypeContext current = TypeContext.Current;
        TypeContext _ = new()
        {
            FullName = typeName,
            Builder = delegateType,
            FinishedType = finishedType
        };
        TypeContext.Current = current;

        return finishedType;
    }
}