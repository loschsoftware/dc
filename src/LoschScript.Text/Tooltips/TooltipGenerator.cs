using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace LoschScript.Text.Tooltips;

/// <summary>
/// Provides functionality for generating tooltips for objects to be used in editors.
/// </summary>
public static class TooltipGenerator
{
    /// <summary>
    /// Generates a tooltip for a local value.
    /// </summary>
    /// <param name="name">The name of the local.</param>
    /// <param name="mutable">Wheter the local is mutable.</param>
    /// <param name="local">The local to generate a tooltip for.</param>
    /// <returns>The generated tooltip.</returns>
    public static Tooltip Local(string name, bool mutable, LocalVariableInfo local)
    {
        ObservableCollection<Word> words = new()
        {
            BuildWord(mutable ? "var " : "val ", Color.Word),
            BuildWord(name, Color.LocalValue),
            BuildWord(": ")
        };

        foreach (Word word in Type(local.LocalType.GetTypeInfo(), false, true, true).Words)
            words.Add(word);

        return new()
        {
            Words = words,
            IconResourceName = "LocalVariable"
        };
    }

    /// <summary>
    /// Generates a tooltip for a property.
    /// </summary>
    /// <param name="property">The property to generate a tooltip for.</param>
    /// <returns>The generated tooltip.</returns>
    public static Tooltip Property(PropertyInfo property)
    {
        ObservableCollection<Word> words = new()
        {
            BuildWord(property.Name, Color.Property),
            BuildWord(": ")
        };

        foreach (Word word in Type(property.PropertyType.GetTypeInfo(), false, true, true).Words)
            words.Add(word);

        return new()
        {
            Words = words,
            IconResourceName = "Property"
        };
    }

    /// <summary>
    /// Generates a tooltip for a field.
    /// </summary>
    /// <param name="field">The field to generate a tooltip for.</param>
    /// <returns>The generated tooltip.</returns>
    public static Tooltip Field(FieldInfo field)
    {
        ObservableCollection<Word> words = new()
        {
            BuildWord(field.Name, Color.Field),
            BuildWord(": ")
        };

        foreach (Word word in Type(field.FieldType.GetTypeInfo(), false, true, true).Words)
            words.Add(word);

        return new()
        {
            Words = words,
            IconResourceName = "FieldPublic"
        };
    }

    /// <summary>
    /// Generates a tooltip for a function.
    /// </summary>
    /// <param name="method">The MethodInfo representing the function.</param>
    /// <returns>The generated tooltip.</returns>
    public static Tooltip Function(MethodInfo method)
    {
        ObservableCollection<Word> words = new()
        {
            BuildWord(method.Name, Color.Function)
        };

        if (method.GetParameters().Length > 0)
            words.Add(BuildWord("("));

        foreach (ParameterInfo param in method.GetParameters()[..^1])
        {
            words.Add(BuildWord(param.Name, Color.LocalValue));
            words.Add(BuildWord(": "));

            foreach (Word word in Type(param.ParameterType.GetTypeInfo(), false, true, true).Words)
                words.Add(word);

            words.Add(BuildWord(", "));
        }

        words.Add(BuildWord(method.GetParameters()[0].Name, Color.LocalValue));
        words.Add(BuildWord(": "));

        foreach (Word word in Type(method.GetParameters()[0].ParameterType.GetTypeInfo(), false, true, true).Words)
            words.Add(word);

        if (method.GetParameters().Length > 0)
            words.Add(BuildWord(")"));

        words.Add(BuildWord(": "));
        foreach (Word word in Type(method.ReturnType.GetTypeInfo(), false, true, true).Words)
            words.Add(word);

        return new()
        {
            Words = words,
            IconResourceName = "Method"
        };
    }

