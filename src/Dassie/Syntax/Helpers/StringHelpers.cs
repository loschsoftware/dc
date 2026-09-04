using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Dassie.Syntax.Helpers;

internal static class StringHelpers
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
                        // TODO: Emit error if length of input is insufficient
                        'u' => GetCharSequence(ReadString(sr, 4)),
                        'U' => GetCharSequence(ReadString(sr, 8)),
                        'x' => HandleVariableLengthUnicodeEscapeSequence(sr),
                        _ => escapeChar
                    });
                }
            }
        }

        return sb.ToString();
    }

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
    
    private static string ReadString(StringReader sr, int count)
    {
        StringBuilder sb = new();
        for (int i = 0; i < count && sr.Peek() != -1; i++)
            sb.Append((char)sr.Read());

        return sb.ToString();
    }

    private static string GetCharSequence(string str) =>
        char.ConvertFromUtf32(int.Parse(str, NumberStyles.HexNumber));
}