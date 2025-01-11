using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Parser;

namespace Dassie.Syntax.Synthesis;

internal static partial class SyntaxFactory
{
    public static class Expression
    {
        public static DassieParser.Integer_atomContext Integer(ParserRuleContext parent, ITerminalNode node)
        {
            DassieParser.Integer_atomContext atom = new(parent, -1);
            atom.AddChild(node);
            return atom;
        }

        public static DassieParser.Real_atomContext Real(ParserRuleContext parent, ITerminalNode node)
        {
            DassieParser.Real_atomContext atom = new(parent, -1);
            atom.AddChild(node);
            return atom;
        }

        public static DassieParser.Boolean_atomContext Boolean(ParserRuleContext parent, ITerminalNode node)
        {
            DassieParser.Boolean_atomContext atom = new(parent, -1);
            atom.AddChild(node);
            return atom;
        }

        public static DassieParser.String_atomContext String(ParserRuleContext parent, ITerminalNode node)
        {
            DassieParser.String_atomContext atom = new(parent, -1);
            atom.AddChild(node);
            return atom;
        }
    }
}