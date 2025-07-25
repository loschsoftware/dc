using Dassie.Parser;
using System;
using System.Reflection;

namespace Dassie.Intrinsics;

// This system was replaced by compiler directives (${...}), but we might still want to
// support intrinsic functions in the future, so this stays empty for now.

internal static class IntrinsicFunctionHandler
{
    public static bool HandleSpecialFunction(string name, DassieParser.ArglistContext args, int line, int column, int length, out Type retType, out MethodInfo method)
    {
        retType = null;
        method = null;
        return false;
    }
}