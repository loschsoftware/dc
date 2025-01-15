using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Dassie.CodeGeneration;

internal static class EventDefaultHandlerCodeGeneration
{
    public static void GenerateDefaultAddHandlerImplementation(FieldInfo eventField)
    {
        CurrentMethod.IL.DeclareLocal(eventField.FieldType);
        CurrentMethod.IL.DeclareLocal(eventField.FieldType);
        CurrentMethod.IL.DeclareLocal(eventField.FieldType);
        Label loopLabel = CurrentMethod.IL.DefineLabel();

        OpCode ldfldOpCode = eventField.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld;
        OpCode ldfldaOpCode = eventField.IsStatic ? OpCodes.Ldsflda : OpCodes.Ldflda;

        EmitLdarg(0);
        CurrentMethod.IL.Emit(ldfldOpCode, eventField);
        EmitStloc(0);

        CurrentMethod.IL.MarkLabel(loopLabel);
        EmitLdloc(0);
        EmitStloc(1);
        EmitLdloc(1);
        EmitLdarg(1);
        CurrentMethod.IL.Emit(OpCodes.Call, typeof(Delegate).GetMethod("Combine", BindingFlags.Public | BindingFlags.Static, [typeof(Delegate), typeof(Delegate)]));
        CurrentMethod.IL.Emit(OpCodes.Castclass, eventField.FieldType);
        EmitStloc(2);
        EmitLdarg(0);
        CurrentMethod.IL.Emit(ldfldaOpCode, eventField);
        EmitLdloc(2);
        EmitLdloc(1);
        CurrentMethod.IL.Emit(OpCodes.Call, typeof(Interlocked).GetMethods().Where(m => m.Name == "CompareExchange" && m.IsGenericMethod).Single().MakeGenericMethod([eventField.FieldType]));
        EmitStloc(0);
        EmitLdloc(0);
        EmitLdloc(1);
        CurrentMethod.IL.Emit(OpCodes.Bne_Un, loopLabel);
        
        CurrentMethod.IL.Emit(OpCodes.Ret);
    }

    public static void GenerateDefaultRemoveHandlerImplementation(FieldInfo eventField)
    {
        CurrentMethod.IL.DeclareLocal(eventField.FieldType);
        CurrentMethod.IL.DeclareLocal(eventField.FieldType);
        CurrentMethod.IL.DeclareLocal(eventField.FieldType);
        Label loopLabel = CurrentMethod.IL.DefineLabel();

        OpCode ldfldOpCode = eventField.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld;
        OpCode ldfldaOpCode = eventField.IsStatic ? OpCodes.Ldsflda : OpCodes.Ldflda;

        EmitLdarg(0);
        CurrentMethod.IL.Emit(ldfldOpCode, eventField);
        EmitStloc(0);

        CurrentMethod.IL.MarkLabel(loopLabel);
        EmitLdloc(0);
        EmitStloc(1);
        EmitLdloc(1);
        EmitLdarg(1);
        CurrentMethod.IL.Emit(OpCodes.Call, typeof(Delegate).GetMethod("Remove", BindingFlags.Public | BindingFlags.Static, [typeof(Delegate), typeof(Delegate)]));
        CurrentMethod.IL.Emit(OpCodes.Castclass, eventField.FieldType);
        EmitStloc(2);
        EmitLdarg(0);
        CurrentMethod.IL.Emit(ldfldaOpCode, eventField);
        EmitLdloc(2);
        EmitLdloc(1);
        CurrentMethod.IL.Emit(OpCodes.Call, typeof(Interlocked).GetMethods().Where(m => m.Name == "CompareExchange" && m.IsGenericMethod).Single().MakeGenericMethod([eventField.FieldType]));
        EmitStloc(0);
        EmitLdloc(0);
        EmitLdloc(1);
        CurrentMethod.IL.Emit(OpCodes.Bne_Un, loopLabel);

        CurrentMethod.IL.Emit(OpCodes.Ret);
    }
}