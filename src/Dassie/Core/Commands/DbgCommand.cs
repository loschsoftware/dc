using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.Configuration;
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

        if (args[0] == "ast")
            return PrintParseTree(args[1..]);

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

    private static int Print(bool tree, string[] args)
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