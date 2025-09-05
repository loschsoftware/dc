using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Debug;
using Dassie.Extensions;
using Dassie.Parser;
using System;
using System.Text;

namespace Dassie.Debug
{
    internal class DebugException(string message) : Exception(message) { }
}

namespace Dassie.Cli.Commands
{
    internal class DbgCommand : ICompilerCommand
    {
        private class ParseTreePrinter
        {
            public static string PrintTree(IParseTree tree, DassieParser parser, int indentLevel = 0)
            {
                StringBuilder sb = new();
                PrintNode(tree, parser, indentLevel, sb);
                return sb.ToString();
            }

            private static void PrintNode(IParseTree node, DassieParser parser, int indentLevel, StringBuilder sb)
            {
                string indent = new(' ', indentLevel * 2);

                if (node is TerminalNodeImpl terminalNode)
                    sb.AppendLine($"{indent}\"{terminalNode.GetText()}\"");
                else if (node is ParserRuleContext ruleContext)
                {
                    string ruleName = parser.RuleNames[ruleContext.RuleIndex];
                    sb.AppendLine($"{indent}{ruleName}");

                    for (int i = 0; i < node.ChildCount; i++)
                        PrintNode(node.GetChild(i), parser, indentLevel + 1, sb);
                }
            }
        }

        private static DbgCommand _instance;
        public static DbgCommand Instance => _instance ??= new();

        public string Command => "dbg";

        public string Description => "Commands used for debugging and testing the compiler. To be used by compiler developers.";
        public bool Hidden() => true;

        public int Invoke(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                LogOut.WriteLine("No command specified.");
                return -1;
            }

            if (args[0] == "fail")
                return Fail();

            if (args[0] == "ast")
                return PrintTree(args[1..]);

            LogOut.WriteLine($"Invalid command '{args[0]}'.");
            return -1;
        }

        private static int Fail()
        {
            throw new DebugException("Exception thrown due to call of 'dbg fail'");
        }

        private static int PrintTree(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                LogOut.WriteLine("No file specified.");
                return -1;
            }

            if (!File.Exists(args[0]))
            {
                LogOut.WriteLine("The specified file does not exist.");
                return -1;
            }

            string text = File.ReadAllText(args[0]);
            ICharStream charStream = CharStreams.fromString(text);
            DassieLexer lexer = new(charStream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            DassieParser parser = new(tokens);

            if (args.Contains("-c") || args.Contains("--compressed"))
                LogOut.WriteLine(parser.compilation_unit().ToStringTree(parser));
            else
                LogOut.WriteLine(ParseTreePrinter.PrintTree(parser.compilation_unit(), parser));
            return 0;
        }
    }
}