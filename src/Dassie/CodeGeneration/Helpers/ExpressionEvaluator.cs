using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Dassie.CodeGeneration.Structure;
using Dassie.Helpers;
using Dassie.Parser;
using Dassie.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Dassie.CodeGeneration.Helpers;

internal class ExpressionEvaluator : DassieParserBaseVisitor<Expression>
{
    private static ExpressionEvaluator _instance;
    public static ExpressionEvaluator Instance => _instance ??= new();
    
    private ExpressionEvaluator() { }

    private bool _requireNonZeroValue = false;

    public bool RequireNonZeroValue
    {
        get
        {
            if (!_requireNonZeroValue)
                return false;

            _requireNonZeroValue = false;
            return true;
        }

        set => _requireNonZeroValue = value;
    }

    public override Expression VisitExpression_atom([NotNull] DassieParser.Expression_atomContext context)
    {
        return Visit(context.expression());
    }

    public override Expression VisitNewlined_expression([NotNull] DassieParser.Newlined_expressionContext context)
    {
        return Visit(context.expression());
    }

    public override Expression VisitAddition_expression([NotNull] DassieParser.Addition_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(a.Type, a.Value + b.Value);
    }

    public override Expression VisitSubtraction_expression([NotNull] DassieParser.Subtraction_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(a.Type, a.Value - b.Value);
    }

    public override Expression VisitMultiply_expression([NotNull] DassieParser.Multiply_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(a.Type, a.Value * b.Value);
    }

    public override Expression VisitDivide_expression([NotNull] DassieParser.Divide_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(a.Type, a.Value / b.Value);
    }

    public override Expression VisitRemainder_expression([NotNull] DassieParser.Remainder_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(a.Type, a.Value % b.Value);
    }

    public override Expression VisitModulus_expression([NotNull] DassieParser.Modulus_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(a.Type, (a.Value % b.Value + b.Value) % b.Value);
    }

    public override Expression VisitLeft_shift_expression([NotNull] DassieParser.Left_shift_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(a.Type, a.Value << b.Value);
    }

    public override Expression VisitRight_shift_expression([NotNull] DassieParser.Right_shift_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(a.Type, a.Value >> b.Value);
    }

    public override Expression VisitLogical_and_expression([NotNull] DassieParser.Logical_and_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(a.Type, a.Value & b.Value);
    }

    public override Expression VisitAnd_expression([NotNull] DassieParser.And_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(a.Type, a.Value & b.Value);
    }

    public override Expression VisitOr_expression([NotNull] DassieParser.Or_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(a.Type, a.Value | b.Value);
    }

    public override Expression VisitLogical_or_expression([NotNull] DassieParser.Logical_or_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(a.Type, a.Value | b.Value);
    }

    public override Expression VisitXor_expression([NotNull] DassieParser.Xor_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(a.Type, a.Value ^ b.Value);
    }

    public override Expression VisitPower_expression([NotNull] DassieParser.Power_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(typeof(double), Math.Pow(a.Value, b.Value));
    }

    public override Expression VisitEquality_expression([NotNull] DassieParser.Equality_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        if (context.op.Text == "==")
            return a.Value == b.Value;

        return new(typeof(bool), a.Value != b.Value);
    }

    public override Expression VisitComparison_expression([NotNull] DassieParser.Comparison_expressionContext context)
    {
        Expression a = Visit(context.expression()[0]);
        Expression b = Visit(context.expression()[1]);

        if (a == null || b == null)
            return null;

        return new(typeof(bool), context.op.Text switch
        {
            "<" => a.Value < b.Value,
            ">" => a.Value > b.Value,
            "<=" => a.Value <= b.Value,
            _ => a.Value >= b.Value
        });
    }

    public override Expression VisitBitwise_complement_expression([NotNull] DassieParser.Bitwise_complement_expressionContext context)
    {
        Expression a = Visit(context.expression());

        if (a == null)
            return null;

        return new(a.Type, ~a.Value);
    }

    public override Expression VisitLogical_negation_expression([NotNull] DassieParser.Logical_negation_expressionContext context)
    {
        Expression a = Visit(context.expression());

        if (a == null)
            return null;

        return new(a.Type, -a.Value);
    }

