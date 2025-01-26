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
    public string Command => "analyze";

    public string UsageString => "analyze [Options]";

    public string Description => "Runs code analyzers on the current project. Use 'dc analyze --help' for more information.";

    public int Invoke(string[] args)
    {
        if (args.Length > 0 && ((string[])["-h", "--help", "/help", "/?"]).Contains(args[0]))
            return ShowUsage();

        IEnumerable<string> files = args.Where(File.Exists);
        string analyzerName = null;

        if (args.Any(a => a.StartsWith("-a=") || a.StartsWith("--analyzer=")))
            analyzerName = string.Join('=', args.First(a => a.StartsWith("-a=") || a.StartsWith("--analyzer=")).Split('=')[1..]);

        if (files.Any())
            return AnalyzeFiles(files, analyzerName: analyzerName);

        return AnalyzeProject(analyzerName);
    }

    private static int ShowUsage()
    {
        StringBuilder sb = new();

        sb.AppendLine();
        sb.AppendLine("Usage:");
        sb.Append($"{"    dc analyze [(--analyzer|-a)=<Name>]",-50}{HelpCommand.FormatLines("Runs the specified or the default analyzer on the current project or project group.", indentWidth: 50)}");
        sb.Append($"{"    dc analyze <Files> [(--analyzer|-a)=<Name>]",-50}{HelpCommand.FormatLines("Runs the specified or the default analyzer on a set of source files.", indentWidth: 50)}");

        HelpCommand.DisplayLogo();
        Console.Write(sb.ToString());
        return 0;
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
                    "dc");

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
                file: "dc");

            return -1;
        }

        string[] files = Directory.EnumerateFiles("./", "*.ds", SearchOption.AllDirectories).ToArray();
        files = files.Where(f => Path.GetDirectoryName(f).Split(Path.DirectorySeparatorChar).Last() != TemporaryBuildDirectoryName).ToArray();

        return AnalyzeFiles(files, ProjectFileDeserializer.DassieConfig, analyzerName);
    }
}