using LoschScript.Meta;
using LoschScript.Parser;
using LoschScript.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace LoschScript.CodeGeneration;

internal static class SymbolResolver
{
    public static object ResolveIdentifier(LoschScriptParser.Identifier_atomContext context, int row, int col, int len, bool noEmitFragments = false)
    {
        string text;

        if (context.Identifier() != null)
            text = context.Identifier().GetText();
        else
            text = context.full_identifier().GetText();

        // 1. Parameters
        if (CurrentMethod.Parameters.Any(p => p.Name == text))
            return CurrentMethod.Parameters.First(p => p.Name == text);

        // 2. Locals
        if (CurrentMethod.Locals.Any(p => p.Name == text))
            return CurrentMethod.Locals.First(p => p.Name == text);

        // 3. Members of current class
        if (TypeContext.Current.Methods.Select(m => m.Builder).Any(m => m.Name == text))
            return TypeContext.Current.Methods.Select(m => m.Builder).Where(m => m.Name == text).ToList();

        // 4. Members of type-imported types ("global members")
        if (TryGetGlobalMember(text, out object globals, row, col, len))
            return globals;

        // 5. Other classes, including aliases
        if (TryGetType(text, out Type t, row, col, len, noEmitFragments))
            return t;

        // 6. Members of other classes
    }

    private static bool TryGetType(string name, out Type type, int row, int col, int len, bool noEmitFragments = false)
    {
        type = Type.GetType(name);

        if (type == null)
        {
            List<Assembly> allAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            allAssemblies.AddRange(Context.ReferencedAssemblies);

            List<Assembly> assemblies = allAssemblies.Where(_a => _a.GetType(name) != null).ToList();
            if (assemblies.Any())
            {
                type = assemblies.First().GetType(name);
                
                if (!noEmitFragments)
                {
                    CurrentFile.Fragments.Add(new()
                    {
                        Line = row,
                        Column = col,
                        Length = name.Length,
                        Color = TooltipGenerator.ColorForType(type.GetTypeInfo()),
                        ToolTip = TooltipGenerator.Type(type.GetTypeInfo(), true, true, false)
                    });
                }

                return true;
            }

            foreach (string ns in CurrentFile.Imports.Concat(Context.GlobalImports))
            {
                string n = $"{ns}.{name}";

                type = Type.GetType(n);

                if (type != null)
                    goto FoundType;

                List<Assembly> _allAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                _allAssemblies.AddRange(Context.ReferencedAssemblies);

                List<Assembly> _assemblies = _allAssemblies.Where(a => a.GetType(n) != null).ToList();
                if (_assemblies.Any())
                {
                    type = _assemblies.First().GetType(n);
                    goto FoundType;
                }

                if (type != null)
                    goto FoundType;
            }

            foreach (string originalName in CurrentFile.Aliases.Where(a => a.Alias == name).Select(a => a.Name))
            {
                type = Type.GetType(originalName);

                if (type != null)
                    goto FoundType;
            }
        }

    FoundType:

        if (type == null)
        {
            EmitErrorMessage(
                row,
                col,
                len,
                LS0009_TypeNotFound,
                $"The name '{name}' could not be resolved.");

            return false;
        }
        else
        {
            if (!noEmitFragments)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Line = row,
                    Column = col,
                    Length = name.Length,
                    Color = TooltipGenerator.ColorForType(type.GetTypeInfo()),
                    ToolTip = TooltipGenerator.Type(type.GetTypeInfo(), true, true, false)
                });
            }

            return true;
        }
    }

    private static bool TryGetGlobalMember(string name, out object members, int row, int col, int len)
    {
        foreach (string type in CurrentFile.ImportedTypes)
        {
            if (TryGetType(type, out Type t, row, col, len, true))
            {
                // 1. Methods
                if (t.GetMethods().Where(m => m.Name == name).Any())
                {
                    members = t.GetMethods().Where(m => m.Name == name).ToList();
                    return true;
                }

                // 2. Fields
                else if (t.GetField(name) != null)
                {
                    members = t.GetField(name);
                    return true;
                }

                // 3. Properties
                else if (t.GetProperty(name) != null)
                {
                    members = t.GetProperty(name);
                    return true;
                }
            }
        }

        members = null;
        return false;
    }
}