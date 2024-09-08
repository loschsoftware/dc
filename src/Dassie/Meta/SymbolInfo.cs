using Dassie.CLI.Helpers;
using Dassie.Runtime;
using Dassie.Text;
using Dassie.Text.Tooltips;
using System;
using System.Linq;
using System.Reflection;
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

        if (VisitorStep1CurrentMethod != null && VisitorStep1CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == Name()))
        {
            FieldInfo fld = VisitorStep1CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == Name()).Value;

            try
            {
                CurrentMethod.IL.Emit(OpCodes.Ldsfld, fld);
            }
            catch { } // No idea why, but sometimes it throws a NullReferenceException somewhere deep inside Emit()...
        }

        else if (CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == Name()))
        {
            FieldInfo fld = CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == Name()).Value;

            try
            {
                CurrentMethod.IL.Emit(OpCodes.Ldsfld, fld);
            }
            catch { } // No idea why, but sometimes it throws a NullReferenceException somewhere deep inside Emit()...
        }

        else
        {
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
        }

        if (Type().IsByRef /*|| Type().IsByRefLike*/)
            CurrentMethod.IL.Emit(Type().GetElementType().GetLoadIndirectOpCode());
    }

    public void LoadAddress()
    {
        if (VisitorStep1CurrentMethod != null && VisitorStep1CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == Name()))
        {
            FieldInfo fld = VisitorStep1CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == Name()).Value;
            CurrentMethod.IL.Emit(OpCodes.Ldsflda, fld);
            return;
        }

        if (CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == Name()))
        {
            FieldInfo fld = CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == Name()).Value;
            CurrentMethod.IL.Emit(OpCodes.Ldsflda, fld);
            return;
        }

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
        if (Type().IsByRef /*|| Type().IsByRefLike*/)
        {
            CurrentMethod.IL.Emit(Type().GetElementType().GetSetIndirectOpCode());
            return;
        }

        if (VisitorStep1CurrentMethod != null && VisitorStep1CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == Name()))
        {
            FieldInfo fld = VisitorStep1CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == Name()).Value;
            CurrentMethod.IL.Emit(OpCodes.Stsfld, fld);
            return;
        }

        if (CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == Name()))
        {
            FieldInfo fld = CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == Name()).Value;
            CurrentMethod.IL.Emit(OpCodes.Stsfld, fld);
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

        //if (CurrentMethod.AdditionalStorageLocations.TryGetValue(this, out FieldInfo fld))
        //{
        //    Load();
        //    CurrentMethod.IL.Emit(OpCodes.Stsfld, fld);
        //}
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

    public static bool operator ==(SymbolInfo left, SymbolInfo right)
    {
        if (left is null || right is null) return false;

        if (left.Local != null && right.Local != null && left.Local == right.Local) return true;
        if (left.Parameter != null && right.Parameter != null && left.Parameter == right.Parameter) return true;
        if (left.Field != null && right.Field != null && left.Field == right.Field) return true;

        return false;
    }

    public static bool operator !=(SymbolInfo left, SymbolInfo right) => !(left == right);

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (obj is not SymbolInfo) return false;

        return this == (SymbolInfo)obj;
    }

    public override int GetHashCode()
    {
        // Who cares anyway
        return 0;
    }
}