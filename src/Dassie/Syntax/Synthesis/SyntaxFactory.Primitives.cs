using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Parser;

namespace Dassie.Syntax.Synthesis;

internal static partial class SyntaxFactory
{
    public static class Primitives
    {
        public static ITerminalNode String(string text)
        {
            text = text
                .Replace("^", "^^")
                .Replace("\0", "^0")
                .Replace("\a", "^a")
                .Replace("\b", "^b")
                .Replace("\e", "^e")
                .Replace("\f", "^f")
                .Replace("\n", "^n")
                .Replace("\r", "^r")
                .Replace("\t", "^t")
                .Replace("\v", "^v");

            return new TerminalNodeImpl(new CommonToken(DassieLexer.String_Literal, text));
        }

        private static TerminalNodeImpl Integral(object value, string suffix)
        {
            return new TerminalNodeImpl(new CommonToken(DassieLexer.Integer_Literal, $"{value}{suffix}"));
        }

        public static ITerminalNode I(nint value) => Integral(value, "n");
        public static ITerminalNode U(nuint value) => Integral(value, "un");

        public static ITerminalNode I1(sbyte value) => Integral(value, "sb");
        public static ITerminalNode U1(byte value) => Integral(value, "b");

        public static ITerminalNode I2(short value) => Integral(value, "s");
        public static ITerminalNode U2(ushort value) => Integral(value, "us");

        public static ITerminalNode I4(int value) => Integral(value, "");
        public static ITerminalNode U4(uint value) => Integral(value, "u");

        public static ITerminalNode I8(long value) => Integral(value, "l");
        public static ITerminalNode U8(ulong value) => Integral(value, "ul");

        private static TerminalNodeImpl Real(object value, string suffix)
        {
            return new TerminalNodeImpl(new CommonToken(DassieLexer.Real_Literal, $"{value}{suffix}"));
        }

        public static ITerminalNode R4(float value) => Real(value, "s");
        public static ITerminalNode R8(double value) => Real(value, "d");
        public static ITerminalNode Decimal(decimal value) => Real(value, "m");

        public static ITerminalNode True() => new TerminalNodeImpl(new CommonToken(DassieLexer.True, "true"));
        public static ITerminalNode False() => new TerminalNodeImpl(new CommonToken(DassieLexer.False, "false"));

        public static ITerminalNode Identifier(string id) => new TerminalNodeImpl(new CommonToken(DassieLexer.Identifier, id));
    }
}