using Dassie.CodeGeneration.Helpers;
using Dassie.Errors;
using Dassie.Extensions;
using Dassie.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dassie.Meta.Directives;

internal static class DirectiveHandler
{
    public static void Error(ErrorInfo error)
    {
        if (!(string.IsNullOrEmpty(Context.CodeSource) || Context.CodeSource.Equals("user", StringComparison.InvariantCultureIgnoreCase)))
            return;

        EmitGeneric(error);
    }

    public static void Error(int row, int col, int length, ErrorKind error, string message, string file = null)
    {
        Error(new()
        {
            CodePosition = (row, col),
            Length = length,
            Severity = Severity.Error,
            ErrorCode = error,
            ErrorMessage = message,
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
                DS0217_InvalidCompilerDirective,
                $"The compiler directive '{identifier}' could not be found.");

            return null;
        }
        
        DirectiveContext dc = new()
        {
            Arguments = (context.expression() ?? []).Select(e => ExpressionEvaluator.Instance.Visit(e).Value).ToArray(),
            DocumentName = CurrentFile.Path,
            LineNumber = context.Start.Line,
            Rule = context
        };

        IEnumerable<ICompilerDirective> dirs = ExtensionLoader.CompilerDirectives.Where(c => c.Identifier == identifier);
        object ret = null;

        foreach (ICompilerDirective dir in dirs)
            ret = dir.Invoke(dc);

        return ret;
    }
}