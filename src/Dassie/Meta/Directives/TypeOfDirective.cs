using Dassie.Extensions;

namespace Dassie.Meta.Directives;

internal class TypeOfDirective : ICompilerDirective
{
    private static TypeOfDirective _instance;
    public static TypeOfDirective Instance => _instance ??= new();

    public string Identifier { get; set; } = "typeof";

    public bool IgnoreArgumentTypes => true;

    public object Invoke(DirectiveContext context)
    {
        if (context.Arguments == null || context.Arguments.Length != 1)
        {
            DirectiveHandler.Error(
                context.Rule.Start.Line,
                context.Rule.Start.Column,
                context.Rule.GetText().Length,
                DS0219_CompilerDirectiveInvalidArguments,
                $"Invalid arguments passed to 'typeof' directive. Expected 1 argument.");

            return null;
        }

        string typeName = context.Arguments[0].ToString();
        return SymbolResolver.ResolveTypeName(
            typeName,
            context.Rule.Start.Line,
            context.Rule.Start.Column,
            context.Rule.GetText().Length).RawType;
    }
}