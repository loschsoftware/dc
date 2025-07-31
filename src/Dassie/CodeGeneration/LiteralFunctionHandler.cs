using System.Reflection;

namespace Dassie.CodeGeneration;

internal static class LiteralFunctionHandler
{
    public static void Evaluate(MethodInfo context, object[] args)
    {
        object ret = context.Invoke(null, args);
        EmitConst(ret);
    }
}