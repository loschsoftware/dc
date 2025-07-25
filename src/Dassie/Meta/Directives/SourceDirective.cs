using Dassie.Extensions;

namespace Dassie.Meta.Directives;

internal class SourceDirective : ICompilerDirective
{
    private static SourceDirective _instance;
    public static SourceDirective Instance => _instance ??= new();

    public string Identifier { get; set; } = "source";

    public object Invoke(DirectiveContext context)
    {
        if (context.Arguments == null || context.Arguments.Length < 1 || context.Arguments.Length > 1)
        {
            DirectiveHandler.Error(
                context.Rule.Start.Line,
                context.Rule.Start.Column,
                context.Rule.GetText().Length,
                DS0218_CompilerDirectiveInvalidArguments,
                $"Invalid arguments passed to 'source' directive. Expected [string].");

            return null;
        }

        Context.CodeSource = context.Arguments[0];
        return null;
    }
}