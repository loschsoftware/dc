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

    public override string Description => StringHelper.AnalyzeCommand_Description;

    public override CommandHelpDetails HelpDetails => new()
    {
        Description = Description,
        Usage =
        [
            "dc analyze [(--analyzer|-a)=<Name>]",
            "dc analyze <Files> [(--analyzer|-a)=<Name>]",
            "dc analyze --markers [--marker:<Marker>] [--exclude:<Marker>] [Files]",
        ],
        Remarks = StringHelper.AnalyzeCommand_Remarks,
        Options =
        [
            ("(--analyzer | -a)=<Name>", StringHelper.AnalyzeCommand_AnalyzerOptionDescription),
            ("Files", StringHelper.AnalyzeCommand_FilesOptionDescription),
            ("--markers [Options] [Files]", StringHelper.AnalyzeCommand_MarkersOptionDescription),
            ("    --marker:<Marker>", StringHelper.AnalyzeCommand_MarkerOptionDescription),
            ("    --exclude:<Marker>", StringHelper.AnalyzeCommand_ExcludeOptionDescription)
        ],
        Examples =
        [
            ("dc analyze", StringHelper.AnalyzeCommand_Example1),
            ("dc analyze --analyzer=CustomAnalyzer", StringHelper.AnalyzeCommand_Example2),
            ("dc analyze ./src/File1.ds ./src/File2.ds", StringHelper.AnalyzeCommand_Example3),
            ("dc analyze ./src/File1.ds ./src/File2.ds -a=CustomAnalyzer", StringHelper.AnalyzeCommand_Example4),
            ("dc analyze --markers", StringHelper.AnalyzeCommand_Example5)
        ]
    };

    public override int Invoke(string[] args)
    {
        IEnumerable<string> files = args.Where(File.Exists);
        string analyzerName = null;

        foreach (string arg in args.Where(a => !a.StartsWith('-') && !File.Exists(a)))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0049_SourceFileNotFound,
                nameof(StringHelper.AnalyzeCommand_SourceFileNotFound), [arg],
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
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0135_DCAnalyzeInvalidAnalyzer,
                    nameof(StringHelper.AnalyzeCommand_AnalyzerNotFound), [analyzerName],
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
            WriteLine(StringHelper.AnalyzeCommand_AnalysisCompletedNoMessages);

        return 0;
    }

    private static int AnalyzeProject(string analyzerName = null)
    {
        if (!File.Exists(ProjectConfigurationFileName))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0134_DCAnalyzeNoProjectFile,
                nameof(StringHelper.AnalyzeCommand_NoProjectFileFound), [],
                file: CompilerExecutableName);

            return -1;
        }

        string[] files;

        try
        {
            files = Directory.EnumerateFiles("./", "*.ds", SearchOption.AllDirectories).ToArray();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0030_FileAccessDenied,
                nameof(StringHelper.AnalyzeCommand_FailedToCollectFiles), [ex.Message],
                CompilerExecutableName);

            return -1;
        }

        files = files.Where(f => Path.GetDirectoryName(f).Split(Path.DirectorySeparatorChar).Last() != TemporaryBuildDirectoryName).ToArray();
        return AnalyzeFiles(files, ProjectFileDeserializer.DassieConfig, analyzerName);
    }
}