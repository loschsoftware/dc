using Dassie.Helpers;
using Dassie.Meta;
using Dassie.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static Dassie.Helpers.TypeHelpers;

namespace Dassie.Errors;

internal static class ErrorMessageHelpers
{
    private static string GenerateParamList(Type[] types)
    {
        StringBuilder sb = new();
        sb.Append('[');

        if (types.Length != 0)
        {
            foreach (Type paramType in types[..^1])
                sb.Append($"{TypeHelpers.TypeName(paramType)}, ");

            sb.Append(TypeHelpers.TypeName(types.Last()));
        }

        sb.Append(']');
        return sb.ToString();
    }

    private static string GenerateParamList(MethodBase overload)
    {
        return GenerateParamList(overload.GetParameters().Select(p => p.ParameterType).ToArray());
    }

    public static void EmitDS0002Error(int row, int col, int len, string name, Type type, IEnumerable<MethodBase> overloads, Type[] providedArgs)
    {
        StringBuilder errorMsgBuilder = new();

        if (!overloads.Any())
            errorMsgBuilder.Append($"Member '{name}' not found in type '{TypeHelpers.TypeName(type)}'.");

        else
        {
            errorMsgBuilder.AppendLine($"Wrong argument types passed to member '{name}':");
            errorMsgBuilder.Append($"    * Expected: {GenerateParamList(overloads.First())}");

            if (overloads.Count() > 1)
                errorMsgBuilder.AppendLine($" ({overloads.Count() - 1} other overload{(overloads.Count() - 1 == 1 ? "" : "s")} available)");
            else
                errorMsgBuilder.AppendLine();

            errorMsgBuilder.Append($"    * Provided: {GenerateParamList(providedArgs)}");
        }

        EmitErrorMessage(
            row,
            col,
            len,
            DS0002_MethodNotFound,
            errorMsgBuilder.ToString());
    }

    public static void EmitDS0002ErrorIfInvalid(int row, int col, int len, string name, Type type, MethodBase overload, Type[] providedArgs)
    {
        if (providedArgs == null)
            return;

        bool error = false;

        if (overload.GetParameters().Length != providedArgs.Length)
            error = true;

        else
        {
            for (int i = 0; i < overload.GetParameters().Length; i++)
            {
                try
                {
                    if (!overload.GetParameters()[i].ParameterType.IsAssignableFrom(providedArgs[i]))
                    {
                        if (providedArgs[i] == typeof(Wildcard))
                            continue;

                        error = true;
                        break;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        if (error)
            EmitDS0002Error(row, col, len, name, type, [overload], providedArgs);
    }

    private static string FormatType(Type type)
    {
        string result = type.FullName;
        if (CurrentFile.Aliases.Any(a => a.Name == result))
            result = CurrentFile.Aliases.First(a => a.Name == result).Alias;

        return result;
    }

    public static string FormatMethod(this MethodInfo method)
    {
        StringBuilder sb = new();

        sb.Append($"{method.DeclaringType.FullName}.{method.Name} ");

        if (method.IsGenericMethod)
        {
            sb.Append("[");
            foreach (Type typeArg in method.GetGenericArguments()[..^1])
                sb.Append($"{typeArg}, ");

            sb.Append(method.GetGenericArguments().Last().ToString());
            sb.Append("] ");
        }

        sb.Append('(');

        if (method.GetParameters().Length > 0)
        {
            foreach (Type type in method.GetParameters()[..^1].Select(p => p.ParameterType))
                sb.Append($"{FormatType(type)}, ");

            sb.Append(FormatType(method.GetParameters().Last().ParameterType));
        }

        sb.Append(')');
        sb.Append($": {FormatType(method.ReturnType)}");
        return sb.ToString();
    }

    public static string FormatMethod(this MockMethodInfo method)
    {
        StringBuilder sb = new();

        sb.Append($"{method.DeclaringType.FullName}.{method.Name} ");

        if (method.IsGenericMethod)
        {
            sb.Append("[");
            foreach (Type typeArg in method.GenericTypeArguments[..^1])
                sb.Append($"{typeArg}, ");

            sb.Append(method.GenericTypeArguments.Last().ToString());
            sb.Append("] ");
        }

        sb.Append('(');

        if (method.Parameters.Count > 0)
        {
            foreach (Type type in method.Parameters[..^1])
                sb.Append($"{FormatType(type)}, ");

            sb.Append(FormatType(method.Parameters.Last()));
        }

        sb.Append(')');
        sb.Append($": {FormatType(method.ReturnType)}");
        return sb.ToString();
    }

    public static void EnsureBaseTypeCompatibility(Type type, bool childTypeIsValueType, int row, int col, int len)
    {
        if (type.IsClass)
        {
            if (type.IsSealed)
            {
                EmitErrorMessage(
                    row, col, len,
                    DS0157_InheritingFromSealedType,
                    $"Cannot inherit from type '{TypeName(type)}' because it is sealed.",
                    tip: "If possible, mark the type as 'open' to allow inheritance.");
            }
            else if (childTypeIsValueType)
            {
                EmitErrorMessage(
                    row, col, len,
                    DS0146_ValueTypeInheritsFromClass,
                    $"Value types cannot inherit from reference types. Value types are only allowed to implement templates.");
            }
            else if (type == typeof(ValueType))
            {
                EmitErrorMessage(
                    row, col, len,
                    DS0145_ValueTypeInherited,
                    $"Inheriting from 'System.ValueType' is not permitted. To declare a value type, use 'val type'.");
            }


        }
        else if (type.IsValueType)
        {
            EmitErrorMessage(
                row, col, len,
                DS0147_ValueTypeAsBaseType,
                $"Inheriting from value types is not permitted. Only reference types can be inherited.");
        }
    }
}