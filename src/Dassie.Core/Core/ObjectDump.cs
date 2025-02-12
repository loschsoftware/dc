using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dassie.Core;

/// <summary>
/// Allows dumping the contents of an object to a string.
/// </summary>
public static class ObjectDump
{
    private struct SelfReference
    {
        public SelfReference(Type wrappedType) => WrappedType = wrappedType;

        public Type WrappedType { get; set; }
    }

    /// <summary>
    /// Dumps the properties of an object to a string.
    /// </summary>
    /// <param name="obj">The object to dump.</param>
    /// <returns>The string representation of the object.</returns>
    public static string Dump(this object obj)
    {
        if (obj == null)
            return "()\r\n";

        return Dump(obj, 0);
    }

    private static string Dump(object obj, int depth, bool inline = false)
    {
        StringBuilder sb = new();

        if (!inline)
            sb.Append(new string(' ', depth * 2));

        sb.AppendLine($"{{{GetTypeName(obj.GetType())}}}");

        if (obj is IEnumerable or string || obj.GetType().IsPrimitive || obj.GetType() == typeof(decimal) || obj.GetType().IsArray || obj.GetType().FullName.StartsWith("System.ValueTuple") || obj.GetType().IsEnum)
            return Format(obj, depth);

        foreach (MemberInfo member in obj.GetType().GetMembers())
        {
            object val;

            try
            {
                if (member is PropertyInfo prop)
                {
                    if (prop.PropertyType == obj.GetType())
                        val = new SelfReference(prop.PropertyType);
                    else
                        val = prop.GetValue(obj);
                }

                else if (member is FieldInfo field && field.IsPublic && !field.IsStatic)
                {
                    if (field.FieldType == obj.GetType())
                        val = new SelfReference(field.FieldType);
                    else
                        val = field.GetValue(obj);
                }

                else
                    continue;
            }
            catch
            {
                sb.Append(new string(' ', (depth + 1) * 2));
                sb.Append($"{member.Name}: [ERROR reading value]{Environment.NewLine}");

                continue;
            }

            sb.Append(new string(' ', (depth + 1) * 2));
            sb.Append($"{member.Name}: {Format(val, depth)}");
        }

        return sb.ToString();
    }

    private static string Format(object obj, int prevDepth, bool omitNL = false)
    {
        if (obj == null)
            return $"(){(omitNL ? "" : Environment.NewLine)}";

        if (obj is SelfReference s)
            return $"[self reference: {{{s.WrappedType.FullName}}}]{(omitNL ? "" : Environment.NewLine)}";

        if (obj is char)
            return $"'{obj.ToString()
                .Replace("^", "^^")
                .Replace("'", "^'")
                .Replace("\r", "^r")
                .Replace("\n", "^n")
                .Replace("\v", "^v")
                .Replace("\t", "^t")
                .Replace("\b", "^b")}'{(omitNL ? "" : Environment.NewLine)}";

        if (obj.GetType().IsPrimitive || obj.GetType() == typeof(decimal))
            return $"{obj}{(omitNL ? "" : Environment.NewLine)}";

        if (obj is string)
            return $"\"{obj.ToString()
                .Replace("^", "^^")
                .Replace("\r", "^r")
                .Replace("\n", "^n")
                .Replace("\v", "^v")
                .Replace("\t", "^t")
                .Replace("\b", "^b")}\"{(omitNL ? "" : Environment.NewLine)}";

        if (obj is IEnumerable || obj.GetType().IsArray)
        {
            IEnumerable enumerable = obj as IEnumerable;

            StringBuilder sb = new();

            if (obj.GetType().IsArray)
                sb.Append('@');

            sb.Append('[');
            foreach (object item in enumerable.Cast<object>().Take(10))
            {
                sb.Append(Format(item, prevDepth, true));
                sb.Append(", ");
            }

            if (enumerable.Cast<object>().Any())
                sb.Remove(sb.Length - 2, 2);

            if (enumerable.Cast<object>().Count() > 10)
                sb.Append(", ...");

            sb.Append($"]{(omitNL ? "" : Environment.NewLine)}");
            return sb.ToString();
        }

        if (obj.GetType().FullName.StartsWith("System.ValueTuple"))
        {
            StringBuilder sb = new();
            sb.Append('(');

            FieldInfo[] fields = obj.GetType().GetFields();

            foreach (FieldInfo field in fields.Take(10))
            {
                sb.Append(Format(field.GetValue(obj), prevDepth, true));
                sb.Append(", ");
            }

            if (fields.Length > 0)
                sb.Remove(sb.Length - 2, 2);

            if (fields.Length > 10)
                sb.Append(", ...");

            sb.Append($"){(omitNL ? "" : Environment.NewLine)}");
            return sb.ToString();
        }

        if (obj.GetType().IsEnum)
            return $"{obj.GetType()}.{obj}{(omitNL ? "" : Environment.NewLine)}";

        return Dump(obj, prevDepth + 1, true);
    }

    private static string GetTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.FullName;

        StringBuilder typeArgsBuilder = new();
        typeArgsBuilder.Append('[');

        foreach (Type typeArg in type.GetGenericArguments())
            typeArgsBuilder.Append($"{GetTypeName(typeArg)}, ");

        typeArgsBuilder.Remove(typeArgsBuilder.Length - 2, 2);
        typeArgsBuilder.Append(']');
        return $"{type.FullName.Split(type.FullName.Contains('`') ? '`' : '[')[0]}{typeArgsBuilder}";
    }
}