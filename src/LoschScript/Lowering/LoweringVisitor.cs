using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using LoschScript.Parser;
using System.Collections.Generic;
using System.Text;

namespace LoschScript.Lowering;

internal class LoweringVisitor : LoschScriptParserBaseVisitor<List<IParseTree>>
{
    public StringBuilder Builder { get; } = new();

    public ICharStream CharStream { get; set; }

    public LoweringVisitor(ICharStream cs) => CharStream = cs;

    CompoundAssignmentRewriter compoundAssignmentRewriter = new();

    public string Text(ParserRuleContext rule)
    {
        return CharStream.GetText(new(rule.Start.StartIndex, rule.Stop.StopIndex));
    }
}