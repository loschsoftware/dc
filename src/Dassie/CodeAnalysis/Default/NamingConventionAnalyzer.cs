using Antlr4.Runtime.Tree;
using Dassie.Configuration;
using Dassie.Messages;
using Dassie.Parser;
using System.Collections.Generic;

namespace Dassie.CodeAnalysis.Default;

internal class NamingConventionAnalyzer : ParseTreeAnalyzer<IParseTree>
{
    private readonly CodeAnalysisConfiguration _config;
    public NamingConventionAnalyzer(DassieConfig config = null) => _config = (config ?? new()).CodeAnalysisConfiguration;

    public override List<MessageInfo> Analyze(List<IParseTree> trees)
    {
        List<MessageInfo> allMessages = [];
        
        foreach (IParseTree tree in trees)
            allMessages.AddRange(AnalyzeCompilationUnit((DassieParser.Compilation_unitContext)tree));

        return allMessages;
    }

    private List<MessageInfo> AnalyzeCompilationUnit(DassieParser.Compilation_unitContext compilationUnit)
    {
        AnalyzingListener listener = new(config: _config);
        ParseTreeWalker.Default.Walk(listener, compilationUnit);
        return listener.Messages;
    }
}