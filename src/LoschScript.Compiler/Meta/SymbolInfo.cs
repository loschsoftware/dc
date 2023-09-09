using LoschScript.CLI;
using LoschScript.Runtime;
using LoschScript.Text;
using LoschScript.Text.Tooltips;
using System;
using System.Reflection.Emit;

namespace LoschScript.Meta;

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

    public void Load() => Helpers.LoadSymbol(this);

    public void LoadAddress() => Helpers.LoadSymbolAddress(this);

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
        switch (SymbolType)
        {
            case SymType.Local:
                EmitStloc(CurrentMethod.IL, Local.Index);
                break;

            case SymType.Parameter:
                
                break;

            default:
                if (Field.Builder.IsStatic)
                    CurrentMethod.IL.Emit(OpCodes.Stsfld, Field.Builder);
                else
                {
                    EmitLdarg0IfCurrentType(CurrentMethod.IL, Field.Builder.FieldType);
                    CurrentMethod.IL.Emit(OpCodes.Stfld, Field.Builder);
                }
                break;
        }
    }

    public bool IsMutable() => SymbolType switch
    {
        SymType.Local => !Local.IsConstant,
        SymType.Parameter => Parameter.Builder.IsOut,
        _ => true
    };

    public string Name() => SymbolType switch
    {
        SymType.Local => Local.Name,
        SymType.Parameter => Parameter.Name,
        _ => Field.Builder.Name
    };
}