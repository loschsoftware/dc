using System;
using System.Collections;
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
        return Dump(obj, 0);
    }

    private static string Dump(object obj, int depth, bool inline = false)
    {
        StringBuilder sb = new();

        if (!inline)
            sb.Append(new string(' ', depth * 2));

        sb.AppendLine($"{{{obj.GetType().FullName}}}");

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
            return "()";

        if (obj is SelfReference s)
            return $"[self reference: {{{s.WrappedType.FullName}}}]{(omitNL ? "" : Environment.NewLine)}";

        if (obj.GetType().IsPrimitive)
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
                sb.Append("@");

            sb.Append("[ ");
            foreach (object item in enumerable.Cast<object>().Take(10))
            {
                sb.Append(Format(item, prevDepth, true));
                sb.Append(", ");
            }

            if (enumerable.Cast<object>().Count() > 0)
                sb.Remove(sb.Length - 2, 2);

            if (enumerable.Cast<object>().Count() > 10)
                sb.Append(", ...");

            sb.Append($" ]{(omitNL ? "" : Environment.NewLine)}");
            return sb.ToString();
        }

        return Dump(obj, prevDepth + 1, true);
    }
}