using Dassie.Extensions;
using System.Collections.Generic;

namespace Dassie.Meta.Directives;

internal class ImportDirective : ICompilerDirective
{
    private static ImportDirective _instance;
    public static ImportDirective Instance => _instance ??= new();

    public string Identifier { get; set; } = "import";

    public object Invoke(DirectiveContext context)
    {
        if (context.Arguments == null || context.Arguments.Length < 1 || context.Arguments.Length > 3)
        {
            DirectiveHandler.Error(
                context.Rule.Start.Line,
                context.Rule.Start.Column,
                context.Rule.GetText().Length,
                DS0219_CompilerDirectiveInvalidArguments,
                "Invalid arguments passed to 'import' directive. Expected between 1 and 3 arguments.");

            return null;
        }

        bool negate = false;
        bool global = false;
        string importee;

        if (context.Arguments.Length == 3)
        {
            _ = bool.TryParse(context.Arguments[2].ToString(), out negate);
            context.Arguments = context.Arguments[..^1];
        }

        if (context.Arguments.Length == 1)
            importee = context.Arguments[0].ToString().Trim('"');
        else
        {
            string target = context.Arguments[0].ToString().Trim('"');
            importee = context.Arguments[1].ToString();

            if (target != "local" && target != "global")
            {
                DirectiveHandler.Error(
                    context.Rule.Start.Line,
                    context.Rule.Start.Column,
                    context.Rule.GetText().Length,
                    DS0221_ImportDirectiveInvalidTarget,
                    $"Invalid import target '{target}'. Expected 'local' or 'global'.");

                return null;
            }

            if (target == "global")
                global = true;
        }

        bool isType = SymbolResolver.ResolveTypeName(importee, noErrors: true) != null;
        Apply(importee, isType, global, negate);

        static void Apply(string importee, bool isType, bool global, bool negate)
        {
            List<string> importList = isType
                ? global ? Context.GlobalTypeImports : CurrentFile.ImportedTypes
                : global ? Context.GlobalImports : CurrentFile.Imports;

            if (negate)
                importList.RemoveAll(i => i == importee);
            else
                importList.Add(importee);
        }

        return null;
    }
}