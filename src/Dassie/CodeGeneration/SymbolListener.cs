using Antlr4.Runtime.Misc;
using Dassie.Parser;

namespace Dassie.CodeGeneration;

internal class SymbolListener : DassieParserBaseListener
{
    public override void EnterType([NotNull] DassieParser.TypeContext context)
    {
        
    }
}