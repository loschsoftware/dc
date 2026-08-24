using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Configuration;
using Dassie.Extensions;
using Dassie.Meta;
using Dassie.Parser;
using Dassie.Syntax;
using Dassie.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Dassie.Helpers.TreeExportHelper;

namespace Dassie.Core.Commands;

internal class DebugException(string message) : Exception(message) { }

internal class DbgCommand : CompilerCommand
{
    private enum TreeKind
    {
        Tokens,
        ParseTree,
        SyntaxTree,
        BoundTree
    }

    internal class TreePrinter
    {
        public static string PrintMermaid(IParseTree tree, DassieParser parser)
        {
            Node GetNode(IParseTree tree)
            {
                if (tree is TerminalNodeImpl terminalNode)
                    return new(terminalNode.GetText(), []);
                else if (tree is ParserRuleContext ruleContext)
                {
                    List<Node> children = [];
                    for (int i = 0; i < tree.ChildCount; i++)
                        children.Add(GetNode(tree.GetChild(i)));

                    return new(parser.RuleNames[ruleContext.RuleIndex], children);
                }

                return null;
            }

            return TreeExportHelper.ExportMermaid(GetNode(tree));
        }

        public static string PrintMermaid(SyntaxNode node)
        {
            static Node GetNode(SyntaxNode node)
            {
                if (node is SyntaxToken token)
                    return new(token.Text, []);

                List<Node> children = [];
                foreach (SyntaxNode child in node.GetChildren())
                    children.Add(GetNode(child));

                return new(node.GetType().Name, children);
            }

            return TreeExportHelper.ExportMermaid(GetNode(node));
        }

        public static string PrintParseTree(IParseTree tree, DassieParser parser, int indentLevel = 0)
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

        public static string PrintSyntaxNode(SyntaxNode node, int indentLevel = 0)
        {
            StringBuilder sb = new();
            PrintNode(node, indentLevel, sb);
            return sb.ToString();
        }

        private static void PrintNode(SyntaxNode node, int indentLevel, StringBuilder sb)
        {
            string indent = new(' ', indentLevel * 2);

            if (node is SyntaxToken token)
                sb.AppendLine($"{indent}\"{token.Value}\"");
            else
            {
                sb.AppendLine($"{indent}{node.GetType().Name}");

                foreach (SyntaxNode child in node.GetChildren())
                    PrintNode(child, indentLevel + 1, sb);
            }
        }
    }

    private static DbgCommand _instance;
    public static DbgCommand Instance => _instance ??= new();

    public override string Command => "dbg";

    public override string Description => StringHelper.DbgCommand_Description;
    public override CommandOptions Options => CommandOptions.Hidden;

    public override int Invoke(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            LogOut.WriteLine(StringHelper.DbgCommand_NoCommandSpecified);
            return -1;
        }

        if (args[0] == "fail")
            return Fail();

        if (args[0] == "parse-tree")
            return PrintParseTree(args[1..]);

        if (args[0] == "syntax-tree")
            return PrintSyntaxTree(args[1..]);

        if (args[0] == "bound-tree")
            return PrintBoundTree(args[1..]);

        if (args[0] == "tokens")
            return PrintTokens(args[1..]);

        if (args[0] == "fragments")
            return PrintFragments(args[1..]);

        if (args[0] == "clear-cache")
            return ClearPackageCache();

        if (args[0] == "clear-temp")
            return ClearTempDir();

        if (args[0] == "print")
            return PrintText(args[1..]);

        LogOut.WriteLine(StringHelper.Format(nameof(StringHelper.DbgCommand_InvalidCommand), args[0]));
        return -1;
    }

    private static int PrintFragments(string[] args)
    {
        if (args.Any(p => !File.Exists(p)))
        {
            foreach (string path in args.Where(p => !File.Exists(p)))
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0049_SourceFileNotFound,
                    nameof(StringHelper.DbgCommand_SourceFileNotFound), [path],
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

    private static int Print(TreeKind kind, string[] args)
    {
        if (args == null || args.Length == 0)
        {
            LogOut.WriteLine(StringHelper.DbgCommand_NoFileSpecified);
            return -1;
        }

        if (!File.Exists(args[0]))
        {
            LogOut.WriteLine(StringHelper.DbgCommand_FileDoesNotExist);
            return -1;
        }

        string text = File.ReadAllText(args[0]);
        ICharStream charStream = CharStreams.fromString(text);
        DassieLexer lexer = new(charStream);
        CommonTokenStream tokens = new(lexer);
        DassieParser parser = new(tokens);

        if (kind == TreeKind.ParseTree)
        {
            if (args.Contains("-c") || args.Contains("--compressed"))
                LogOut.WriteLine(parser.compilation_unit().ToStringTree(parser));
            else if (args.Contains("-m") || args.Contains("--mermaid"))
                LogOut.WriteLine(TreePrinter.PrintMermaid(parser.compilation_unit(), parser));
            else
                LogOut.WriteLine(TreePrinter.PrintParseTree(parser.compilation_unit(), parser));

            return 0;
        }

        if (kind == TreeKind.SyntaxTree)
        {
            SyntaxTreeGenerator generator = new();
            SyntaxNode node = generator.Visit(parser.compilation_unit());

            if (args.Contains("-m") || args.Contains("--mermaid"))
                LogOut.WriteLine(TreePrinter.PrintMermaid(node));
            else
                LogOut.WriteLine(TreePrinter.PrintSyntaxNode(node));

            return 0;
        }

        if (kind == TreeKind.BoundTree)
        {
            throw new NotSupportedException();
        }

        // Print tokens
        foreach ((int i, IToken token) in lexer.GetAllTokens().Index())
            LogOut.WriteLine($"#{i + 1} [{token.StartIndex}-{token.StopIndex}] {DassieLexer.DefaultVocabulary.GetSymbolicName(token.Type)}: \"{token.Text}\"");

        return 0;
    }

    private static int PrintParseTree(string[] args)
    {
        return Print(TreeKind.ParseTree, args);
    }

    private static int PrintSyntaxTree(string[] args)
    {
        return Print(TreeKind.SyntaxTree, args);
    }

    private static int PrintBoundTree(string[] args)
    {
        return Print(TreeKind.BoundTree, args);
    }

    private static int PrintTokens(string[] args)
    {
        return Print(TreeKind.Tokens, args);
    }

    private static int ClearPackageCache()
    {
        string packageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages");
        if (!Directory.Exists(packageDir))
            return 0;

        Directory.Delete(packageDir, true);
        return 0;
    }

    private static int ClearTempDir()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "Dassie");
        if (!Directory.Exists(tempDir))
            return 0;

        Directory.Delete(tempDir, true);
        return 0;
    }

    private static int PrintText(string[] args)
    {
        string text = string.Join(' ', args);
        _ = ProjectFileSerializer.DassieConfig;

        if (ProjectFileSerializer.MacroParser == null)
        {
            Console.WriteLine(text);
            return 0;
        }

        Console.WriteLine(ProjectFileSerializer.MacroParser.Expand(text).Value);
        return 0;
    }
}