using Dassie.Extensions;

namespace Dassie.Meta.Directives;

internal class IncludeDirective : ICompilerDirective
{
    private static IncludeDirective _instance;
    public static IncludeDirective Instance => _instance ??= new();

    public string Identifier => "include";

    public object Invoke(DirectiveContext context)
    {
        if (context?.Arguments.Length != 1)
        {
            DirectiveHandler.Error(
                context.Rule.Start.Line,
                context.Rule.Start.Column,
                context.Rule.GetText().Length,
                DS0219_CompilerDirectiveInvalidArguments,
                StringHelper.IncludeDirective_InvalidArguments);

            return null;
        }

        string uri = context.Arguments[0].ToString();
        // TODO: Implement ${include} directive, supporting both file paths and URLs
        return null;
    }
}