using Dassie.Extensions;

namespace Dassie.Meta.Directives;

internal class LineDirective : ICompilerDirective
{
    private static LineDirective _instance;
    public static LineDirective Instance => _instance ??= new();

    public string Identifier { get; set; } = "line";

    public void Invoke(DirectiveContext context)
    {
        if (context.Arguments == null || context.Arguments.Length < 1 || context.Arguments.Length > 1 || !int.TryParse(context.Arguments[0], out int line))
        {
            DirectiveHandler.Error(
                context.Rule.Start.Line,
                context.Rule.Start.Column,
                context.Rule.GetText().Length,
                DS0218_CompilerDirectiveInvalidArguments,
                $"Invalid arguments passed to 'line' directive. Expected [int].");

            return;
        }

        LineNumberOffset = line - context.Rule.Start.Line;
    }
}