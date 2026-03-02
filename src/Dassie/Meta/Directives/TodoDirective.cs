using Dassie.Extensions;

namespace Dassie.Meta.Directives;

internal class TodoDirective : ICompilerDirective
{
    private static TodoDirective _instance;
    public static TodoDirective Instance => _instance ??= new();

    public string Identifier { get; set; } = "todo";

    public object Invoke(DirectiveContext context)
    {
        if (context.Arguments == null || context.Arguments.Length < 1 || context.Arguments.Length > 1)
        {
            DirectiveHandler.Error(
                context.Rule.Start.Line,
                context.Rule.Start.Column,
                context.Rule.GetText().Length,
                DS0219_CompilerDirectiveInvalidArguments,
                StringHelper.TodoDirective_InvalidArguments);

            return null;
        }

        if (CurrentMethod == null)
        {
            DirectiveHandler.Error(
                context.Rule.Start.Line,
                context.Rule.Start.Column,
                context.Rule.GetText().Length,
                DS0220_CompilerDirectiveInvalidScope,
                StringHelper.TodoDirective_InvalidScope);

            return null;
        }

        string todoDesc = context.Arguments[0].ToString().Trim('"');
        string todoMsg = StringHelper.Format(nameof(StringHelper.TodoDirective_Message), Path.GetFileName(CurrentFile.Path), context.Rule.Start.Line + LineNumberOffset, todoDesc);
        CurrentMethod.IL.EmitWriteLine(todoMsg);
        return null;
    }
}