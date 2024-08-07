﻿using Dassie.Meta;
using Dassie.Parser;
using Dassie.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dassie.CodeGeneration;

internal static class SymbolResolver
{
    public class EnumValueInfo
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public Type EnumType { get; set; }
    }

    public static object GetSmallestTypeFromLeft(DassieParser.Full_identifierContext fullId, Type[] typeArgs, int row, int col, int len, out int firstUnusedPart, bool noEmitFragments = false)
    {
        string[] parts = fullId.Identifier().Select(f => f.GetText()).ToArray();
        firstUnusedPart = 0;

        string typeString = "";

        for (int i = 0; i < parts.Length; i++)
        {
            typeString = string.Join(".", parts[0..(i + 1)]);
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

        firstUnusedPart = Math.Min(1, fullId.Identifier().Length - 1);

        // First part of full_id could also be parameter, local, member of current class.
        return ResolveIdentifier(
            parts[0],
            row,
            col,
            len,
            noEmitFragments);
    }

    public static object ResolveIdentifier(string text, int row, int col, int len, bool noEmitFragments = false, bool throwErrors = true)
    {
        // 1. Parameters
        if (CurrentMethod.Parameters.Any(p => p.Name == text))
            return CurrentMethod.Parameters.First(p => p.Name == text);

        // 2. Locals
        if (CurrentMethod.Locals.Any(p => p.Name == text))
        {
            LocalInfo loc = CurrentMethod.Locals.First(p => p.Name == text);

            if (!loc.IsAvailable && throwErrors)
            {
                EmitErrorMessage(
                    row,
                    col,
                    len,
                    DS0062_LocalOutsideScope,
                    $"The local '{loc.Name}' is not in scope.");
            }

            return loc;
        }

        // 3. Members of current class
        if (TypeContext.Current.Methods.Select(m => m.Builder).Where(m => m != null).Any(m => m.Name == text))
            return TypeContext.Current.Methods.Select(m => m.Builder).First(m => m.Name == text);

        if (TypeContext.Current.Fields.Select(f => f.Builder).Where(f => f != null).Any(f => f.Name == text))
            return TypeContext.Current.Fields.Select(f => f.Builder).First(f => f.Name == text);

        // 4. Members of type-imported types ("global members")
        if (TryGetGlobalMember(text, out object globals, row, col, len))
            return globals;

        // 5. Other classes, including aliases
        if (TryGetType(text, out Type t, row, col, len, noEmitFragments))
            return t;

        // 6. Members of other classes
        return null;
    }

    static int memberIndex = -1;
    public static object ResolveMember(Type type, string name, int row, int col, int len, bool noEmitFragments = false, Type[] argumentTypes = null, BindingFlags flags = BindingFlags.Public, bool throwErrors = true)
    {
        memberIndex++;

        if (type.IsByRef)
            type = type.GetElementType();

        if (type is TypeBuilder tb)
            return ResolveMember(tb, name, row, col, len, noEmitFragments, argumentTypes, flags, throwErrors);

        // 0. Constructors
        if (name == type.Name)
        {
            argumentTypes ??= Type.EmptyTypes;

            ConstructorInfo[] cons = type.GetConstructors()
                .Where(c => c.GetParameters().Length == argumentTypes.Length)
                .ToArray();

            if (!CurrentMethod.ParameterBoxIndices.ContainsKey(memberIndex))
                CurrentMethod.ParameterBoxIndices.Add(memberIndex, new());

            if (cons.Any())
            {
                ConstructorInfo final = null;

                foreach (ConstructorInfo possibleMethod in cons)
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
                        if (argumentTypes[i] == possibleMethod.GetParameters()[i].ParameterType || possibleMethod.GetParameters()[i].ParameterType.IsAssignableFrom(argumentTypes[i]))
                        {
                            if (possibleMethod.GetParameters()[i].ParameterType == typeof(object))
                            {
                                CurrentMethod.ParameterBoxIndices[memberIndex].Add(i);
                            }

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

                for (int i = 0; i < final.GetParameters().Length; i++)
                {
                    if (CurrentMethod.ParameterBoxIndices[memberIndex].Contains(i)
                        && final.GetParameters()[i].ParameterType != typeof(object))
                    {
                        CurrentMethod.ParameterBoxIndices.Remove(i);
                    }
                }

                if (type != typeof(object) && final.DeclaringType == typeof(object))
                    CurrentMethod.BoxCallingType = true;

                if (!noEmitFragments)
                {
                    CurrentFile.Fragments.Add(new()
                    {
                        Line = row,
                        Column = col,
                        Length = len,
                        Color = Dassie.Text.Color.Function,
                        IsNavigationTarget = false,
                        ToolTip = TooltipGenerator.Constructor(final)
                    });
                }

                return final;
            }
        }

        // 1. Fields
        FieldInfo f = f = type.GetField(name/*, flags*/);
        if (f != null)
        {
            if (type.IsEnum)
            {
                if (!noEmitFragments)
                {
                    CurrentFile.Fragments.Add(new()
                    {
                        Line = row,
                        Column = col,
                        Length = len,
                        Color = Dassie.Text.Color.EnumField,
                        IsNavigationTarget = false,
                        ToolTip = TooltipGenerator.EnumField(f)
                    });
                }

                return new EnumValueInfo()
                {
                    Name = f.Name,
                    Value = f.GetRawConstantValue(),
                    EnumType = type
                };
            }

            if (!noEmitFragments)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Line = row,
                    Column = col,
                    Length = len,
                    Color = Dassie.Text.Color.Field,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Field(f)
                });
            }

            return f;
        }

        // 2. Properties
        PropertyInfo p = type.GetProperty(name/*, flags*/);
        if (p != null)
        {
            if (!noEmitFragments)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Line = row,
                    Column = col,
                    Length = len,
                    Color = Dassie.Text.Color.Property,
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

        if (!CurrentMethod.ParameterBoxIndices.ContainsKey(memberIndex))
            CurrentMethod.ParameterBoxIndices.Add(memberIndex, new());

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
                    if (argumentTypes[i] == possibleMethod.GetParameters()[i].ParameterType || possibleMethod.GetParameters()[i].ParameterType.IsAssignableFrom(argumentTypes[i]))
                    {
                        if (possibleMethod.GetParameters()[i].ParameterType == typeof(object))
                        {
                            CurrentMethod.ParameterBoxIndices[memberIndex].Add(i);
                        }

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

            for (int i = 0; i < final.GetParameters().Length; i++)
            {
                try
                {
                    if (CurrentMethod.ParameterBoxIndices[memberIndex].Contains(i)
                                && final.GetParameters()[i].ParameterType != typeof(object))
                    {
                        CurrentMethod.ParameterBoxIndices.Remove(i);
                    }
                }
                catch
                {
                    break;
                }
            }

            if (type != typeof(object) && final.DeclaringType == typeof(object))
                CurrentMethod.BoxCallingType = true;

            if (!noEmitFragments)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Line = row,
                    Column = col,
                    Length = len,
                    Color = Dassie.Text.Color.Function,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Function(final)
                });
            }

            return final;
        }

    Error:

        if (throwErrors)
        {
            EmitErrorMessage(
                row,
                col,
                len,
                DS0002_MethodNotFound,
                $"Type '{type.FullName}' has no compatible member called '{name}'.");
        }

        return null;
    }

    private static object ResolveMember(TypeBuilder tb, string name, int row, int col, int len, bool noEmitFragments = false, Type[] argumentTypes = null, BindingFlags flags = BindingFlags.Public, bool throwErrors = true)
    {
        TypeContext[] types = Context.Types.Where(c => c.Builder == tb).ToArray();

        if (types.Length == 0 && throwErrors)
        {
            EmitErrorMessage(
                row,
                col,
                len,
                DS0085_TypeInfoCouldNotBeRead,
                $"Members of '{tb.FullName}' could not be located.");

            return null;
        }

        TypeContext tc = types.First();

        // 0. Constructors
        if (name == tb.Name)
        {
            argumentTypes ??= Type.EmptyTypes;

            ConstructorInfo[] cons = tc.ConstructorContexts.Select(c => c.ConstructorBuilder)
                .Where(c => c.GetParameters().Length == argumentTypes.Length)
                .ToArray();

            if (!CurrentMethod.ParameterBoxIndices.ContainsKey(memberIndex))
                CurrentMethod.ParameterBoxIndices.Add(memberIndex, new());

            if (!cons.Any() && throwErrors)
            {
                EmitErrorMessage(
                    row, col, len,
                    DS0002_MethodNotFound,
                    $"The type '{name}' has no constructor with the specified argument types.");

                return null;
            }

            ConstructorInfo final = null;

            foreach (ConstructorInfo possibleMethod in cons)
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
                    if (argumentTypes[i] == possibleMethod.GetParameters()[i].ParameterType || possibleMethod.GetParameters()[i].ParameterType.IsAssignableFrom(argumentTypes[i]))
                    {
                        if (possibleMethod.GetParameters()[i].ParameterType == typeof(object))
                        {
                            CurrentMethod.ParameterBoxIndices[memberIndex].Add(i);
                        }

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

            for (int i = 0; i < final.GetParameters().Length; i++)
            {
                if (CurrentMethod.ParameterBoxIndices[memberIndex].Contains(i)
                    && final.GetParameters()[i].ParameterType != typeof(object))
                {
                    CurrentMethod.ParameterBoxIndices.Remove(i);
                }
            }

            if (tc.Builder != typeof(object) && final.DeclaringType == typeof(object))
                CurrentMethod.BoxCallingType = true;

            if (!noEmitFragments)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Line = row,
                    Column = col,
                    Length = len,
                    Color = Dassie.Text.Color.Function,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Constructor(final)
                });
            }

            return final;
        }

        // 1. Fields
        if (tc.Fields.Any(f => f.Name == name))
        {
            MetaFieldInfo f = tc.Fields.First(f => f.Name == name);

            if (f.Builder.FieldType.IsEnum)
            {
                if (!noEmitFragments)
                {
                    CurrentFile.Fragments.Add(new()
                    {
                        Line = row,
                        Column = col,
                        Length = len,
                        Color = Dassie.Text.Color.EnumField,
                        IsNavigationTarget = false,
                        ToolTip = TooltipGenerator.EnumField(f.Builder)
                    });
                }

                return new EnumValueInfo()
                {
                    Name = f.Name,
                    Value = f.Builder.GetRawConstantValue(),
                    EnumType = f.Builder.FieldType
                };
            }

            if (!noEmitFragments)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Line = row,
                    Column = col,
                    Length = len,
                    Color = Dassie.Text.Color.Field,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Field(f.Builder)
                });
            }

            return f.Builder;
        }

        // 2. Properties
        if (tc.Properties.Any(p => p.Name == name))
        {
            PropertyInfo p = tc.Properties.First(p => p.Name == name);

            if (!noEmitFragments)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Line = row,
                    Column = col,
                    Length = len,
                    Color = Dassie.Text.Color.Property,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Property(p)
                });
            }

            return p;
        }

        // 3. Methods
        argumentTypes ??= Type.EmptyTypes;

        MethodInfo[] methods = tc.Methods.Select(m => m.Builder)
            .Where(m => m.Name == name)
            .Where(m => m.GetParameters().Length == argumentTypes.Length)
            .ToArray();

        if (!CurrentMethod.ParameterBoxIndices.ContainsKey(memberIndex))
            CurrentMethod.ParameterBoxIndices.Add(memberIndex, new());

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
                    Type pType = possibleMethod.GetParameters()[i].ParameterType;
                    if (pType.IsByRef)
                        pType = pType.GetElementType();

                    if (argumentTypes[i] == pType || pType.IsAssignableFrom(argumentTypes[i]))
                    {
                        if (pType == typeof(object))
                        {
                            CurrentMethod.ParameterBoxIndices[memberIndex].Add(i);
                        }

                        if (possibleMethod.GetParameters()[i].ParameterType.IsByRef)
                            CurrentMethod.ByRefArguments.Add(i);
                        
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

            for (int i = 0; i < final.GetParameters().Length; i++)
            {
                try
                {
                    if (CurrentMethod.ParameterBoxIndices[memberIndex].Contains(i)
                                && final.GetParameters()[i].ParameterType != typeof(object))
                    {
                        CurrentMethod.ParameterBoxIndices.Remove(i);
                    }
                }
                catch
                {
                    break;
                }
            }

            if (tc.Builder != typeof(object) && final.DeclaringType == typeof(object))
                CurrentMethod.BoxCallingType = true;

            if (!noEmitFragments)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Line = row,
                    Column = col,
                    Length = len,
                    Color = Dassie.Text.Color.Function,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Function(final)
                });
            }

            return final;
        }

    Error:

        if (throwErrors)
        {
            EmitErrorMessage(
                row,
                col,
                len,
                DS0002_MethodNotFound,
                $"Type '{tc.Builder.FullName}' has no compatible member called '{name}'.");
        }

        return false;
    }

    public static bool TryGetType(string name, out Type type, int row, int col, int len, bool noEmitFragments = false)
    {
        type = Type.GetType(name);

        if (type == null)
        {
            if (Context.Types.Any(t => t.FilesWhereDefined.Contains(CurrentFile.Path) && t.FullName == name))
            {
                type = Context.Types.First(t => t.FilesWhereDefined.Contains(CurrentFile.Path) && t.FullName == name).Builder;
                return true;
            }

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

                if (Context.Types.Any(t => t.FullName == n))
                {
                    type = Context.Types.First(t => t.FullName == n).Builder;
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
            //EmitErrorMessage(
            //    row,
            //    col,
            //    len,
            //    DS0009_TypeNotFound,
            //    $"The name '{name}' could not be resolved.");

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