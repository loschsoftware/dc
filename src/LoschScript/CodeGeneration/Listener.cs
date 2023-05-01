using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using LoschScript.Meta;
using LoschScript.Parser;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LoschScript.CodeGeneration;

internal class Listener : LoschScriptParserBaseListener
{
    public override void EnterEveryRule([NotNull] ParserRuleContext context)
    {
        base.EnterEveryRule(context);

        if (GlobalConfig.AdvancedDiagnostics)
            Console.WriteLine($"Entering rule '{context.GetType().Name}': {context.GetText()}{Environment.NewLine}");
    }

    public override void EnterTop_level_statements([NotNull] LoschScriptParser.Top_level_statementsContext context)
    {
        base.EnterTop_level_statements(context);

        TypeBuilder tb = Context.Module.DefineType($"{(string.IsNullOrEmpty(CurrentFile.ExportedNamespace) ? "" : $"{CurrentFile.ExportedNamespace}.")}Program");
        
        TypeContext tc = new()
        {
            Builder = tb
        };

        tc.FilesWhereDefined.Add(CurrentFile.Path);

        MethodBuilder mb = tb.DefineMethod("Main", MethodAttributes.Static);
        ILGenerator il = mb.GetILGenerator();
        MethodContext mc = new()
        {
            Builder = mb,
            IL = il
        };

        mc.FilesWhereDefined.Add(CurrentFile.Path);

        tc.Methods.Add(mc);

        Context.Types.Add(tc);
    }

    public override void EnterExport_directive([NotNull] LoschScriptParser.Export_directiveContext context)
    {
        base.EnterExport_directive(context);

        CurrentFile.ExportedNamespace = context.full_identifier().GetText();
    }

    public override void EnterBasic_import([NotNull] LoschScriptParser.Basic_importContext context)
    {
        base.EnterBasic_import(context);

        foreach (string ns in context.full_identifier().Select(f => f.GetText()))
        {
            if (context.Exclamation_Mark() == null)
            {
                CurrentFile.Imports.Add(ns);
                return;
            }

            Context.GlobalImports.Add(ns);
        }
    }

    public override void EnterType_import([NotNull] LoschScriptParser.Type_importContext context)
    {
        base.EnterType_import(context);

        foreach (string ns in context.full_identifier().Select(f => f.GetText()))
        {
            if (context.Exclamation_Mark() == null)
            {
                CurrentFile.ImportedTypes.Add(ns);
                return;
            }

            Context.GlobalTypeImports.Add(ns);
        }
    }

    public override void EnterAlias([NotNull] LoschScriptParser.AliasContext context)
    {
        base.EnterAlias(context);

        for (int i = 0; i < context.Identifier().Length; i++)
        {
            if (context.Exclamation_Mark() == null)
            {
                CurrentFile.Aliases.Add((context.full_identifier()[i].GetText(), context.Identifier()[i].GetText()));
                return;
            }

            Context.GlobalAliases.Add((context.full_identifier()[i].GetText(), context.Identifier()[i].GetText()));
        }
    }
}