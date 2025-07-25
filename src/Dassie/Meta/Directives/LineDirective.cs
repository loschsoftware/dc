using Dassie.Extensions;

namespace Dassie.Meta.Directives;

internal class LineDirective : ICompilerDirective
{
    private static LineDirective _instance;
    public static LineDirective Instance => _instance ??= new();

    public string Identifier { get; set; } = "line";

    public object Invoke(DirectiveContext context)
    {
        if (context.Arguments == null || context.Arguments.Length == 0)
            return context.Rule.Start.Line + LineNumberOffset;

        if (context.Arguments.Length > 2 || !int.TryParse(context.Arguments[0].ToString(), out int line))
        {
            DirectiveHandler.Error(
                context.Rule.Start.Line,
                context.Rule.Start.Column,
                context.Rule.GetText().Length,
                DS0218_CompilerDirectiveInvalidArguments,
                $"Invalid arguments passed to 'line' directive. Expected [] or [int] or [int, bool].");

            return null;
        }

        bool add = false;
        if (context.Arguments.Length == 2)
            _ = bool.TryParse(context.Arguments[1].ToString(), out add);

        if (add)
            LineNumberOffset += line;
        else
            LineNumberOffset = line - context.Rule.Start.Line;

        return null;
    }
}