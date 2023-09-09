using Antlr4.Runtime.Misc;
using LoschScript.Parser;
using LoschScript.Text;
using System;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace LoschScript.CodeGeneration;

internal class ExpressionEvaluator : LoschScriptParserBaseVisitor<Expression>
{
    public override Expression VisitString_atom([NotNull] LoschScriptParser.String_atomContext context)
    {
        string text = context.GetText()[1..^1];

        if (context.Verbatim_String_Literal() != null)
        {
            string verbatimText = text[1..];

            Regex doubleQuote = new(@"""""");
            foreach (Match match in doubleQuote.Matches(verbatimText))
            {
                CurrentFile.Fragments.Add(new()
                {
                    Color = Color.StringEscapeSequence,
                    Length = match.Length,
                    Line = context.Start.Line,
                    Column = context.Start.Column + match.Index + 1,
                });
            }

            return new(typeof(string), verbatimText);
        }

        Regex escapeSequenceRegex = new(@"\^(?:['""^0abfnrtv]|[0-9A-Fa-f]{1,4})");
        foreach (Match match in escapeSequenceRegex.Matches(text))
        {
            CurrentFile.Fragments.Add(new()
            {
                Color = Color.StringEscapeSequence,
                Length = match.Length,
                Line = context.Start.Line,
                Column = context.Start.Column + match.Index + 1,
            });
        }

        string rawText = context.GetText()[1..^1]
            .Replace("^'", "'")
            .Replace("^\"", "\"")
            .Replace("^^", "^")
            .Replace("^0", "\0")
            .Replace("^a", "\a")
            .Replace("^b", "\b")
            .Replace("^f", "\f")
            .Replace("^n", "\n")
            .Replace("^r", "\r")
            .Replace("^t", "\t")
            .Replace("^v", "\v");

        // TODO: Handle Hex and Unicode escape sequences

        return new(typeof(string), rawText);
    }

    public override Expression VisitCharacter_atom([NotNull] LoschScriptParser.Character_atomContext context)
    {
        char rawChar = char.Parse(context.GetText()
            .Replace("^'", "'")
            .Replace("^\"", "\"")
            .Replace("^^", "^")
            .Replace("^0", "\0")
            .Replace("^a", "\a")
            .Replace("^b", "\b")
            .Replace("^f", "\f")
            .Replace("^n", "\n")
            .Replace("^r", "\r")
            .Replace("^t", "\t")
            .Replace("^v", "\v")[1..^1]);

        return new(typeof(char), rawChar);
    }

    public override Expression VisitInteger_atom([NotNull] LoschScriptParser.Integer_atomContext context)
    {
        string text = context.GetText();

        if (text.EndsWith("sb", StringComparison.OrdinalIgnoreCase))
        {
            return new(typeof(sbyte), sbyte.Parse(text[0..^2].Replace("'", "")));
        }

        if (text.EndsWith("b", StringComparison.OrdinalIgnoreCase))
        {
            text += "0";
            return new(typeof(byte), byte.Parse(text[0..^2].Replace("'", "")));
        }

        if (text.EndsWith("us", StringComparison.OrdinalIgnoreCase))
        {
            return new(typeof(ushort), ushort.Parse(text[0..^2].Replace("'", "")));
        }

        if (text.EndsWith("s", StringComparison.OrdinalIgnoreCase))
        {
            text += "0";
            return new(typeof(short), short.Parse(text[0..^2].Replace("'", "")));
        }

        if (text.EndsWith("ul", StringComparison.OrdinalIgnoreCase))
        {
            return new(typeof(ulong), ulong.Parse(text[0..^2].Replace("'", "")));
        }

        if (text.EndsWith("u", StringComparison.OrdinalIgnoreCase))
        {
            text += "0";
            return new(typeof(uint), uint.Parse(text[0..^2].Replace("'", "")));
        }

        if (text.EndsWith("l", StringComparison.OrdinalIgnoreCase))
        {
            text += "0";
            return new(typeof(long), long.Parse(text[0..^2].Replace("'", "")));
        }

        if (text.EndsWith("un", StringComparison.OrdinalIgnoreCase))
        {
            return new(typeof(uint), uint.Parse(text[0..^2].Replace("'", "")));
        }

        if (text.EndsWith("n", StringComparison.OrdinalIgnoreCase))
        {
            text += "0";
            return new(typeof(int), int.Parse(text[0..^2].Replace("'", "")));
        }

        text += "00";
        return new(typeof(int), int.Parse(text[0..^2].Replace("'", "")));
    }
}