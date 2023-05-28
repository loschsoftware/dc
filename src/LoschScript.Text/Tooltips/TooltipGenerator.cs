using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace LoschScript.Text.Tooltips;

/// <summary>
/// Provides functionality for generating tooltips for objects to be used in editors.
/// </summary>
public static class TooltipGenerator
{
    /// <summary>
    /// Generates a tooltip for a type.
    /// </summary>
    /// <param name="type">The type to generate a tooltip for.</param>
    /// <param name="omitNamespace">If <see langword="true"/>, omits the namespace of the type from the tooltip.</param>
    /// <param name="doc">Optional XML documentation of the type.</param>
    /// <returns>Returns the generated tooltip.</returns>
    public static Tooltip Type(TypeInfo type, bool omitNamespace = false, Tooltip doc = null)
    {
        ObservableCollection<Word> words = new();

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

        if (doc != null)
        {
            words.Add(BuildWord(Environment.NewLine));

            foreach (Word word in doc.Words)
                words.Add(word);
        }

        return new()
        {
            Words = words
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
}