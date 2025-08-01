using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.CodeAnalysis;
using Dassie.CodeAnalysis.Default;
using Dassie.Configuration;
using Dassie.Errors;
using Dassie.Extensions;
using Dassie.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dassie.Cli.Commands;

internal class AnalyzeCommand : ICompilerCommand
{
    private static AnalyzeCommand _instance;
    public static AnalyzeCommand Instance => _instance ??= new();

    public string Command => "analyze";

    public string UsageString => "analyze [Options]";

    public string Description => "Runs code analyzers on the current project or on a list of source files.";

    public CommandHelpDetails HelpDetails() => new()
    {
        Description = Description,
        Usage =
        [
            "dc analyze [(--analyzer|-a)=<Name>]",
            "dc analyze <Files> [(--analyzer|-a)=<Name>]",
        ],
        Remarks = "Code analyzers other than the default one are installed as part of compiler extensions (packages). Managing compiler extensions is facilitated through the 'dc package' command.",
        Options =
        [
            ("(--analyzer | -a)=<Name>", "The name of the code analyzer to run. If none is specified, the default analyzer is used."),
            ("Files", "A list of source files to analyze. If this option is not used, all source files in the current project will be analyzed.")
        ]
    };

    public int Invoke(string[] args)
    {
        IEnumerable<string> files = args.Where(File.Exists);
        string analyzerName = null;

        if (args.Any(a => a.StartsWith("-a=") || a.StartsWith("--analyzer=")))
            analyzerName = string.Join('=', args.First(a => a.StartsWith("-a=") || a.StartsWith("--analyzer=")).Split('=')[1..]);

        if (files.Any())
            return AnalyzeFiles(files, analyzerName: analyzerName);

        Func<string, bool> predicate = static a => !a.StartsWith("-a=") && !a.StartsWith("--analyzer=") && !File.Exists(a);
        if (args.Any(predicate))
        {
            foreach (string arg in args.Where(predicate))
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0211_UnexpectedArgument,
                    $"Unexpected argument '{arg}'.",
                    CompilerExecutableName);
            }

            return -1;
        }

        return AnalyzeProject(analyzerName);
    }

    private static int AnalyzeFiles(IEnumerable<string> files, DassieConfig config = null, string analyzerName = null)
    {
        ParseTreeAnalyzer<IParseTree> analyzer = null;

        if (string.IsNullOrEmpty(analyzerName))
            analyzer = new NamingConventionAnalyzer(config);
        else
        {
            if (ExtensionLoader.TryGetAnalyzer(analyzerName, out IAnalyzer<IParseTree> a))
            {
                if (a is ParseTreeAnalyzer<IParseTree> pta)
                    analyzer = pta;
                else
                    throw new NotImplementedException("'dc analyze' only supports parse tree analyzers at this point.");
            }
            else
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0134_DCAnalyzeInvalidAnalyzer,
                    $"The analyzer '{analyzerName}' could not be found.",
                    CompilerExecutableName);

                return -1;
            }
        }

        List<ErrorInfo> errors = [];

        foreach (string file in files)
        {
            using FileStream fs = File.OpenRead(file);
            ICharStream charStream = CharStreams.fromStream(fs);
            DassieLexer lexer = new(charStream);
            ITokenStream tokenStream = new CommonTokenStream(lexer);
            DassieParser parser = new(tokenStream);

            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new LexerErrorListener());
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ParserErrorListener());

            CurrentFile = new(file);

            DassieParser.Compilation_unitContext compilationUnit = parser.compilation_unit();
            errors.AddRange(analyzer.Analyze([compilationUnit]));
        }

        foreach (ErrorInfo error in errors)
            EmitGeneric(error);

        if (errors.Count == 0)
            WriteLine($"{Environment.NewLine}Analysis completed without messages.");

        return 0;
    }

    private static int AnalyzeProject(string analyzerName = null)
    {
        if (!File.Exists(ProjectConfigurationFileName))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0133_DCAnalyzeNoProjectFile,
                "The current directory contains no project file.",
                file: CompilerExecutableName);

            return -1;
        }

        string[] files = Directory.EnumerateFiles("./", "*.ds", SearchOption.AllDirectories).ToArray();
        files = files.Where(f => Path.GetDirectoryName(f).Split(Path.DirectorySeparatorChar).Last() != TemporaryBuildDirectoryName).ToArray();

        return AnalyzeFiles(files, ProjectFileDeserializer.DassieConfig, analyzerName);
    }
}