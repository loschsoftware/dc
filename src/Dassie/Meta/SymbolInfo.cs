using Dassie.Helpers;
using Dassie.Runtime;
using Dassie.Text;
using Dassie.Text.Tooltips;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Dassie.Meta;

internal class SymbolInfo
{
    public enum SymType
    {
        Local,
        Parameter,
        Field,
        Property
    }

    public SymType SymbolType { get; set; }

    public LocalInfo Local { get; set; }

    public ParamInfo Parameter { get; set; }

    public MetaFieldInfo Field { get; set; }

    public PropertyInfo Property { get; set; }

    public Type Type() => SymbolType switch
    {
        SymType.Local => Local.Builder.LocalType,
        SymType.Parameter => Parameter.Type,
        SymType.Field => Field.Builder.FieldType,
        _ => Property.PropertyType
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
        SymType.Field => Field.Union,
        _ => default
    };

    public Tooltip GetToolTip() => SymbolType switch
    {
        SymType.Local => TooltipGenerator.Local(Local.Name, !Local.IsConstant, Local.Builder),
        SymType.Parameter => TooltipGenerator.Parameter(Parameter.Name, Parameter.Type),
        SymType.Field => TooltipGenerator.Field(Field.Builder),
        _ => TooltipGenerator.Property(Property)
    };

    public Fragment GetFragment(int line, int col, int length, bool navigable = false) => new(
        line,
        col,
        length,
        SymbolType switch
        {
            SymType.Local => Color.LocalVariable,
            SymType.Parameter => Color.LocalValue,
            SymType.Field => Color.Field,
            _ => Color.Property
        },
        navigable)
    {
        ToolTip = GetToolTip()
    };

    public void Load(bool implicitLoadClosureContainerInstanceField = false, string localName = "", bool skipLdind = false)
    {
        if ((CurrentMethod.LoadReference && !Type().IsByRef) || CurrentMethod.ByRefArguments.Contains(CurrentMethod.CurrentArg))
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

        if (implicitLoadClosureContainerInstanceField && CurrentMethod.Locals.Any(l => l.Name == localName))
            EmitLdloc(CurrentMethod.Locals.First(l => l.Name == localName).Index);
        else if (implicitLoadClosureContainerInstanceField)
            EmitLdarg(0);

        if (VisitorStep1CurrentMethod != null && VisitorStep1CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == Name()))
        {
            (FieldInfo fld, _) = VisitorStep1CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == Name()).Value;

            try
            {
                CurrentMethod.IL.Emit(OpCodes.Ldfld, fld);
            }
            catch { } // No idea why, but sometimes it throws a NullReferenceException somewhere deep inside Emit()...
        }

        else if (CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == Name()))
        {
            (FieldInfo fld, _) = CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == Name()).Value;

            try
            {
                CurrentMethod.IL.Emit(OpCodes.Ldfld, fld);
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

                case SymType.Property:
                    EmitCall(Property.PropertyType, Property.GetGetMethod());
                    break;

                default:
                    if (Field.Builder.IsLiteral)
                    {
                        EmitConst(Field.Builder.GetRawConstantValue());
                        break;
                    }

                    if (Field.Builder.GetRequiredCustomModifiers().Contains(typeof(IsVolatile)))
                        CurrentMethod.IL.Emit(OpCodes.Volatile);

                    if (!Field.Builder.IsStatic)
                        EmitLdarg0IfCurrentType(Field.Builder.FieldType);

                    CurrentMethod.IL.Emit(Field.Builder.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, Field.Builder);
                    break;
            }
        }

        if (CurrentMethod.LoadIndirectIfByRef && !skipLdind && Type().IsByRef /*|| Type().IsByRefLike*/)
            CurrentMethod.IL.Emit(Type().GetElementType().GetLoadIndirectOpCode());
    }

    public void LoadAddress(bool implicitLoadClosureContainerInstanceField = false, string localName = "")
    {
        if (implicitLoadClosureContainerInstanceField && CurrentMethod.Locals.Any(l => l.Name == localName))
            EmitLdloc(CurrentMethod.Locals.First(l => l.Name == localName).Index);
        else if (implicitLoadClosureContainerInstanceField)
            EmitLdarg(0);

        if (VisitorStep1CurrentMethod != null && VisitorStep1CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == Name()))
        {
            (FieldInfo fld, _) = VisitorStep1CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == Name()).Value;
            CurrentMethod.IL.Emit(OpCodes.Ldflda, fld);
            return;
        }

        if (CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == Name()))
        {
            (FieldInfo fld, _) = CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == Name()).Value;
            CurrentMethod.IL.Emit(OpCodes.Ldflda, fld);
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

            case SymType.Property:
                EmitCall(Property.PropertyType, Property.GetGetMethod());
                break;

            default:
                if (Field.Builder.GetRequiredCustomModifiers().Contains(typeof(IsVolatile)))
                    CurrentMethod.IL.Emit(OpCodes.Volatile);

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

            case SymType.Property:
                if (Property.PropertyType.IsValueType)
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

    public void Set(bool implicitLoadClosureContainerInstanceField = false, string localName = "", bool setIndirectIfByRef = true)
    {
        if ((Type().IsByRef /*|| Type().IsByRefLike*/) && setIndirectIfByRef)
        {
            CurrentMethod.IL.Emit(Type().GetElementType().GetSetIndirectOpCode());
            return;
        }

        if (implicitLoadClosureContainerInstanceField && CurrentMethod.Locals.Any(l => l.Name == localName))
            EmitLdloc(CurrentMethod.Locals.First(l => l.Name == localName).Index);
        else if (implicitLoadClosureContainerInstanceField)
            EmitLdarg(0);

        if (VisitorStep1CurrentMethod != null && VisitorStep1CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == Name()))
        {
            (FieldInfo fld, _) = VisitorStep1CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == Name()).Value;
            CurrentMethod.IL.Emit(OpCodes.Stfld, fld);
            return;
        }

        if (CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == Name()))
        {
            (FieldInfo fld, _) = CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == Name()).Value;
            CurrentMethod.IL.Emit(OpCodes.Stfld, fld);
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

            case SymType.Property:
                EmitCall(Property.PropertyType, Property.GetSetMethod());
                break;

            default:
                if (Field.Builder.GetRequiredCustomModifiers().Contains(typeof(IsVolatile)))
                    CurrentMethod.IL.Emit(OpCodes.Volatile);

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
        SymType.Property => Property.GetSetMethod() != null,
        _ => true
    };

    public string Name() => SymbolType switch
    {
        SymType.Local => Local.Name,
        SymType.Parameter => Parameter.Name,
        SymType.Property => Property.Name,
        _ => Field.Builder.Name
    };

    public static bool operator ==(SymbolInfo left, SymbolInfo right)
    {
        if (left is null || right is null) return false;

        if (left.Local != null && right.Local != null && left.Local == right.Local) return true;
        if (left.Parameter != null && right.Parameter != null && left.Parameter == right.Parameter) return true;
        if (left.Field != null && right.Field != null && left.Field == right.Field) return true;
        if (left.Property != null && right.Property != null && left.Property == right.Property) return true;

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