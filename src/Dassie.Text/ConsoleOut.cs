using Dassie.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DColor = System.Drawing.Color;

namespace Dassie.Text;

/// <summary>
/// Provides helper methods for writing colored fragments or tooltips to the console.
/// </summary>
public static class ConsoleOut
{
    private static void SetColor(DColor color)
    {
        Console.Write($"\x1b[38;2;{color.R};{color.G};{color.B}m");
    }

    private static DColor GetColor(Color fragmentColor)
    {
        return fragmentColor switch
        {
            Color.Function => ColorTranslator.FromHtml("#DCDCAA"),
            Color.ReferenceType => ColorTranslator.FromHtml("#4EC9B0"),
            Color.ValueType => ColorTranslator.FromHtml("#86C691"),
            Color.TemplateType => ColorTranslator.FromHtml("#B8D7A3"),
            Color.LocalValue => ColorTranslator.FromHtml("#9CDCFE"),
            Color.LocalVariable => ColorTranslator.FromHtml("#9CDCFE"),
            Color.Module => ColorTranslator.FromHtml("#C491CA"),
            Color.Word => ColorTranslator.FromHtml("#569CD6"),
            Color.StringEscapeSequence => ColorTranslator.FromHtml("#FFD68F"),
            Color.Error => ColorTranslator.FromHtml("#EC5965"),
            Color.Warning => ColorTranslator.FromHtml("#FFE5A3"),
            Color.Information => ColorTranslator.FromHtml("#6CB5FF"),
            Color.ExpressionString => ColorTranslator.FromHtml("#EEF183"),
            Color.IntrinsicFunction => ColorTranslator.FromHtml("#90C689"),
            _ => ColorTranslator.FromHtml("#FFFFFF"),
        };
    }

    /// <summary>
    /// Writes a tooltip to the console.
    /// </summary>
    /// <param name="tooltip">The tooltip to write.</param>
    public static void WriteLine(Tooltip tooltip)
    {
        foreach (Word word in tooltip.Words)
            Write(word);

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine();
    }

    /// <summary>
    /// Writes a word to the console.
    /// </summary>
    /// <param name="word">The word to write.</param>
    public static void Write(Word word) => Write(word.Text, word.Fragment);

    /// <summary>
    /// Writes text to the console with specific fragment style information.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="fragment">The corresponding fragment.</param>
    public static void Write(string text, Fragment fragment)
    {
        SetColor(GetColor(fragment.Color));
        Console.Write(text);
    }

    private static int GetOffset(string text, int line, int column)
    {
        int currentLine = 1;
        int currentColumn = 1;
        int offset = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (currentLine == line && currentColumn == column)
                return offset;

            currentColumn++;
            offset++;

            if (text[i] == '\n')
            {
                currentLine++;
                currentColumn = 1;
            }
        }

        return -1;
    }

    /// <summary>
    /// Writes source code with fragments to the console. Does not include basic syntax highlighting for keywords.
    /// </summary>
    /// <param name="source">The source code.</param>
    /// <param name="fragments">A list of fragments for color information.</param>
    public static void Write(string source, IEnumerable<Fragment> fragments)
    {
        for (int i = 0; i < source.Length; i++)
        {
            bool Predicate(Fragment f) => GetOffset(source, f.Line, f.Column) == i;

            if (fragments.Any(Predicate))
                SetColor(GetColor(fragments.First(Predicate).Color));

            Console.Write(source[i]);
        }
    }
}