    /// <summary>
    /// Generates a tooltip for a type.
    /// </summary>
    /// <param name="type">The type to generate a tooltip for.</param>
    /// <param name="showBaseType">Wheter to include the base type in the tooltip.</param>
    /// <param name="omitNamespace">If <see langword="true"/>, omits the namespace of the type from the tooltip.</param>
    /// <param name="doc">Optional XML documentation of the type.</param>
    /// <returns>Returns the generated tooltip.</returns>
    public static Tooltip Type(TypeInfo type, bool showBaseType, bool omitNamespace = false, bool noModifiers = false, Tooltip doc = null)
    {
        ObservableCollection<Word> words = new();

        if (!noModifiers)
        {
            if (type.IsInterface)
                words.Add(BuildWord("template ", Color.Word));
            else if (type.IsSealed && type.IsAbstract)
                words.Add(BuildWord("module ", Color.Word));
            else
            {
                if (type.IsSealed)
                    words.Add(BuildWord("sealed ", Color.Word));

                if (type.IsAbstract)
                    words.Add(BuildWord("abstract ", Color.Word));

                words.Add(BuildWord(type.IsValueType ? "val type " : "ref type ", Color.Word));
            }
        }

        words.Add(BuildWord(omitNamespace ? "" : (type.Namespace + ".")));
        words.Add(BuildWord(type.Name.Split('`')[0], ColorForType(type)));

        if (type.GenericTypeArguments.Length > 0)
        {
            words.Add(BuildWord("["));

            int i = 0;
            for (; i < type.GenericTypeArguments.Length - 1; ++i, words.Add(BuildWord($"T{i}, ", Color.TypeParameter))) { }

            words.Add(BuildWord($"T{i + 1}", Color.TypeParameter));

            words.Add(BuildWord("]"));
        }

        if (showBaseType && type.BaseType != null)
        {
            words.Add(BuildWord(": ", Color.Default));

            foreach (Word word in Type(type.BaseType.GetTypeInfo(), false, omitNamespace, true).Words)
                words.Add(word);

            if (type.ImplementedInterfaces.Count() == 1)
                words.Add(BuildWord(", "));

            if (type.ImplementedInterfaces.Count() > 0)
            {
                foreach (Type t in type.ImplementedInterfaces.ToArray()[..^1])
                {
                    words.Add(BuildWord(", "));

                    foreach (Word word in Type(t.GetTypeInfo(), false, omitNamespace, true).Words)
                        words.Add(word);
                }
            }

            if (type.ImplementedInterfaces.Count() > 1)
                words.Add(BuildWord(", "));

            if (type.ImplementedInterfaces.Count() > 0)
            {
                foreach (Word word in Type(type.ImplementedInterfaces.Last().GetTypeInfo(), false, omitNamespace, true).Words)
                    words.Add(word);
            }
        }

        if (doc != null)
        {
            words.Add(BuildWord(Environment.NewLine));

            foreach (Word word in doc.Words)
                words.Add(word);
        }

        return new()
        {
            Words = words,
            IconResourceName = ResourceNameForType(type)
        };
    }

    private static Word BuildWord(string text, Color color = Color.Default) => new()
    {
        Fragment = new() { Color = color },
        Text = $"{text}"
    };

    /// <summary>
    /// Returns the appropriate <see cref="Color"/> for the specified type.
    /// </summary>
    /// <param name="type">The type to get a color for.</param>
    /// <returns>The appropriate color.</returns>
    public static Color ColorForType(TypeInfo type)
    {
        if (type.IsInterface)
            return Color.TemplateType;

        if (type.IsAbstract && type.IsSealed) // That's how static types are represented in the CLR...
            return Color.Module;

        if (type.IsValueType)
            return Color.ValueType;

        return Color.ReferenceType;
    }

    private static string ResourceNameForType(TypeInfo type)
    {
        string part1 = ColorForType(type) switch
        {
            Color.TemplateType => "Interface",
            Color.Module => "Module",
            Color.ValueType => "Struct",
            _ => "Class"
        };

        string part2 = "Public";

        if (type.IsNestedFamORAssem)
            part2 = "Internal";

        if (type.IsNestedPrivate)
            part2 = "Private";

        return $"{part1}{part2}";
    }
}