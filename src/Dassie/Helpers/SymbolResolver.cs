using Antlr4.Runtime.Tree;
using Dassie.CodeGeneration;
using Dassie.Core;
using Dassie.Errors;
using Dassie.Meta;
using Dassie.Parser;
using Dassie.Runtime;
using Dassie.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dassie.Helpers;

internal static class SymbolResolver
{
    public class EnumValueInfo
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public Type EnumType { get; set; }
    }

    public class DirectlyInitializedValueType
    {
        public Type Type { get; init; }
    }

    public static string GetTypeArgumentListSuffix(Type[] typeArgs, bool includeBacktick)
    {
        static string Name(Type t)
        {
            if (t is TypeBuilder)
                return t.FullName;

            return t.AssemblyQualifiedName;
        }

        if (typeArgs == null || typeArgs.Length == 0)
            return "";

        StringBuilder sb = new();

        if (includeBacktick)
            sb.Append($"`{typeArgs.Length}");

        sb.Append('[');

        foreach (Type t in typeArgs[0..^1])
            sb.Append($"[{Name(t)}],");

        sb.Append($"[{Name(typeArgs.Last())}]");
        sb.Append(']');
        return sb.ToString();
    }

    public static object GetSmallestTypeFromLeft(DassieParser.Full_identifierContext fullId, Type[] typeArgs, int row, int col, int len, out int firstUnusedPart, bool noEmitFragments = false)
    {
        string[] parts = fullId.Identifier().Select(f => f.GetText()).ToArray();
        firstUnusedPart = 0;

        string typeString = "";

        for (int i = 0; i < parts.Length; i++)
        {
            string regularType = string.Join(".", parts[0..(i + 1)]);
            typeString = string.Join(".", regularType + GetTypeArgumentListSuffix(typeArgs, true));
            string typeStringNoBacktick = string.Join(".", regularType + GetTypeArgumentListSuffix(typeArgs, false));

            firstUnusedPart++;

            if (typeArgs != null && typeArgs.Length > 0)
            {
                string uninitializedGenericTypeName = $"{regularType}`{typeArgs.Length}";
                if (ResolveIdentifier(uninitializedGenericTypeName, row, col, len, true) is Type uninitializedGenericType)
                    TypeHelpers.CheckGenericTypeCompatibility(uninitializedGenericType, typeArgs, row, col, len, true);

                else
                {
                    if (ResolveIdentifier(regularType, row, col, len, true) is Type genericTypeWithoutBacktick)
                        TypeHelpers.CheckGenericTypeCompatibility(genericTypeWithoutBacktick, typeArgs, row, col, len, true);
                }
            }

            if (ResolveIdentifier(typeString, row, col, len, true, typeArgs: typeArgs) is Type t)
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

            if (ResolveIdentifier(typeStringNoBacktick, row, col, len, true, typeArgs: typeArgs) is Type _t)
            {
                if (!noEmitFragments)
                {
                    CurrentFile.Fragments.Add(new()
                    {
                        Line = row,
                        Column = col,
                        Length = len,
                        Color = TooltipGenerator.ColorForType(_t.GetTypeInfo()),
                        IsNavigationTarget = false,
                        ToolTip = TooltipGenerator.Type(_t.GetTypeInfo(), false, true)
                    });
                }

                return _t;
            }
        }

        firstUnusedPart = Math.Min(1, fullId.Identifier().Length - 1);

        // First part of full_id could also be parameter, local, member of current class.
        return ResolveIdentifier(
            parts[0],
            row,
            col,
            len,
            noEmitFragments,
            typeArgs: typeArgs);
    }

    public static object ResolveIdentifier(string text, int row, int col, int len, bool noEmitFragments = false, bool throwErrors = true, Type[] typeArgs = null)
    {
        // 1. Parameters
        if (CurrentMethod.Parameters.Any(p => p.Name == text))
        {
            ParamInfo param = CurrentMethod.Parameters.First(p => p.Name == text);

            if (CurrentMethod.CaptureSymbols)
            {
                CurrentMethod.CapturedSymbols.Add(new()
                {
                    Parameter = param
                });
            }

            return param;
        }

        if (CurrentMethod.IsLocalFunction && CurrentMethod.Parent.Parameters.Any(p => p.Name == text))
        {
            ParamInfo param = CurrentMethod.Parameters.First(p => p.Name == text);
            CurrentMethod.CapturedSymbols.Add(new()
            {
                Parameter = param
            });

            return param;
        }

        // 2. Locals
        if (CurrentMethod.Locals.Any(p => p.Name == text))
        {
            LocalInfo loc = CurrentMethod.Locals.First(p => p.Name == text);

            if (loc.Scope > CurrentMethod.CurrentScope && throwErrors)
            {
                EmitErrorMessage(
                    row,
                    col,
                    len,
                    DS0062_LocalOutsideScope,
                    $"The local '{loc.Name}' is not in scope.");
            }

            if (CurrentMethod.CaptureSymbols)
            {
                CurrentMethod.CapturedSymbols.Add(new()
                {
                    Local = loc
                });
            }

            return loc;
        }

        if (CurrentMethod.IsLocalFunction && CurrentMethod.Parent.Locals.Any(p => p.Name == text))
        {
            LocalInfo loc = CurrentMethod.Parent.Locals.First(p => p.Name == text);

            // TODO: Check scoping

            CurrentMethod.CapturedSymbols.Add(new()
            {
                Local = loc
            });

            return loc;
        }

        // 3. Members of current class
        if (TypeContext.Current.Methods.Select(m => m.Builder).Where(m => m != null).Any(m => m.Name == text))
            return TypeContext.Current.Methods.Select(m => m.Builder).First(m => m != null && m.Name == text);

        if (TypeContext.Current.Builder.BaseType?.GetMethods(BindingFlags.Public | BindingFlags.NonPublic).Any(m => m.Name == text) is true)
            return TypeContext.Current.Builder.BaseType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic).First(m => m.Name == text);

        if (TypeContext.Current.Fields.Select(f => f.Builder).Where(f => f != null).Any(f => f.Name == text))
        {
            MetaFieldInfo field = TypeContext.Current.Fields.First(f => f.Name == text);

            if (field.ConstantValue == null)
                return field.Builder;

            return field;
        }

        if (TypeContext.Current.Builder.BaseType?.GetFields(BindingFlags.Public | BindingFlags.NonPublic).Any(f => f.Name == text) is true)
            return TypeContext.Current.Builder.BaseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic).First(f => f.Name == text);

        if (TypeContext.Current.Properties.Any(p => p.Name == text))
            return TypeContext.Current.Properties.First(p => p.Name == text);

        if (TypeContext.Current.Builder.BaseType?.GetProperties(BindingFlags.Public | BindingFlags.NonPublic).Any(p => p.Name == text) is true)
            return TypeContext.Current.Builder.BaseType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic).First(p => p.Name == text);

        // 4. Members of type-imported types ("global members")
        if (TryGetGlobalMember(text, out object globals, row, col, len))
            return globals;

        // 5. Other classes, including aliases
        if (TryGetType(text, out Type t, row, col, len, noEmitFragments, typeArgs))
            return t;

        // 6. Members of other classes
        return null;
    }

    private static MethodInfo GetOverload(Type type, IEnumerable<MethodInfo> methods, Type[] typeArgs, Type[] argumentTypes, int row, int col, int len, bool getDefaultOverload, bool noEmitFragments, bool throwErrors)
    {
        if (getDefaultOverload && methods.Any())
            return methods.First();

        methods = methods.Where(c => c.GetParameters().Length == argumentTypes.Length);

        if (!CurrentMethod.ParameterBoxIndices.ContainsKey(memberIndex))
            CurrentMethod.ParameterBoxIndices.Add(memberIndex, new());

        typeArgs = CurrentMethod.TypeArgumentsForNextMethodCall;

        if (methods.Any())
        {
            MethodInfo final = null;

            foreach (MethodInfo candidate in methods)
            {
                MethodInfo possibleMethod = candidate;

                if (final != null)
                    break;

                if (possibleMethod.IsGenericMethod)
                {
                    TypeHelpers.CheckGenericMethodCompatibility(possibleMethod, typeArgs, row, col, len, throwErrors);
                    possibleMethod = possibleMethod.MakeGenericMethod(typeArgs);
                }

                if (possibleMethod.GetParameters().Length == 0 && argumentTypes.Length == 0)
                {
                    final = possibleMethod;
                    break;
                }

                for (int i = 0; i < possibleMethod.GetParameters().Length; i++)
                {
                    if (possibleMethod.GetParameters()[i].ParameterType.IsAssignableFrom(argumentTypes[i])
                        || possibleMethod.GetParameters()[i].ParameterType == typeof(Wildcard))
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
                return null;

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
                    Color = Text.Color.Function,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Function(final)
                });
            }

            return final;
        }

        return null;
    }

    static int memberIndex = -1;
    public static object ResolveMember(Type type, string name, int row, int col, int len, bool noEmitFragments = false, Type[] argumentTypes = null, BindingFlags flags = BindingFlags.Public, bool throwErrors = true, bool getDefaultOverload = false, bool doNotRedirectTypeBuilder = false)
    {
        memberIndex++;

        if (type == null)
            return null;

        Type[] typeArgs = Type.EmptyTypes;

        if (type.IsByRef)
            type = type.GetElementType();

        if (type is TypeBuilder tb /*&& !doNotRedirectTypeBuilder*/)
            return ResolveMember(tb, name, row, col, len, noEmitFragments, argumentTypes, flags, throwErrors, getDefaultOverload);

        Type deconstructedGenericType = null;
        if (type.GetType().Name == "TypeBuilderInstantiation" && type.IsGenericType)
            deconstructedGenericType = TypeHelpers.DeconstructGenericType(type);

        // -1. Nested types
        try
        {
            if (type.GetNestedTypes().Any(t => t.Name == name))
                return type.GetNestedType(name);
        }
        catch { }

        // 0. Constructors
        if (name == type.Name || name == type.FullName || (!doNotRedirectTypeBuilder && name == type.AssemblyQualifiedName))
        {
            argumentTypes ??= Type.EmptyTypes;

            List<ConstructorInfo> allCons = [];
            IEnumerable<ConstructorInfo> cons = [];

            if (deconstructedGenericType != null)
            {
                ConstructorInfo[] _constructors = deconstructedGenericType.GetConstructors()
                    .Where(c => c.GetParameters().Length == argumentTypes.Length)
                    .ToArray();

                foreach (ConstructorInfo con in _constructors)
                    allCons.Add(TypeBuilder.GetConstructor(type, con));

                cons = GetAvailableMembers(deconstructedGenericType, allCons).ToList();
            }
            else
            {
                allCons = type.GetConstructors()
                    .Where(c => c.GetParameters().Length == argumentTypes.Length)
                    .ToList();

                cons = GetAvailableMembers(type, allCons).ToList();
            }

            if (!CurrentMethod.ParameterBoxIndices.ContainsKey(memberIndex))
                CurrentMethod.ParameterBoxIndices.Add(memberIndex, new());

            if (!cons.Any() && (argumentTypes == null || argumentTypes.Length == 0) && type.IsValueType)
            {
                return new DirectlyInitializedValueType()
                {
                    Type = type
                };
            }

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
                        Color = Text.Color.Function,
                        IsNavigationTarget = false,
                        ToolTip = TooltipGenerator.Constructor(final)
                    });
                }

                return final;
            }

            if (allCons.Count > 0)
            {
                EmitErrorMessage(
                    row, col, len,
                    DS0127_AccessModifiersTooRestrictive,
                    $"'{name}' cannot be called because its access modifiers are too restrictive.");
            }
        }

        // 1. Fields
        FieldInfo f;

        if (deconstructedGenericType != null)
        {
            FieldInfo _f = deconstructedGenericType.GetField(name/*, flags*/);

            if (_f == null)
                f = _f;

            else
                f = TypeBuilder.GetField(type, _f);
        }
        else
            f = type.GetField(name/*, flags*/);

        if (f != null)
        {
            if (!IsFieldAvailable(type, f))
            {
                EmitErrorMessage(
                    row, col, len,
                    DS0127_AccessModifiersTooRestrictive,
                    $"Field '{type.Name}.{f.Name}' cannot be accessed because its access modifiers are too restrictive.");
            }

            if (type.GetType().Name != "TypeBuilderInstantiation" && type.IsEnum)
            {
                if (!noEmitFragments)
                {
                    CurrentFile.Fragments.Add(new()
                    {
                        Line = row,
                        Column = col,
                        Length = len,
                        Color = Text.Color.EnumField,
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
                    Color = Text.Color.Field,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Field(f)
                });
            }

            return f;
        }

        // 2. Properties
        PropertyInfo p = null;

        try
        {
            p = type.GetProperty(name/*, flags*/);
        }
        catch { }

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
        argumentTypes = argumentTypes.Where(t => t != null).ToArray();

        IEnumerable<MethodInfo> methods = [];

        if (deconstructedGenericType != null)
        {
            MethodInfo[] _constructors = deconstructedGenericType.GetMethods()
                .Where(m => m.Name == name)
                .ToArray();

            foreach (MethodInfo meth in _constructors)
                methods = methods.Append(TypeBuilder.GetMethod(type, meth));
        }
        else
        {
            methods = type.GetMethods()
                .Where(m => m.Name == name);
        }

        MethodInfo result = GetOverload(type, methods, typeArgs, argumentTypes, row, col, len, getDefaultOverload, noEmitFragments, throwErrors);

        if (result != null)
            return result;

        if (type.IsValueType)
            CurrentMethod.IL.Emit(TypeHelpers.GetLoadIndirectOpCode(type));

        // 5. Extension methods
        IEnumerable<MethodInfo> extMethods = GetExtensions(name);
        result = GetOverload(type, extMethods, typeArgs, [type, .. argumentTypes], row, col, len, getDefaultOverload, noEmitFragments, throwErrors);

        if (result != null)
            return result;

        Error:

        object parentMember = ResolveMember(type.BaseType, name, row, col, len, noEmitFragments, argumentTypes, flags, throwErrors, getDefaultOverload);
        if (parentMember != null)
            return parentMember;

        if (throwErrors)
        {
            IEnumerable<MethodBase> overloads = type.GetMethods()
                .Where(m => m.Name == name)
                .Select(m => m.IsGenericMethod ? m.MakeGenericMethod(typeArgs) : m);

            if (name == type.Name || name == type.FullName || name == type.AssemblyQualifiedName)
                overloads = type.GetConstructors();

            ErrorMessageHelpers.EmitDS0002Error(row, col, len, name, type, overloads, argumentTypes);
        }

        return null;
    }

    private static object ResolveMember(TypeBuilder tb, string name, int row, int col, int len, bool noEmitFragments = false, Type[] argumentTypes = null, BindingFlags flags = BindingFlags.Public, bool throwErrors = true, bool getDefaultOverload = false)
    {
        //if (tb.IsCreated())
        //    return ResolveMember((Type)tb, name, row, col, len, noEmitFragments, argumentTypes, flags, throwErrors, getDefaultOverload, true);

        TypeContext[] types = Context.Types.Where(c => c.Builder == tb).ToArray();

        Type[] typeArgs = Type.EmptyTypes;

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

        // -1. Nested types
        if (types != null && types.First().Children.Any(t => t.Builder.Name == name))
            return types.First().Children.First(t => t.Builder.Name == name);

        // 0. Constructors
        if (name == tb.Name || name == tb.FullName)
        {
            if (tb.IsAbstract && tb.IsSealed)
            {
                EmitErrorMessage(
                    row, col, len,
                    DS0139_ModuleInstantiation,
                    $"Module '{name}' cannot be instantiated.");

                return null;
            }

            argumentTypes ??= Type.EmptyTypes;

            if (tc.ConstructorContexts.Count == 0)
            {
                if (argumentTypes.Length != 0)
                    goto Error;

                ConstructorInfo c = tb.GetConstructor([]);

                if (c != null)
                    return c;

                if (tb.IsValueType)
                {
                    return new DirectlyInitializedValueType()
                    {
                        Type = tb
                    };
                }
            }

            ConstructorInfo[] cons = tc.ConstructorContexts.Select(c => c.ConstructorBuilder)
                .Where(c => c.GetParameters().Length == argumentTypes.Length)
                .ToArray();

            if (!CurrentMethod.ParameterBoxIndices.ContainsKey(memberIndex))
                CurrentMethod.ParameterBoxIndices.Add(memberIndex, new());

            if (!cons.Any() && throwErrors)
                goto Error;

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
                    if (possibleMethod.GetParameters()[i].ParameterType.IsAssignableFrom(argumentTypes[i])
                        || possibleMethod.GetParameters()[i].ParameterType == typeof(Wildcard))
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
                    Color = Text.Color.Function,
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

            if (!IsFieldAvailable(tc.Builder, f.Builder))
            {
                EmitErrorMessage(
                    row, col, len,
                    DS0127_AccessModifiersTooRestrictive,
                    $"Field '{tc.Builder.Name}.{f.Builder.Name}' cannot be accessed because its access modifiers are too restrictive.");
            }

            if (f.Builder.DeclaringType.IsEnum)
            {
                if (!noEmitFragments)
                {
                    CurrentFile.Fragments.Add(new()
                    {
                        Line = row,
                        Column = col,
                        Length = len,
                        Color = Text.Color.EnumField,
                        IsNavigationTarget = false,
                        ToolTip = TooltipGenerator.EnumField(f.Builder)
                    });
                }

                return new EnumValueInfo()
                {
                    Name = f.Name,
                    Value = f.ConstantValue,
                    EnumType = f.Builder.DeclaringType
                };
            }

            if (!noEmitFragments)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Line = row,
                    Column = col,
                    Length = len,
                    Color = Text.Color.Field,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Field(f.Builder)
                });
            }

            if (f.ConstantValue != null)
                return f;

            return f;
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
                    Color = Text.Color.Property,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Property(p)
                });
            }

            return p;
        }

        // 3. Methods
        argumentTypes ??= Type.EmptyTypes;

        IEnumerable<MethodInfo> methods = tc.Methods.Select(m => m.Builder)
            .Where(m => m != null && m.Name == name);

        if (getDefaultOverload && methods.Any())
            return methods.First();

        methods = methods.Where(m => m != null && m.GetParameters().Length == argumentTypes.Length);

        if (!CurrentMethod.ParameterBoxIndices.ContainsKey(memberIndex))
            CurrentMethod.ParameterBoxIndices.Add(memberIndex, new());

        if (methods.Any())
        {
            MethodInfo final = null;

            foreach (MethodInfo candidate in methods)
            {
                MethodInfo possibleMethod = candidate;

                if (final != null)
                    break;

                if (possibleMethod.GetParameters().Length == 0 && argumentTypes.Length == 0)
                {
                    final = possibleMethod;
                    break;
                }

                typeArgs = CurrentMethod.TypeArgumentsForNextMethodCall;

                if (possibleMethod.IsGenericMethod)
                    possibleMethod = possibleMethod.MakeGenericMethod(typeArgs);

                for (int i = 0; i < possibleMethod.GetParameters().Length; i++)
                {
                    Type pType = possibleMethod.GetParameters()[i].ParameterType;
                    if (pType.IsByRef)
                        pType = pType.GetElementType();

                    if (pType.IsAssignableFrom(argumentTypes[i])
                        || argumentTypes[i] == typeof(Wildcard))
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
                    Color = Text.Color.Function,
                    IsNavigationTarget = false,
                    ToolTip = TooltipGenerator.Function(final)
                });
            }

            return final;
        }

    Error:

        object parentMember = ResolveMember(tc.Builder.BaseType, name, row, col, len, noEmitFragments, argumentTypes, flags, throwErrors, getDefaultOverload);
        if (parentMember != null)
            return parentMember;

        if (throwErrors)
        {
            IEnumerable<MethodBase> overloads = tc.Methods.Select(m => m.Builder)
                .Where(m => m != null && (m.Name == name || m.IsConstructor))
                .Select(m => m.IsGenericMethod ? m.MakeGenericMethod(typeArgs) : m);

            if (name == tb.Name || name == tb.FullName)
                overloads = tc.ConstructorContexts.Select(c => c.ConstructorBuilder);

            ErrorMessageHelpers.EmitDS0002Error(row, col, len, name, tc.Builder, overloads, argumentTypes);
        }

        return false;
    }

    public static bool TryGetType(string name, out Type type, int row, int col, int len, bool noEmitFragments = false, Type[] typeArgs = null)
    {
        type = ResolveTypeName(name, row, col, len, noEmitFragments, typeArgs, noErrors: true, doNotFillGenericTypeDefinition: typeArgs == null);
        return type != null;
    }

    //public static bool TryGetType(string name, out Type type, int row, int col, int len, bool noEmitFragments = false, Type[] typeArgs = null)
    //{
    //    if (CurrentMethod != null && CurrentMethod.TypeParameters.Any(t => t.Name == name))
    //    {
    //        type = CurrentMethod.TypeParameters.First(t => t.Name == name).Builder;
    //        return true;
    //    }

    //    if (TypeContext.Current != null && TypeContext.Current.TypeParameters.Any(t => t.Name == name))
    //    {
    //        type = TypeContext.Current.TypeParameters.First(t => t.Name == name).Builder;
    //        return true;
    //    }

    //    type = Type.GetType(name);

    //    if (type == null)
    //    {
    //        if (Context.Types.Any(t => t.FilesWhereDefined.Contains(CurrentFile.Path) && t.Builder.FullName == name))
    //        {
    //            TypeContext ctx = Context.Types.First(t => t.FilesWhereDefined.Contains(CurrentFile.Path) && t.Builder.FullName == name);
    //            type = ctx.FinishedType ?? ctx.Builder;

    //            if (type.IsGenericType && type.IsGenericTypeDefinition && typeArgs != null)
    //                type = type.MakeGenericType(typeArgs);

    //            return true;
    //        }

    //        string nonGenericName = name;
    //        if (name.Contains('`'))
    //            nonGenericName = name.Split('`')[0];

    //        if (Context.Types.Any(t => t.FilesWhereDefined.Contains(CurrentFile.Path) && t.Builder.FullName == nonGenericName))
    //        {
    //            TypeContext ctx = Context.Types.First(t => t.FilesWhereDefined.Contains(CurrentFile.Path) && t.Builder.FullName == nonGenericName);
    //            type = ctx.FinishedType ?? ctx.Builder;

    //            if (type.IsGenericType && type.IsGenericTypeDefinition && typeArgs != null)
    //                type = type.MakeGenericType(typeArgs);

    //            if (type.IsGenericType)
    //                return true;

    //            type = null;
    //        }

    //        List<Assembly> allAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
    //        allAssemblies.AddRange(Context.ReferencedAssemblies);

    //        List<Assembly> assemblies = allAssemblies.Where(_a => _a.GetType(name) != null).ToList();
    //        if (assemblies.Any())
    //        {
    //            type = assemblies.First().GetType(name);

    //            if (type.IsGenericType && type.IsGenericTypeDefinition && typeArgs != null)
    //                type = type.MakeGenericType(typeArgs);

    //            if (!noEmitFragments)
    //            {
    //                CurrentFile.Fragments.Add(new()
    //                {
    //                    Line = row,
    //                    Column = col,
    //                    Length = name.Length,
    //                    Color = TooltipGenerator.ColorForType(type.GetTypeInfo()),
    //                    ToolTip = TooltipGenerator.Type(type.GetTypeInfo(), true, true, false)
    //                });
    //            }

    //            return true;
    //        }

    //        foreach (string ns in CurrentFile.Imports.Concat(Context.GlobalImports))
    //        {
    //            string n = $"{ns}.{name}";

    //            type = Type.GetType(n);

    //            if (type != null)
    //            {
    //                if (type.IsGenericType && type.IsGenericTypeDefinition && typeArgs != null)
    //                    type = type.MakeGenericType(typeArgs);

    //                goto FoundType;
    //            }

    //            List<Assembly> _allAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
    //            _allAssemblies.AddRange(Context.ReferencedAssemblies);

    //            List<Assembly> _assemblies = _allAssemblies.Where(a => a.GetType(n) != null).ToList();
    //            if (_assemblies.Any())
    //            {
    //                type = _assemblies.First().GetType(n);

    //                if (type.IsGenericType && type.IsGenericTypeDefinition && typeArgs != null)
    //                    type = type.MakeGenericType(typeArgs);

    //                goto FoundType;
    //            }

    //            if (Context.Types.Any(t => t.FullName == n))
    //            {
    //                TypeContext ctx = Context.Types.First(t => t.FullName == n);
    //                type = ctx.FinishedType ?? ctx.Builder;

    //                if (type.IsGenericType && type.IsGenericTypeDefinition && typeArgs != null)
    //                    type = type.MakeGenericType(typeArgs);

    //                goto FoundType;
    //            }

    //            if (type != null)
    //            {
    //                if (type.IsGenericType && type.IsGenericTypeDefinition && typeArgs != null)
    //                    type = type.MakeGenericType(typeArgs);

    //                goto FoundType;
    //            }
    //        }

    //        foreach (string originalName in CurrentFile.Aliases.Concat(Context.GlobalAliases).Where(a => a.Alias == name).Select(a => a.Name))
    //        {
    //            type = Type.GetType(originalName);

    //            if (type != null)
    //            {
    //                if (type.IsGenericType && type.IsGenericTypeDefinition && typeArgs != null)
    //                    type = type.MakeGenericType(typeArgs);

    //                goto FoundType;
    //            }
    //        }
    //    }

    //FoundType:

    //    if (type == null)
    //    {
    //        //EmitErrorMessage(
    //        //    row,
    //        //    col,
    //        //    len,
    //        //    DS0009_TypeNotFound,
    //        //    $"The name '{name}' could not be resolved.");

    //        return false;
    //    }
    //    else
    //    {
    //        //if (!noEmitFragments)
    //        //{
    //        //    CurrentFile.Fragments.Add(new()
    //        //    {
    //        //        Line = row,
    //        //        Column = col,
    //        //        Length = name.Length,
    //        //        Color = TooltipGenerator.ColorForType(type.GetTypeInfo()),
    //        //        ToolTip = TooltipGenerator.Type(type.GetTypeInfo(), true, true, false)
    //        //    });
    //        //}

    //        return true;
    //    }
    //}

    private static bool TryGetGlobalMember(string name, out object members, int row, int col, int len)
    {
        foreach (string type in CurrentFile.ImportedTypes.Concat(Context.GlobalTypeImports))
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

    public static (Type Type, MethodInfo[] Methods) ResolveGlobalMethod(string name, int row, int col, int len)
    {
        foreach (string type in CurrentFile.ImportedTypes)
        {
            Type t = ResolveTypeName(type, row, col, len, true);

            if (t.GetMethods().Where(m => m.Name == name).Any())
                return (t, t.GetMethods().Where(m => m.Name == name).ToArray());
        }

        return (null, Array.Empty<MethodInfo>());
    }

    public static Type ResolveAttributeTypeName(DassieParser.Type_nameContext name, bool noEmitFragments = false)
    {
        List<Type> typeArgs = [];

        if (name.type_arg_list() != null)
        {
            foreach (DassieParser.Type_nameContext typeArg in name.type_arg_list().type_name())
                typeArgs.Add(ResolveTypeName(typeArg));
        }

        return ResolveAttributeTypeName(
            name.GetText(),
            typeArgs.Count == 0 ? null : typeArgs.ToArray(),
            name.Start.Line,
            name.Start.Column,
            name.GetText().Length,
            noEmitFragments);
    }

    public static Type ResolveAttributeTypeName(string name, Type[] typeParams, int row, int col, int len, bool noEmitFragments = false)
    {
        Type t;
        if ((t = ResolveTypeName(name, row, col, len, noEmitFragments: noEmitFragments, noErrors: true, typeParams: typeParams)) == null)
            t = ResolveTypeName($"{name}Attribute", row, col, len, noEmitFragments: noEmitFragments, noErrors: true);

        if (t != null && !t.IsAssignableTo(typeof(Attribute)))
        {
            EmitErrorMessage(
                row, col, len,
                DS0177_InvalidAttributeType,
                $"The type '{t}' cannot be used as an attribute.");
        }

        if (t != null)
            return t;

        ResolveTypeName(name, row, col, len, noEmitFragments: noEmitFragments, typeParams: typeParams); // To display error
        return t;
    }

    public static Type ResolveTypeName(DassieParser.Type_nameContext name, bool noEmitFragments = false, bool noEmitDS0149 = false, bool noErrors = false)
    {
        Type t = ResolveTypeNameInternal(name, noEmitFragments, noEmitDS0149, noErrors);

        if (t == null)
            return t;

        if (t is TypeBuilder)
        {
            if (!Context.Types.Where(tc => tc.FullName == t.FullName).Any())
                return t;

            if (Context.Types.First(tc => tc.FullName == t.FullName).IsAlias)
                return Context.Types.First(tc => tc.FullName == t.FullName).AliasedType;

            return t;
        }

        try
        {
            if (t.GetCustomAttribute<AliasAttribute>() is AliasAttribute alias)
                return alias.AliasedType;
        }
        catch (NotSupportedException) { }

        return t;
    }

    private static Type ResolveTypeNameInternal(DassieParser.Type_nameContext name, bool noEmitFragments = false, bool noEmitDS0149 = false, bool noErrors = false)
    {
        if (name.Func() != null)
        {
            // Function pointer type
            // e.g. func*[int, int, int] -> X (a: int, b: int): int

            Type[] typeArgs = name.type_arg_list().type_name().Select(t => ResolveTypeName(t, noEmitFragments)).ToArray();

            Type[] parameterTypes = typeArgs[..^1];
            Type returnType = typeArgs[^1];

            //EmitErrorMessage(
            //    name.Start.Line,
            //    name.Start.Column,
            //    name.GetText().Length,
            //    DS0159_FrameworkLimitation,
            //    $"Function pointer types are currently unsupported.");

            if (returnType == typeof(void))
                return FunctionPointerHelpers.MakeGenericManagedCallVoidFunctionPointerType(parameterTypes);

            return FunctionPointerHelpers.MakeGenericManagedCallFunctionPointerType((returnType, parameterTypes));
        }

        if (name.Double_Ampersand() != null)
        {
            Type t = ResolveTypeName(name.type_name(), noEmitFragments, noEmitDS0149: true);

            if (!noEmitDS0149 && !noErrors)
            {
                EmitErrorMessage(
                    name.Start.Line,
                    name.Start.Column,
                    name.GetText().Length,
                    DS0149_NestedByRefType,
                    $"Invalid type '{name.GetText()}': Nested references are not permitted.");
            }

            if (t.IsByRef)
                return t;

            return t.MakeByRefType();
        }

        if (name.Ampersand() != null)
        {
            Type t = ResolveTypeName(name.type_name(), noEmitFragments, noEmitDS0149: true);

            if (t.IsByRef && !noErrors)
            {
                EmitErrorMessage(
                    name.Start.Line,
                    name.Start.Column,
                    name.GetText().Length,
                    DS0149_NestedByRefType,
                    $"Invalid type '{name.GetText()}': Nested references are not permitted.");

                return t;
            }

            return t.MakeByRefType();
        }

        int arrayDims = 0;

        if (name.array_type_specifier() != null)
        {
            arrayDims = (name.array_type_specifier().Comma() ?? Array.Empty<ITerminalNode>()).Length + 1;
            arrayDims += (name.array_type_specifier().Double_Comma() ?? Array.Empty<ITerminalNode>()).Length * 2;
        }

        if (arrayDims > 32 && !noErrors)
        {
            EmitErrorMessage(
                name.Start.Line,
                name.Start.Column,
                name.GetText().Length,
                DS0079_ArrayTooManyDimensions,
                $"An array cannot have more than 32 dimensions.");
        }

        if (name.identifier_atom() != null)
        {
            if (name.identifier_atom().Identifier() != null)
                return ResolveTypeName(name.identifier_atom().Identifier().GetText(), name.Start.Line, name.Start.Column, name.identifier_atom().Identifier().GetText().Length, noEmitFragments, arrayDimensions: arrayDims, noErrors: noErrors);

            return ResolveTypeName(name.identifier_atom().full_identifier().GetText(), name.Start.Line, name.Start.Column, name.identifier_atom().full_identifier().GetText().Length, noEmitFragments, arrayDimensions: arrayDims, noErrors: noErrors);
        }

        // Tuple type
        // e.g. (int, string)
        if (name.Comma() != null && name.Comma().Length > 0)
        {
            var partTypes = TypeHelpers.GetTupleItems(name, noEmitFragments, noEmitDS0149);
            if (partTypes.All(p => string.IsNullOrEmpty(p.Name)))
                return TypeHelpers.GetValueTupleType(partTypes.Select(t => t.Type).ToArray());

            return null;
        }

        // Inline union type
        // e.g. (int | string)
        if (name.Bar() != null && name.Bar().Length > 0)
        {
            var partTypes = TypeHelpers.GetUnionItems(name, noEmitFragments, noEmitDS0149);
            return UnionTypeCodeGeneration.GenerateInlineUnionType(partTypes);
        }

        if (name.type_arg_list() != null)
        {
            Type[] typeParams = name.type_arg_list().type_name().Select(t => ResolveTypeName(t, noEmitFragments)).ToArray();

            DassieParser.Type_nameContext childName = (DassieParser.Type_nameContext)name.children[0];
            if (childName.identifier_atom() != null && childName.identifier_atom().Identifier() != null)
            {
                Type result = ResolveTypeName(childName.identifier_atom().Identifier().GetText(), childName.Start.Line, childName.Start.Column, childName.identifier_atom().Identifier().GetText().Length, noEmitFragments, typeParams, arrayDimensions: arrayDims, noErrors: noErrors);

                TypeHelpers.CheckGenericTypeCompatibility(result, typeParams, childName.Start.Line, childName.Start.Column, childName.GetText().Length, !noErrors);

                if (typeParams != null && result.IsGenericTypeDefinition)
                    result = result.MakeGenericType(typeParams);

                return result;
            }
        }

        if (name.type_name() != null && name.type_name() != null)
        {
            Type child = ResolveTypeName(name.type_name(), noEmitFragments, noErrors: noErrors);
            return ResolveTypeName(child.AssemblyQualifiedName, name.Start.Line, name.Start.Column, name.GetText().Length, noEmitFragments, arrayDimensions: arrayDims, noErrors: noErrors);
        }

        // TODO: Implement other kinds of types
        return null;
    }

    public static Type ResolveTypeName(string name, int row = 0, int col = 0, int len = 0, bool noEmitFragments = false, Type[] typeParams = null, int arrayDimensions = 0, bool noErrors = false, bool disableBacktickGenericResolve = false, bool doNotFillGenericTypeDefinition = false)
    {
        Type t = ResolveTypeNameInternal(name, row, col, len, noEmitFragments, typeParams, arrayDimensions, noErrors, disableBacktickGenericResolve, doNotFillGenericTypeDefinition);

        if (t == null)
            return t;

        if (t is TypeBuilder)
        {
            if (!Context.Types.Where(t => t.FullName == name).Any())
                return t;

            if (Context.Types.First(t => t.FullName == name).IsAlias)
                return Context.Types.First(t => t.FullName == name).AliasedType;

            return t;
        }
        
        try
        {
            if (t.GetCustomAttribute<AliasAttribute>() is AliasAttribute alias)
                return alias.AliasedType;
        }
        catch (NotSupportedException) { }

        return t;
    }

    private static Type ResolveTypeNameInternal(string name, int row = 0, int col = 0, int len = 0, bool noEmitFragments = false, Type[] typeParams = null, int arrayDimensions = 0, bool noErrors = false, bool disableBacktickGenericResolve = false, bool doNotFillGenericTypeDefinition = false)
    {
        if (TypeContext.Current != null && TypeContext.Current.FullName == name)
            return TypeContext.Current.Builder;

        if (CurrentMethod != null && CurrentMethod.TypeParameters.Any(t => t.Name == name))
            return CurrentMethod.TypeParameters.First(t => t.Name == name).Builder;

        if (TypeContext.Current != null && TypeContext.Current.TypeParameters.Any(t => t.Name == name))
            return TypeContext.Current.TypeParameters.First(t => t.Name == name).Builder;

        if (typeParams != null && typeParams.Length > 0 && !disableBacktickGenericResolve)
        {
            // Convention used by Microsoft compilers to allow "overloaded" generics
            // e.g. A[T], A[T1, T2], A[T1, T2, T3]
            // otherwise only one set of generic parameters would be possible

            string backtickName = $"{name}`{typeParams.Length}";
            Type t = ResolveTypeName(backtickName, row, col, len, noEmitFragments, typeParams, arrayDimensions, noErrors: true, disableBacktickGenericResolve: true);
            if (t != null)
                return t;
        }

        Type type = Type.GetType(name);

        if (type == null)
        {
            if (Context.Types.Any(t => t.FilesWhereDefined.Contains(CurrentFile.Path) && t.FullName == name))
            {
                TypeContext ctx = Context.Types.First(t => t.FilesWhereDefined.Contains(CurrentFile.Path) && t.FullName == name);
                return ctx.FinishedType ?? ctx.Builder;
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

                if (arrayDimensions > 0)
                    type = type.MakeArrayType(arrayDimensions);

                return type;
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
                    TypeContext ctx = Context.Types.First(t => t.FullName == n);
                    type = ctx.FinishedType ?? ctx.Builder;
                    goto FoundType;
                }

                if (type != null)
                    goto FoundType;
            }

            foreach (string originalName in CurrentFile.Aliases.Concat(Context.GlobalAliases).Where(a => a.Alias == name).Select(a => a.Name))
            {
                type = Type.GetType(originalName);

                if (type != null)
                    goto FoundType;
            }
        }

    FoundType:

        if (type == null && noErrors)
            return type;

        if (type == null)
        {
            EmitErrorMessage(
                row,
                col,
                len,
                DS0009_TypeNotFound,
                $"The name '{name}' could not be resolved.");
        }
        else
        {
            if (!doNotFillGenericTypeDefinition)
            {
                TypeHelpers.CheckGenericTypeCompatibility(type, typeParams, row, col, len, true);

                if (typeParams != null && type.IsGenericTypeDefinition)
                    type = type.MakeGenericType(typeParams);
            }

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
        }

        if (arrayDimensions > 1)
            type = type.MakeArrayType(arrayDimensions);

        else if (arrayDimensions == 1)
            type = type.MakeArrayType();

        return type;
    }

    public static (int Type, int Index) GetLocalOrParameterIndex(string name)
    {
        if (CurrentMethod.Locals.Any(l => l.Name == name))
            return (0, CurrentMethod.Locals.First(l => l.Name == name).Index);

        else if (CurrentMethod.Parameters.Any(p => p.Name == name))
            return (1, CurrentMethod.Parameters.First(p => p.Name == name).Index);

        return (-1, -1);
    }

    public static SymbolInfo GetSymbol(string name)
    {
        if (CurrentMethod.Locals.Any(l => l.Name == name))
        {
            LocalInfo l = CurrentMethod.Locals.First(l => l.Name == name);
            return new()
            {
                SymbolType = SymbolInfo.SymType.Local,
                Local = l
            };
        }

        else if (CurrentMethod.Parameters.Any(p => p.Name == name))
        {
            ParamInfo p = CurrentMethod.Parameters.First(p => p.Name == name);
            return new()
            {
                SymbolType = SymbolInfo.SymType.Parameter,
                Parameter = p
            };
        }

        else if (TypeContext.Current.Fields.Any(f => f.Name == name))
        {
            MetaFieldInfo f = TypeContext.Current.Fields.First(f => f.Name == name);
            return new()
            {
                SymbolType = SymbolInfo.SymType.Field,
                Field = f
            };
        }

        else if (TypeContext.Current.Properties.Any(p => p.Name == name))
        {
            return new()
            {
                SymbolType = SymbolInfo.SymType.Property,
                Property = TypeContext.Current.Properties.First(p => p.Name == name)
            };
        }

        return null;
    }

    private static IEnumerable<MethodInfo> GetAvailableMembers(Type type, IEnumerable<MethodInfo> members)
    {
        if (type == TypeContext.Current.Builder)
            return members;

        if (TypeContext.Current.Builder.BaseType == type)
        {
            IEnumerable<MethodInfo> mems = members.Where(m => m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly);

            if (type.Assembly == Context.Assembly)
                mems = members.Where(m => m.IsFamilyAndAssembly).Union(mems);

            return mems;
        }

        if (type.Assembly == Context.Assembly)
            return members.Where(m => m.IsPublic || m.IsAssembly || m.IsFamilyOrAssembly);

        return members.Where(m => m.IsPublic);
    }

    private static IEnumerable<ConstructorInfo> GetAvailableMembers(Type type, IEnumerable<ConstructorInfo> members)
    {
        if (type == TypeContext.Current.Builder)
            return members;

        if (TypeContext.Current.Builder.BaseType == type)
        {
            IEnumerable<ConstructorInfo> mems = members.Where(m => m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly);

            if (type.Assembly == Context.Assembly)
                mems = members.Where(m => m.IsFamilyAndAssembly).Union(mems);

            return mems;
        }

        if (type.Assembly == Context.Assembly)
            return members.Where(m => m.IsPublic || m.IsAssembly || m.IsFamilyOrAssembly);

        return members.Where(m => m.IsPublic);
    }

    private static bool IsFieldAvailable(Type callingType, FieldInfo field)
    {
        if (field.IsPublic)
            return true;

        if (TypeContext.Current.Builder.BaseType == callingType)
        {
            if (callingType.Assembly == Context.Assembly)
                return field.IsFamily || field.IsFamilyOrAssembly || field.IsFamilyAndAssembly;

            return field.IsFamily || field.IsFamilyOrAssembly;
        }

        if (callingType.Assembly == Context.Assembly)
            return field.IsAssembly;

        return callingType == TypeContext.Current.Builder;
    }

    public static IEnumerable<MethodInfo> GetExtensions(string name)
    {
        IEnumerable<MethodInfo> extensionMethodsTypesFromCurrentAssembly = [];

        if (Context.Attributes.Any(a => a.Constructor == typeof(ExtensionAttribute).GetConstructor([])))
        {
            extensionMethodsTypesFromCurrentAssembly = Context.Types.Where(t => t.Attributes.Any(a => a.Constructor == typeof(ExtensionAttribute).GetConstructor([])))
                .SelectMany(t => t.Methods)
                .Where(m => m.Attributes.Any(a => a.Constructor == typeof(ExtensionAttribute).GetConstructor([])))
                .Select(m => m.Builder)
                .Where(m => m.Name == name);
        }

        IEnumerable<MethodInfo> extensionMethodsFromReferencedAssemblies = Context.ReferencedAssemblies.Append(typeof(stdout).Assembly)
            .Where(a => a.GetCustomAttribute<ExtensionAttribute>() != null)
            .SelectMany(a =>
            {
                Type[] types = [];

                try
                {
                    types = a.GetTypes();
                }
                catch (ReflectionTypeLoadException) { }

                return types;

            })
            .Where(t => t.GetCustomAttribute<ExtensionAttribute>() != null)
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttribute<ExtensionAttribute>() != null)
            .Where(m => m.Name == name);

        return extensionMethodsTypesFromCurrentAssembly.Concat(extensionMethodsFromReferencedAssemblies);
    }

    public static MethodInfo[] ResolveCustomOperatorOverloads(string methodName)
    {
        static IEnumerable<MethodInfo> GetOperatorsOfType(Type type)
        {
            if (type is TypeBuilder tb)
            {
                TypeContext tc = Context.Types.First(t => t.Builder == tb);
                return tc.Methods.Where(mc => mc.IsCustomOperator && mc.Builder.IsPublic && mc.Builder.IsStatic)
                    .Select(m => m.Builder);
            }

            return type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.GetCustomAttribute<OperatorAttribute>() != null);
        }

        IEnumerable<Type> allTypes = Context.Types.Select(t => t.Builder).Cast<Type>()
            .Concat(Context.ReferencedAssemblies.Append(typeof(stdout).Assembly)
                .SelectMany(a =>
                {
                    Type[] types = [];

                    try
                    {
                        types = a.GetTypes();
                    }
                    catch (ReflectionTypeLoadException) { }

                    return types;

                }));
        /*.Where(t => t.IsAbstract && t.IsSealed && t.GetCustomAttribute<ContainsCustomOperatorsAttribute>() != null);*/

        IEnumerable<(string Namespace, IEnumerable<MethodInfo> Operators)> ops = allTypes
            .Select(a => (
                a.Namespace,
                Operators: GetOperatorsOfType(a)));

        IEnumerable<MethodInfo> availableOperators = ops
            .Where(o => string.IsNullOrEmpty(o.Namespace)
                || CurrentFile.Imports.Contains(o.Namespace))
            .SelectMany(m => m.Operators);

        if (availableOperators.Any(o => o.Name == methodName))
            return availableOperators.Where(o => o.Name == methodName).ToArray();

        return null;
    }
}