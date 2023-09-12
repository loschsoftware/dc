using LoschScript.Meta;
using LoschScript.Parser;
using LoschScript.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LoschScript.CodeGeneration;

internal static class SymbolResolver
{
    public static object GetSmallestTypeFromLeft(LoschScriptParser.Full_identifierContext fullId, int row, int col, int len, out int firstUnusedPart, bool noEmitFragments = false)
    {
        string[] parts = fullId.Identifier().Select(f => f.GetText()).ToArray();
        firstUnusedPart = 0;

        string typeString = "";

        for (int i = 1; i < parts.Length; i++)
        {
            typeString = string.Join(".", parts[0..i]);
            firstUnusedPart++;

            if (ResolveIdentifier(typeString, row, col, len, true) is Type t)
            {
                if (!noEmitFragments)
                {
                    CurrentFile.Fragments.Add(new()
                    {
                        Line = row,
                        Column = col,
                        Length = len,
                        Color = TooltipGenerator.ColorForType(t.GetTypeInfo()),
                        IsNavigationTarget = false,
                        ToolTip = TooltipGenerator.Type(t.GetTypeInfo(), false, true)
                    });
                }

                return t;
            }
        }

        // First part of full_id could also be parameter, local, member of current class.
        return ResolveIdentifier(
            parts[0],
            row,
            col,
            len,
            noEmitFragments);
    }

    public static object ResolveIdentifier(string text, int row, int col, int len, bool noEmitFragments = false)
    {
        // 1. Parameters
        if (CurrentMethod.Parameters.Any(p => p.Name == text))
            return CurrentMethod.Parameters.First(p => p.Name == text);

        // 2. Locals
        if (CurrentMethod.Locals.Any(p => p.Name == text))
            return CurrentMethod.Locals.First(p => p.Name == text);

        // 3. Members of current class
        if (TypeContext.Current.Methods.Select(m => m.Builder).Any(m => m.Name == text))
            return TypeContext.Current.Methods.Select(m => m.Builder).First(m => m.Name == text);

        // 4. Members of type-imported types ("global members")
        if (TryGetGlobalMember(text, out object globals, row, col, len))
            return globals;

        // 5. Other classes, including aliases
        if (TryGetType(text, out Type t, row, col, len, noEmitFragments))
            return t;

        // 6. Members of other classes
        return null;
    }

    public static object ResolveMember(Type type, string name, int row, int col, int len, bool noEmitFragments = false, Type[] argumentTypes = null, BindingFlags flags = BindingFlags.Public)
    {
        // 1. Fields
        FieldInfo f = type.GetField(name, flags);
        if (f != null)
        {
            if (!noEmitFragments)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Line = row,
                    Column = col,
                    Length = len,
                    Color = Text.Color.Field,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Field(f)
                });
            }

            return f;
        }

        // 2. Properties
        PropertyInfo p = type.GetProperty(name, flags);
        if (p != null)
        {
            if (!noEmitFragments)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Line = row,
                    Column = col,
                    Length = len,
                    Color = Text.Color.Property,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Property(p)
                });
            }

            return p;
        }

        // 3. Methods

        argumentTypes ??= Type.EmptyTypes;

        MethodInfo[] methods = type.GetMethods()
            .Where(m => m.Name == name)
            .Where(m => m.GetParameters().Length == argumentTypes.Length)
            .ToArray();

        if (methods.Any())
        {
            MethodInfo final = null;

            foreach (MethodInfo possibleMethod in methods)
            {
                if (final != null)
                    break;

                if (possibleMethod.GetParameters().Length == 0 && argumentTypes.Length == 0)
                {
                    final = possibleMethod;
                    break;
                }

                for (int i = 0; i < possibleMethod.GetParameters().Length; i++)
                {
                    if (argumentTypes[i] == possibleMethod.GetParameters()[i].ParameterType || possibleMethod.GetParameters()[i].ParameterType == typeof(object))
                    {
                        if (possibleMethod.GetParameters()[i].ParameterType == typeof(object))
                            CurrentMethod.ParameterBoxIndices.Add(i);

                        if (i == possibleMethod.GetParameters().Length - 1)
                        {
                            final = possibleMethod;
                            break;
                        }
                    }

                    else
                        break;
                }
            }

            if (final == null)
                goto Error;

            if (type != typeof(object) && final.DeclaringType == typeof(object))
                CurrentMethod.BoxCallingType = true;

            if (!noEmitFragments)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Line = row,
                    Column = col,
                    Length = len,
                    Color = Text.Color.Function,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Function(methods.First())
                });
            }

            return final;
        }

        Error:

        EmitErrorMessage(
            row,
            col,
            len,
            LS0002_MethodNotFound,
            $"Type '{type.FullName}' has no compatible member called '{name}'.");

        return null;
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