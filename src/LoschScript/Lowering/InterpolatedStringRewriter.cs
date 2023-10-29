using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using LoschScript.Lowering;
using System.IO;
using System.Text;

namespace LoschScript.Lowerin;

internal class InterpolatedStringRewriter : IRewriter
{
    public string Rewrite(IParseTree tree, LoweringListener listener)
    {
        string literal = listener.GetTextForRule((ParserRuleContext)tree);

        if (!literal.StartsWith("$"))
            return literal;

        StringBuilder result = new();

        StringReader sr = new(literal[2..^1]);

        result.Append("\"\"");

        // TODO: Replace this retarded algorithm
        while (sr.Peek() != -1)
        {
            char c = (char)sr.Read();

            if (c == '{')
            {
                StringBuilder sb = new();

                while (sr.Peek() != -1)
                {
                    char c2 = (char)sr.Read();

                    if (c2 == '}')
                        break;

                    if (c2 == '{')
                    {
                        result.Append("+ \"{\"");
                        continue;
                    }
                    
                    sb.Append(c2);
                }

                result.Append($"+ {sb}");
            }
            else
                result.Append($"+ \"{c}\"");
        }

        return result.ToString();
    }
}