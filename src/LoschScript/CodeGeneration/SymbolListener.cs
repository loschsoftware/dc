using Antlr4.Runtime.Misc;
using LoschScript.Parser;

namespace LoschScript.CodeGeneration;

internal class SymbolListener : LoschScriptParserBaseListener
{
    public override void EnterType([NotNull] LoschScriptParser.TypeContext context)
    {
        
    }
}