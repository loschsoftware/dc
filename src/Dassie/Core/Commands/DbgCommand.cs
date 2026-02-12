using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Extensions;
using Dassie.Meta;
using Dassie.Parser;
using Dassie.Text;
using System;
using System.Linq;
using System.Text;

namespace Dassie.Core.Commands;

internal class DebugException(string message) : Exception(message) { }

internal class DbgCommand : CompilerCommand
{
    internal class ParseTreePrinter
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

    public override string Command => "dbg";

    public override string Description => "Commands used for debugging and testing the compiler. To be used by compiler developers.";
    public override CommandOptions Options => CommandOptions.Hidden;

    public override int Invoke(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            LogOut.WriteLine("No command specified.");
            return -1;
        }

        if (args[0] == "fail")
            return Fail();

        if (args[0] == "ast")
            return PrintParseTree(args[1..]);

        if (args[0] == "tokens")
            return PrintTokens(args[1..]);

        if (args[0] == "fragments")
            return PrintFragments(args[1..]);

        if (args[0] == "clear-cache")
            return ClearPackageCache();

        LogOut.WriteLine($"Invalid command '{args[0]}'.");
        return -1;
    }

    private static int PrintFragments(string[] args)
    {
        if (args.Any(p => !File.Exists(p)))
        {
            foreach (string path in args.Where(p => !File.Exists(p)))
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0049_SourceFileNotFound,
                    $"The specified source file '{path}' could not be found.",
                    CompilerExecutableName);
            }
        }

        CompileCommand.Instance.Invoke([.. args.Where(File.Exists)]);

        foreach (FileContext file in Context.Files)
        {
            Console.WriteLine($"{file.Path}:");

            foreach (Fragment fragment in file.Fragments)
            {
                Console.Write($"    {fragment.Color} ({fragment.Line},{fragment.Column})+{fragment.Length}: ");
                ConsoleOut.WriteLine(fragment.ToolTip);
            }
        }

        return 0;
    }

    private static int Fail()
    {
        throw new DebugException("Exception thrown due to call of 'dbg fail'");
    }

    private static int Print(bool tree, string[] args)
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
        CommonTokenStream tokens = new(lexer);
        DassieParser parser = new(tokens);

        // Print parse tree
        if (tree)
        {
            if (args.Contains("-c") || args.Contains("--compressed"))
                LogOut.WriteLine(parser.compilation_unit().ToStringTree(parser));
            else
                LogOut.WriteLine(ParseTreePrinter.PrintTree(parser.compilation_unit(), parser));

            return 0;
        }

        // Print tokens
        foreach ((int i, IToken token) in lexer.GetAllTokens().Index())
            LogOut.WriteLine($"#{i + 1} [{token.StartIndex}-{token.StopIndex}] {DassieLexer.DefaultVocabulary.GetSymbolicName(token.Type)}: \"{token.Text}\"");

        return 0;
    }

    private static int PrintParseTree(string[] args)
    {
        return Print(true, args);
    }

    private static int PrintTokens(string[] args)
    {
        return Print(false, args);
    }

    private static int ClearPackageCache()
    {
        Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages"), true);
        return 0;
    }
}