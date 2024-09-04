﻿using Dassie.CLI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dassie.Errors;

internal class ErrorMessageHelpers
{
    private static string GenerateParamList(Type[] types)
    {
        StringBuilder sb = new();
        sb.Append('[');

        if (types.Length != 0)
        {
            foreach (Type paramType in types[..^1])
                sb.Append($"{TypeHelpers.Format(paramType)}, ");

            sb.Append(TypeHelpers.Format(types.Last()));
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
            errorMsgBuilder.Append($"Member '{name}' not found in type '{TypeHelpers.Format(type)}'.");

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
}