using Antlr4.Runtime.Tree;
using LoschScript.Parser;
using System.Collections.Generic;
using System.Text;

namespace LoschScript.Lowering;

internal class LoweringVisitor : LoschScriptParserBaseVisitor<List<IParseTree>>
{
    public StringBuilder Builder { get; } = new();

    CompoundAssignmentRewriter compoundAssignmentRewriter = new();
}