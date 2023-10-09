using Antlr4.Runtime.Misc;
using LoschScript.CLI;
using LoschScript.Parser;
using LoschScript.Runtime;
using LoschScript.Text;
using System;
using System.Globalization;
using System.Linq;
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

            return new(typeof(string), verbatimText.Replace("\"\"", "\""));
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
                LS0075_Overflow,
                $"The integer literal is too large for type '{literalType.FullName}'.");
        }

        return new(typeof(int), 1);
    }

    public override Expression VisitReal_atom([NotNull] LoschScriptParser.Real_atomContext context)
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

    public override Expression VisitBoolean_atom([NotNull] LoschScriptParser.Boolean_atomContext context)
    {
        return new(typeof(bool), context.False() == null);
    }

    public override Expression VisitType_name([NotNull] LoschScriptParser.Type_nameContext context)
    {
        if (context.identifier_atom() != null)
        {
            bool success = SymbolResolver.TryGetType(
                              context.identifier_atom().GetText(),
                              out Type t,
                              context.identifier_atom().Start.Line,
                              context.identifier_atom().Start.Column,
                              context.identifier_atom().GetText().Length);

            if (success)
                return new(typeof(Type), t);
        }

        if (context.builtin_type_alias() != null)
        {
            string dotNetTypeName = $"System.{context.GetText() switch
            {
                "int8" => "SByte",
                "uint8" => "Byte",
                "int16" => "Int16",
                "uint16" => "UInt16",
                "int32" => "Int32",
                "uint32" => "UInt32",
                "int64" => "Int64",
                "uint64" => "UInt64",
                "float32" => "Single",
                "float64" => "Double",
                "decimal" => "Decimal",
                "native" => "IntPtr",
                "unative" => "UIntPtr",
                "bool" => "Boolean",
                "string" => "String",
                "char" => "Char",
                _ => "Object"
            }}";

            Type t = Helpers.ResolveTypeName(
                        dotNetTypeName,
                        context.Start.Line,
                        context.Start.Column,
                        context.GetText().Length);

            return new(t, t);
        }

        if (context.Bar() != null)
        {
            UnionValue union = new(null, context.type_name().Select(VisitType_name).Select(e => (Type)e.Value).ToArray());
            CurrentMethod.CurrentUnion = union;

            if (union.AllowedTypes.Distinct().Count() < union.AllowedTypes.Length)
            {
                EmitWarningMessage(
                    context.Start.Line,
                    context.Start.Column,
                    context.GetText().Length,
                    LS0047_UnionTypeDuplicate,
                    "The union type contains duplicate cases.");
            }

            return new(union.GetType(), union.GetType());
        }

        // TODO: Implement the other types
        return new(typeof(object), typeof(object));
    }

    public override Expression VisitEmpty_atom([NotNull] LoschScriptParser.Empty_atomContext context)
    {
        return new(typeof(object), null);
    }

    public override Expression VisitIndex([NotNull] LoschScriptParser.IndexContext context)
    {
        Expression a = Visit(context.integer_atom());
        return new(typeof(Index), new Index(a.Value, context.Caret() != null));
    }
}