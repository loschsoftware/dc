using Antlr4.Runtime.Misc;
using LoschScript.Parser;
using System.Linq;

namespace LoschScript.CodeGeneration;

internal class Visitor : LoschScriptParserBaseVisitor<dynamic>
{
    public override dynamic VisitExport_directive([NotNull] LoschScriptParser.Export_directiveContext context)
    {
        base.VisitExport_directive(context);

        CurrentFile.ExportedNamespace = context.full_identifier().GetText();

        return null;
    }

    public override dynamic VisitBasic_import([NotNull] LoschScriptParser.Basic_importContext context)
    {
        base.VisitBasic_import(context);

        foreach (string ns in context.full_identifier().Select(f => f.GetText()))
        {
            if (context.Exclamation_Mark() == null)
            {
                CurrentFile.ImportedNamespaces.Add(ns);
                return null;
            }

            Context.GlobalImports.Add(ns);
        }

        return null;
    }

    public override dynamic VisitType_import([NotNull] LoschScriptParser.Type_importContext context)
    {
        base.VisitType_import(context);

        foreach (string ns in context.full_identifier().Select(f => f.GetText()))
        {
            if (context.Exclamation_Mark() == null)
            {
                CurrentFile.ImportedTypes.Add(ns);
                return null;
            }

            Context.GlobalTypeImports.Add(ns);
        }

        return null;
    }
    
    public override dynamic VisitAlias([NotNull] LoschScriptParser.AliasContext context)
    {
        base.VisitAlias(context);

        for (int i = 0; i < context.Identifier().Length; i++)
        {
            if (context.Exclamation_Mark() == null)
            {
                CurrentFile.Aliases.Add((context.full_identifier()[i].GetText(), context.Identifier()[i].GetText()));
                return null;
            }

            Context.GlobalAliases.Add((context.full_identifier()[i].GetText(), context.Identifier()[i].GetText()));
        }

        return null;
    }
}