    public override Expression VisitString_atom([NotNull] DassieParser.String_atomContext context)
    {
        string text = null;

        if (context.String_Literal() != null)
            text = context.String_Literal().GetText()[1..^1];

        if (context.Verbatim_String_Literal() != null)
        {
            text = context.Verbatim_String_Literal().GetText()[1..^1];
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

        return new(typeof(string), GetRawString(text));
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

        int count = 1;
        while (count++ <= 8 && hexDigits.Contains(char.ToUpperInvariant(c)))
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
        Expression result = VisitInteger_atom_Impl(context);

        if (RequireNonZeroValue && result.Value == 0)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                context.GetText().Length,
                DS0135_DivisionByConstantZero,
                "Division by constant zero.");
        }

        return result;
    }

    private Expression VisitInteger_atom_Impl([NotNull] DassieParser.Integer_atomContext context)
    {
        string text = context.GetText();
        Type literalType = typeof(int);
        NumberStyles style = NumberStyles.Integer;

        if (context.Hex_Integer_Literal() != null)
        {
            style = NumberStyles.HexNumber;
            text = text[2..];
        }

        if (context.Binary_Integer_Literal() != null)
        {
            style = NumberStyles.BinaryNumber;
            text = text[2..];
        }

        try
        {
            if (text.EndsWith("sb", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(sbyte);
                return new(typeof(sbyte), sbyte.Parse(text[0..^2].Replace("'", ""), style));
            }

            if (text.EndsWith("b", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(byte);
                text += "0";
                return new(typeof(byte), byte.Parse(text[0..^2].Replace("'", ""), style));
            }

            if (text.EndsWith("us", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(ushort);
                return new(typeof(ushort), ushort.Parse(text[0..^2].Replace("'", ""), style));
            }

            if (text.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(short);
                text += "0";
                return new(typeof(short), short.Parse(text[0..^2].Replace("'", ""), style));
            }

            if (text.EndsWith("ul", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(ulong);
                return new(typeof(ulong), ulong.Parse(text[0..^2].Replace("'", ""), style));
            }

            if (text.EndsWith("u", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(uint);
                text += "0";
                return new(typeof(uint), uint.Parse(text[0..^2].Replace("'", ""), style));
            }

            if (text.EndsWith("l", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(long);
                text += "0";
                return new(typeof(long), long.Parse(text[0..^2].Replace("'", ""), style));
            }

            if (text.EndsWith("un", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(uint);
                return new(typeof(uint), uint.Parse(text[0..^2].Replace("'", ""), style));
            }

            if (text.EndsWith("n", StringComparison.OrdinalIgnoreCase))
            {
                literalType = typeof(int);
                text += "0";
                return new(typeof(int), int.Parse(text[0..^2].Replace("'", ""), style));
            }

            text += "00";
            return new(typeof(int), int.Parse(text[0..^2].Replace("'", ""), style));
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
        Expression result = VisitReal_atom_Impl(context);

        if (RequireNonZeroValue && result.Value == 0)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                context.GetText().Length,
                DS0135_DivisionByConstantZero,
                "Division by constant zero.");
        }

        return result;
    }

    private Expression VisitReal_atom_Impl([NotNull] DassieParser.Real_atomContext context)
    {
        string text = context.GetText();

        if (text.EndsWith("s"))
            return new(typeof(float), float.Parse(text[0..^1].Replace("'", ""), CultureInfo.GetCultureInfo("en-US")));

        if (text.EndsWith("d"))
            return new(typeof(double), double.Parse(text[0..^1].Replace("'", ""), CultureInfo.GetCultureInfo("en-US")));

        if (text.EndsWith("m"))
            return new(typeof(decimal), decimal.Parse(text[0..^1].Replace("'", ""), CultureInfo.GetCultureInfo("en-US")));

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

    //public override Expression VisitIndex([NotNull] DassieParser.IndexContext context)
    //{
    //    Expression a = Visit(context.integer_atom());
    //    return new(typeof(Index), new Index(a.Value, context.Caret() != null));
    //}

    public override Expression VisitArray_expression([NotNull] DassieParser.Array_expressionContext context)
    {
        if (context.expression() == null || context.expression().Length == 0)
            return new(typeof(object[]), Array.Empty<object>());

        Type elementType = null;
        List<object> items = [];

        foreach (IParseTree tree in context.expression())
        {
            Expression expr = Visit(tree);
            if (expr == null)
                return null;

            if (elementType == null)
                elementType = expr.Type;

            if (elementType != null && expr.Type != elementType)
                return null;

            items.Add(expr.Value);
        }

        MethodInfo cast = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(elementType);
        MethodInfo toArray = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(elementType);
        return new(elementType.MakeArrayType(), toArray.Invoke(null, [cast.Invoke(null, [items])]));
    }
}