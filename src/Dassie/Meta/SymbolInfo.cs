using Dassie.CLI.Helpers;
using Dassie.Runtime;
using Dassie.Text;
using Dassie.Text.Tooltips;
using System;
using System.Reflection.Emit;

namespace Dassie.Meta;

internal class SymbolInfo
{
    public enum SymType
    {
        Local,
        Parameter,
        Field
    }

    public SymType SymbolType { get; set; }

    public LocalInfo Local { get; set; }

    public ParamInfo Parameter { get; set; }

    public MetaFieldInfo Field { get; set; }

    public Type Type() => SymbolType switch
    {
        SymType.Local => Local.Builder.LocalType,
        SymType.Parameter => Parameter.Type,
        _ => Field.Builder.FieldType
    };

    public int Index() => SymbolType switch
    {
        SymType.Parameter => Parameter.Index,
        SymType.Local => Local.Index,
        _ => -1
    };

    public UnionValue Union() => SymbolType switch
    {
        SymType.Local => Local.Union,
        SymType.Parameter => Parameter.Union,
        _ => Field.Union
    };

    public Tooltip GetToolTip() => SymbolType switch
    {
        SymType.Local => TooltipGenerator.Local(Local.Name, !Local.IsConstant, Local.Builder),
        SymType.Parameter => TooltipGenerator.Parameter(Parameter.Name, Parameter.Type),
        _ => TooltipGenerator.Field(Field.Builder)
    };

    public Fragment GetFragment(int line, int col, int length, bool navigable = false) => new(
        line,
        col,
        length,
        SymbolType switch
        {
            SymType.Local => Color.LocalVariable,
            SymType.Parameter => Color.LocalValue,
            _ => Color.Field
        },
        navigable)
    {
        ToolTip = GetToolTip()
    };

    public void Load()
    {
        if (CurrentMethod.ByRefArguments.Contains(CurrentMethod.CurrentArg))
        {
            if (!IsMutable())
            {
                EmitErrorMessage(
                    0, 0, 0, // TODO: Get correct location
                    DS0095_ImmutableSymbolPassedByReference,
                    $"The symbol '{Name()}' is immutable and cannot be passed by reference.");
            }

            LoadAddress();
            return;
        }

        switch (SymbolType)
        {
            case SymType.Local:
                EmitLdloc(Local.Index);
                break;

            case SymType.Parameter:
                EmitLdarg(Parameter.Index);
                break;

            default:
                if (!Field.Builder.IsStatic)
                    EmitLdarg0IfCurrentType(Field.Builder.FieldType);

                CurrentMethod.IL.Emit(Field.Builder.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, Field.Builder);
                break;
        }

        if (Type().IsByRef || Type().IsByRefLike)
            CurrentMethod.IL.Emit(Type().GetElementType().GetLoadIndirectOpCode());
    }

    public void LoadAddress()
    {
        switch (SymbolType)
        {
            case SymType.Local:
                EmitLdloca(Local.Index);
                break;

            case SymType.Parameter:
                EmitLdarga(Parameter.Index);
                break;

            default:
                if (!Field.Builder.IsStatic)
                    EmitLdarg0IfCurrentType(Field.Builder.FieldType);

                CurrentMethod.IL.Emit(Field.Builder.IsStatic ? OpCodes.Ldsflda : OpCodes.Ldflda, Field.Builder);
                break;
        }
    }

    public void LoadAddressIfValueType()
    {
        switch (SymbolType)
        {
            case SymType.Local:
                if (Local.Builder.LocalType.IsValueType)
                    LoadAddress();
                else
                    Load();
                break;

            case SymType.Parameter:
                if (Parameter.Type.IsValueType)
                    LoadAddress();
                else
                    Load();
                break;

            default:
                if (Field.Builder.FieldType.IsValueType)
                    LoadAddress();
                else
                    Load();
                break;
        }
    }

    public void Set()
    {
        if (Type().IsByRef || Type().IsByRefLike)
        {
            CurrentMethod.IL.Emit(Type().GetElementType().GetSetIndirectOpCode());
            return;
        }

        switch (SymbolType)
        {
            case SymType.Local:
                EmitStloc(Local.Index);
                break;

            case SymType.Parameter:
                EmitStarg(Parameter.Index);
                break;

            default:
                if (Field.Builder.IsStatic)
                    CurrentMethod.IL.Emit(OpCodes.Stsfld, Field.Builder);
                else
                {
                    EmitLdarg0IfCurrentType(Field.Builder.FieldType);
                    CurrentMethod.IL.Emit(OpCodes.Stfld, Field.Builder);
                }
                break;
        }
    }

    public bool IsMutable() => SymbolType switch
    {
        SymType.Local => !Local.IsConstant,
        SymType.Parameter => Parameter.Builder.IsOut || Parameter.IsMutable,
        _ => true
    };

    public string Name() => SymbolType switch
    {
        SymType.Local => Local.Name,
        SymType.Parameter => Parameter.Name,
        _ => Field.Builder.Name
    };
}