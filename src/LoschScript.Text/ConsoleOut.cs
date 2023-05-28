using LoschScript.Text.Tooltips;
using System;

namespace LoschScript.Text;

/// <summary>
/// Provides helper methods for writing colored fragments or tooltips to the console.
/// </summary>
public static class ConsoleOut
{
    /// <summary>
    /// Writes a tooltip to the console.
    /// </summary>
    /// <param name="tooltip">The tooltip to write.</param>
    public static void WriteLine(Tooltip tooltip)
    {
        foreach (Word word in tooltip.Words)
        {
            Console.ForegroundColor = word.Fragment.Color switch
            {
                Color.Word => ConsoleColor.Blue,
                Color.ControlFlow or Color.Module => ConsoleColor.Magenta,
                Color.Function or Color.TypeParameter => ConsoleColor.Yellow,
                Color.ReferenceType => ConsoleColor.Cyan,
                Color.ValueType => ConsoleColor.Green,
                Color.TemplateType => ConsoleColor.DarkYellow,
                Color.LocalValue or Color.LocalVariable => ConsoleColor.DarkBlue,
                _ => ConsoleColor.Gray
            };

            Console.Write(word.Text);
        }

        Console.ForegroundColor = ConsoleColor.Gray;

        Console.WriteLine();
    }
}