using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.IO;
using System.Text;

namespace Dassie.Lowering;

internal class InterpolatedStringRewriter : ITreeToStringRewriter
{
    private static bool ContainsInterpolation(string str)
    {
        StringReader sr = new(str);

        while (sr.Peek() != -1)
        {
            char c = (char)sr.Read();
            if (c == '^')
            {
                char escapeChar = (char)sr.Read();
                if (escapeChar == '{')
                    return true;
            }
        }

        return false;
    }

    public string Rewrite(IParseTree tree, LoweringListener listener)
    {
        string literal = listener.GetTextForRule((ParserRuleContext)tree);

        if (!ContainsInterpolation(literal))
            return literal;

        StringBuilder result = new();
        StringReader sr = new(literal[1..^1]);

        result.Append("\"\"");

        while (sr.Peek() != -1)
        {
            char c = (char)sr.Read();

            if (c == '^' && (char)sr.Peek() == '{')
            {
                sr.Read();
                StringBuilder sb = new();

                while (sr.Peek() != -1)
                {
                    char c2 = (char)sr.Read();

                    if (c2 == '}')
                        break;

                    sb.Append(c2);
                }

                if (sb.Length > 0)
                    result.Append($" + {{{sb}}}");
            }
            else
            {
                StringBuilder sb = new();
                sb.Append(c);

                while ((char)sr.Peek() != '^' && sr.Peek() != -1)
                    sb.Append((char)sr.Read());

                result.Append($" + \"{sb}\"");
            }
        }

        return result.ToString();
    }
}