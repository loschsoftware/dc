using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.CodeAnalysis;
using Dassie.CodeAnalysis.Default;
using Dassie.Configuration;
using Dassie.Extensions;
using Dassie.Messages;
using Dassie.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dassie.Core.Commands;

internal class AnalyzeCommand : CompilerCommand
{
    private static AnalyzeCommand _instance;
    public static AnalyzeCommand Instance => _instance ??= new();

    public override string Command => "analyze";

    public override string Description => "Runs code analyzers on the current project or on a list of source files.";

    public override CommandHelpDetails HelpDetails => new()
    {
        Description = Description,
        Usage =
        [
            "dc analyze [(--analyzer|-a)=<Name>]",
            "dc analyze <Files> [(--analyzer|-a)=<Name>]",
            "dc analyze --markers [--marker:<Marker>] [--exclude:<Marker>] [Files]",
        ],
        Remarks = "A code analyzer is a tool that examines source code for potential issues and style violations. " +
        "Code analyzers other than the default one are installed as part of compiler extensions (packages). " +
        $"Managing compiler extensions is facilitated through the 'dc package' command.{Environment.NewLine}{Environment.NewLine}" +
        "The '--markers' option provides a simple way to scan for code comments with marker symbols such as 'TODO', 'NOTE' or 'FIXME'. It searches through the current project or specified files and displays all according comments in a structured list.",
        Options =
        [
            ("(--analyzer | -a)=<Name>", "The name of the code analyzer to run. If none is specified, the default analyzer is used."),
            ("Files", "A list of source files to analyze. If this option is not used, all source files in the current project will be analyzed."),
            ("--markers [Options] [Files]", "Extracts and displays all comments containing markers such as TODO from the current project or the specified source files."),
            ("    --marker:<Marker>", "Specifies a custom marker to include in the search. Multiple can be specified."),
            ("    --exclude:<Marker>", "Specifies a marker to ignore in the search. Multiple can be specified.")
        ],
        Examples =
        [
            ("dc analyze", "Runs the default code analyzer on all source files in the current project."),
            ("dc analyze --analyzer=CustomAnalyzer", "Runs 'CustomAnalyzer' on all source files in the current project."),
            ("dc analyze ./src/File1.ds ./src/File2.ds", "Runs the default code analyzer on the specified source files."),
            ("dc analyze ./src/File1.ds ./src/File2.ds -a=CustomAnalyzer", "Runs 'CustomAnalyzer' on the specified source files."),
            ("dc analyze --markers", "Displays all comments with markers in the current project.")
        ]
    };

    public override int Invoke(string[] args)
    {
        IEnumerable<string> files = args.Where(File.Exists);
        string analyzerName = null;

        foreach (string arg in args.Where(a => !a.StartsWith('-') && !File.Exists(a)))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0049_SourceFileNotFound,
                $"The file '{arg}' could not be found.",
                CompilerExecutableName);
        }

        if (args.Contains("--markers"))
        {
            TaskAnalyzer analyzer;
            if (files.Any())
                analyzer = new(files);
            else
                analyzer = new(Directory.GetCurrentDirectory());

            IEnumerable<string> customMarkers = args.Where(a => a.StartsWith("--marker:")).Select(a => string.Join(':', a.Split(':')[1..]));
            IEnumerable<string> excludedMarkers = args.Where(a => a.StartsWith("--exclude:")).Select(a => string.Join(':', a.Split(':')[1..]));
            analyzer.ConfigureMarkers(customMarkers.Select(s => new KeyValuePair<string, int>(s, 155)).ToDictionary(), excludedMarkers.ToList());

            analyzer.PrintAll();
            return 0;
        }

        if (args.Any(a => a.StartsWith("-a=") || a.StartsWith("--analyzer=")))
            analyzerName = string.Join('=', args.First(a => a.StartsWith("-a=") || a.StartsWith("--analyzer=")).Split('=')[1..]);

        if (files.Any())
            return AnalyzeFiles(files, analyzerName: analyzerName);

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
                    DS0135_DCAnalyzeInvalidAnalyzer,
                    $"The analyzer '{analyzerName}' could not be found.",
                    CompilerExecutableName);

                return -1;
            }
        }

        List<MessageInfo> errors = [];

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

        foreach (MessageInfo error in errors)
            Emit(error);

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
                DS0134_DCAnalyzeNoProjectFile,
                "The current directory contains no project file.",
                file: CompilerExecutableName);

            return -1;
        }

        string[] files = Directory.EnumerateFiles("./", "*.ds", SearchOption.AllDirectories).ToArray();
        files = files.Where(f => Path.GetDirectoryName(f).Split(Path.DirectorySeparatorChar).Last() != TemporaryBuildDirectoryName).ToArray();

        return AnalyzeFiles(files, ProjectFileDeserializer.DassieConfig, analyzerName);
    }
}