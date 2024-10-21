﻿using Antlr4.Runtime.Misc;
using Dassie.Cli;
using Dassie.Helpers;
using Dassie.Parser;
using Dassie.Runtime;
using Dassie.Text;
using NuGet.Protocol;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dassie.CodeGeneration;

internal class ExpressionEvaluator : DassieParserBaseVisitor<Expression>
{
    public override Expression VisitString_atom([NotNull] DassieParser.String_atomContext context)
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

            return new(typeof(string), verbatimText.Replace("\"\"", "\""));
        }

        Regex escapeSequenceRegex = new(@"\^(?:['""^0abefnrtv]|[0-9A-Fa-f]{1,4})");
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

        return new(typeof(string), GetRawString(context.GetText()[1..^1]));
    }

    private static char GetChar(StringReader reader, int count)
    {
        StringBuilder sequence = new();
        for (int i = 0; i < count; i++)
            sequence.Append((char)reader.Read());

        return (char)int.Parse(sequence.ToString(), NumberStyles.HexNumber);
    }

    private static char HandleUtf16EscapeSequence(StringReader reader) => GetChar(reader, 4);
    private static char HandleUtf32EscapeSequence(StringReader reader) => GetChar(reader, 8);

    private static char HandleVariableLengthUnicodeEscapeSequence(StringReader reader)
    {
        StringBuilder sequence = new();

        char c = (char)reader.Read();
        char[] hexDigits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'];
        
        while (hexDigits.Contains(char.ToUpperInvariant(c)))
        {
            sequence.Append(c);
            c = (char)reader.Read();
        }

        while (sequence.Length < 4)
            sequence.Insert(0, '0');

        return (char)int.Parse(sequence.ToString(), NumberStyles.HexNumber);
    }

    private static string GetRawString(string str)
    {
        StringReader sr = new(str);
        StringBuilder sb = new();

        while (sr.Peek() != -1)
        {
            char c = (char)sr.Read();

            if (c != '^')
                sb.Append(c);
            else
            {
                char escapeChar = (char)sr.Read();
                sb.Append(escapeChar switch
                {
                    '0' => '\0',
                    'a' => '\a',
                    'b' => '\b',
                    'e' => '\x1b',
                    'f' => '\f',
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    'v' => '\v',
                    'u' => HandleUtf16EscapeSequence(sr),
                    'U' => HandleUtf32EscapeSequence(sr),
                    'x' => HandleVariableLengthUnicodeEscapeSequence(sr),
                    _ => escapeChar
                });
            }
        }

        return sb.ToString();
    }

    public override Expression VisitCharacter_atom([NotNull] DassieParser.Character_atomContext context)
    {
        if (context.GetText().Length < 3)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                context.GetText().Length,
                DS0076_EmptyCharacterLiteral,
                "A character literal cannot be empty.");

            return new(typeof(char), ' ');
        }

        return new(typeof(char), GetRawString(context.GetText()[1..^1])[0]);
    }

    public override Expression VisitInteger_atom([NotNull] DassieParser.Integer_atomContext context)
    {
        string text = context.GetText();
        Type literalType = typeof(int);

        try
        {
            if (text.EndsWith("sb", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(sbyte);
                return new(typeof(sbyte), sbyte.Parse(text[0..^2].Replace("'", "")));
            }

            if (text.EndsWith("b", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(byte);
                text += "0";
                return new(typeof(byte), byte.Parse(text[0..^2].Replace("'", "")));
            }

            if (text.EndsWith("us", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(ushort);
                return new(typeof(ushort), ushort.Parse(text[0..^2].Replace("'", "")));
            }

            if (text.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(short);
                text += "0";
                return new(typeof(short), short.Parse(text[0..^2].Replace("'", "")));
            }

            if (text.EndsWith("ul", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(ulong);
                return new(typeof(ulong), ulong.Parse(text[0..^2].Replace("'", "")));
            }

            if (text.EndsWith("u", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(uint);
                text += "0";
                return new(typeof(uint), uint.Parse(text[0..^2].Replace("'", "")));
            }

            if (text.EndsWith("l", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(long);
                text += "0";
                return new(typeof(long), long.Parse(text[0..^2].Replace("'", "")));
            }

            if (text.EndsWith("un", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(uint);
                return new(typeof(uint), uint.Parse(text[0..^2].Replace("'", "")));
            }

            if (text.EndsWith("n", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(int);
                text += "0";
                return new(typeof(int), int.Parse(text[0..^2].Replace("'", "")));
            }

            text += "00";
            return new(typeof(int), int.Parse(text[0..^2].Replace("'", "")));
        }
        catch (OverflowException)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                context.GetText().Length,
                DS0075_Overflow,
                $"The literal is too large or too small for type '{literalType.FullName}'.");
        }

        return new(typeof(int), 1);
    }

    public override Expression VisitReal_atom([NotNull] DassieParser.Real_atomContext context)
    {
        string text = context.GetText();

        if (text.EndsWith("s"))
            return new(typeof(float), float.Parse(text[0..^1].Replace("'", ""), CultureInfo.GetCultureInfo("en-US")));

        if (text.EndsWith("d"))
            return new(typeof(double), double.Parse(text[0..^1].Replace("'", ""), CultureInfo.GetCultureInfo("en-US")));

        if (text.EndsWith("m"))
        {
            // TODO: Apparently decimals are a pain in the ass... For now we'll cheat and emit doubles instead
            return new(typeof(double), double.Parse(text[0..^1].Replace("'", ""), CultureInfo.GetCultureInfo("en-US")));
        }

        return new(typeof(double), double.Parse(text.Replace("'", ""), CultureInfo.GetCultureInfo("en-US")));
    }

    public override Expression VisitBoolean_atom([NotNull] DassieParser.Boolean_atomContext context)
    {
        return new(typeof(bool), context.False() == null);
    }

    public override Expression VisitType_name([NotNull] DassieParser.Type_nameContext context)
    {
        return new(typeof(Type), SymbolResolver.ResolveTypeName(context));

        //if (context.Ampersand() != null)
        //    return new(typeof(Type), VisitType_name(context.type_name().First()).Value.MakeByRefType());

        //if (context.identifier_atom() != null)
        //{
        //    bool success = SymbolResolver.TryGetType(
        //                      context.identifier_atom().GetText(),
        //                      out Type t,
        //                      context.identifier_atom().Start.Line,
        //                      context.identifier_atom().Start.Column,
        //                      context.identifier_atom().GetText().Length);

        //    if (success)
        //        return new(typeof(Type), t);
        //}

        //if (context.Bar() != null)
        //{
        //    UnionValue union = new(null, context.type_name().Select(VisitType_name).Select(e => (Type)e.Value).ToArray());
        //    CurrentMethod.CurrentUnion = union;

        //    if (union.AllowedTypes.Distinct().Count() < union.AllowedTypes.Length)
        //    {
        //        EmitWarningMessage(
        //            context.Start.Line,
        //            context.Start.Column,
        //            context.GetText().Length,
        //            DS0047_UnionTypeDuplicate,
        //            "The union type contains duplicate cases.");
        //    }

        //    return new(union.GetType(), union.GetType());
        //}

        //// TODO: Implement the other types
        //return new(typeof(object), typeof(object));
    }

    public override Expression VisitEmpty_atom([NotNull] DassieParser.Empty_atomContext context)
    {
        return new(typeof(object), null);
    }

    public override Expression VisitIndex([NotNull] DassieParser.IndexContext context)
    {
        Expression a = Visit(context.integer_atom());
        return new(typeof(Index), new Index(a.Value, context.Caret() != null));
    }
}