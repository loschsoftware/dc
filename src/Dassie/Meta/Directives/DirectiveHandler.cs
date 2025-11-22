using Dassie.CodeGeneration.Helpers;
using Dassie.Extensions;
using Dassie.Messages;
using Dassie.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dassie.Meta.Directives;

internal static class DirectiveHandler
{
    public static void Error(MessageInfo error)
    {
        if (!(string.IsNullOrEmpty(Context.CodeSource) || Context.CodeSource.Equals("user", StringComparison.InvariantCultureIgnoreCase)))
            return;

        Emit(error);
    }

    public static void Error(int row, int col, int length, MessageCode error, string message, string file = null)
    {
        Error(new()
        {
            Location = (row, col),
            Length = length,
            Severity = Severity.Error,
            Code = error,
            Text = message,
            File = file ?? Path.GetFileName(CurrentFile.Path)
        });
    }

    public static object HandleCompilerDirective(DassieParser.Special_symbolContext context)
    {
        string identifier;

        if (context.Identifier() != null)
            identifier = SymbolResolver.GetIdentifier(context.Identifier());
        else
            identifier = "import";

        if (!ExtensionLoader.CompilerDirectives.Any(c => c.Identifier == identifier))
        {
            Error(
                context.Start.Line,
                context.Start.Column,
                context.GetText().Length,
                DS0218_InvalidCompilerDirective,
                $"The compiler directive '{identifier}' could not be resolved.");

            return null;
        }

        IEnumerable<ICompilerDirective> dirs = ExtensionLoader.CompilerDirectives.Where(c => c.Identifier == identifier);
        object ret = null;

        foreach (ICompilerDirective dir in dirs)
        {
            DassieParser.ExpressionContext[] args = context.expression() ?? [];

            DirectiveContext dc = new()
            {
                Arguments = dir.IgnoreArgumentTypes ? args.Select(a => a.GetText()).ToArray() : args.Select(e => ExpressionEvaluator.Instance.Visit(e).Value).ToArray(),
                DocumentName = CurrentFile.Path,
                LineNumber = context.Start.Line,
                Rule = context
            };

            ret = dir.Invoke(dc);
        }

        return ret;
    }
}