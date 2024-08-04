using Antlr4.Runtime.Tree;
using Dassie.Meta;
using Dassie.Parser;

namespace Dassie.CodeGeneration;

internal static class TreeHelpers
{
    public static bool CanBePassedByReference(IParseTree tree)
    {
        
        if (tree is not DassieParser.Full_identifier_member_access_expressionContext && tree is not DassieParser.Member_access_expressionContext)
            return false;

        object o = SymbolResolver.ResolveIdentifier(tree.GetText(), -1, -1, -1, true, false);
        return o is ParamInfo or LocalInfo or MetaFieldInfo;
    }
}