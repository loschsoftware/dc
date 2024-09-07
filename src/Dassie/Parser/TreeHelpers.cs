using Antlr4.Runtime.Tree;
using Dassie.CodeGeneration;
using Dassie.Meta;
using System;

namespace Dassie.Parser;

internal static class TreeHelpers
{
    public static bool CanBePassedByReference(IParseTree tree)
    {

        if (tree is not DassieParser.Full_identifier_member_access_expressionContext && tree is not DassieParser.Member_access_expressionContext)
            return false;

        object o = SymbolResolver.ResolveIdentifier(tree.GetText(), -1, -1, -1, true, false);
        return o is ParamInfo or LocalInfo or MetaFieldInfo;
    }

    public static bool IsType<T>(IParseTree tree)
    {
        if (tree.GetType() == typeof(T))
            return true;

        if (tree.ChildCount == 0)
            return tree.GetType() == typeof(T);

        return IsType<T>(tree.GetChild(0));
    }

    public static T GetChildOfType<T>(IParseTree tree)
    {
        if (tree.ChildCount == 0)
        {
            if (tree.GetType() != typeof(T))
                throw new InvalidOperationException();

            return (T)tree;
        }

        return GetChildOfType<T>(tree.GetChild(0));
    }
}