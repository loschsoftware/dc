using Dassie.Extensions;

namespace Dassie.Meta.Directives;

internal class ILDirective : ICompilerDirective
{
    private static ILDirective _instance;
    public static ILDirective Instance => _instance ??= new();

    public string Identifier { get; set; } = "il";

    public object Invoke(DirectiveContext context)
    {
        if (context.Arguments == null || context.Arguments.Length < 1 || context.Arguments.Length > 1)
        {
            DirectiveHandler.Error(
                context.Rule.Start.Line,
                context.Rule.Start.Column,
                context.Rule.GetText().Length,
                DS0219_CompilerDirectiveInvalidArguments,
                "Invalid arguments passed to 'il' directive. Expected [string].");

            return null;
        }

        if (CurrentMethod == null)
        {
            DirectiveHandler.Error(
                context.Rule.Start.Line,
                context.Rule.Start.Column,
                context.Rule.GetText().Length,
                DS0220_CompilerDirectiveInvalidScope,
                "The 'il' compiler directive can only be used inside of a function.");

            return null;
        }

        string arg = context.Arguments[0].ToString();
        EmitInlineIL(arg, context.Rule.Start.Line, context.Rule.Start.Column + 1, context.Rule.GetText().Length);

        return null;
    }
}