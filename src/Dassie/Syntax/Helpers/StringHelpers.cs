using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Dassie.Syntax.Helpers;

internal class StringHelpers
{
    private static readonly Dictionary<char, char> _escapeTable = new()
    {
        ['\0'] = '0',
        ['\a'] = 'a',
        ['\b'] = 'b',
        ['\e'] = 'e',
        ['\f'] = 'f',
        ['\n'] = 'n',
        ['\r'] = 'r',
        ['\t'] = 't',
        ['\v'] = 'v',
        ['"'] = '"',
        ['^'] = '^'
    };

    private static readonly Dictionary<char, char> _unescapeTable = new()
    {
        ['0'] = '\0',
        ['a'] = '\a',
        ['b'] = '\b',
        ['e'] = '\e',
        ['f'] = '\f',
        ['n'] = '\n',
        ['r'] = '\r',
        ['t'] = '\t',
        ['v'] = '\v'
    };

    public static string EscapeString(string str)
    {
        StringReader sr = new(str);
        StringBuilder sb = new();

        while (sr.Peek() != -1)
        {
            char c = (char)sr.Read();

            if (_escapeTable.TryGetValue(c, out char escapeChar))
                sb.Append($"^{escapeChar}");
            else
                sb.Append(c);
        }

        return sb.ToString();
    }

    public static string UnescapeString(string str)
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

                if (_unescapeTable.TryGetValue(escapeChar, out char unescapedChar))
                    sb.Append(unescapedChar);
                else
                {
                    sb.Append(escapeChar switch
                    {
                        'u' => HandleUtf16EscapeSequence(sr),
                        'U' => HandleUtf32EscapeSequence(sr),
                        'x' => HandleVariableLengthUnicodeEscapeSequence(sr),
                        _ => escapeChar
                    });
                }
            }
        }

        return sb.ToString();
    }

    private static char HandleUtf16EscapeSequence(StringReader reader) => GetChar(reader, 4);
    private static char HandleUtf32EscapeSequence(StringReader reader) => GetChar(reader, 8);

    private static char HandleVariableLengthUnicodeEscapeSequence(StringReader reader)
    {
        StringBuilder sequence = new();
        char[] hexDigits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'];

        while (sequence.Length < 4 && reader.Peek() != -1 && hexDigits.Contains(char.ToUpperInvariant((char)reader.Peek())))
            sequence.Append((char)reader.Read());

        while (sequence.Length < 4)
            sequence.Insert(0, '0');

        return (char)int.Parse(sequence.ToString(), NumberStyles.HexNumber);
    }

    private static char GetChar(StringReader reader, int count)
    {
        StringBuilder sequence = new();
        for (int i = 0; i < count; i++)
            sequence.Append((char)reader.Read());

        return (char)int.Parse(sequence.ToString(), NumberStyles.HexNumber);
    }
}
