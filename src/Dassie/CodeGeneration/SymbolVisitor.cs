using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Dassie.Helpers;
using Dassie.Parser;
using Dassie.Text;
using Dassie.Text.Tooltips;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dassie.CodeGeneration;

internal class SymbolVisitor : DassieParserBaseVisitor<object>
{
    public override Type VisitCompilation_unit([NotNull] DassieParser.Compilation_unitContext context)
    {
        if (context.import_directive().Length > 1)
        {
            CurrentFile.FoldingRegions.Add(new()
            {
                StartLine = context.import_directive().First().Start.Line,
                StartColumn = context.import_directive().First().Start.Column,
                EndLine = context.import_directive().Last().Start.Line,
                EndColumn = context.import_directive().Last().Start.Column + context.import_directive().Last().GetText().Length
            });
        }

        foreach (IParseTree tree in context.import_directive())
            Visit(tree);

        if (context.export_directive() != null)
            Visit(context.export_directive());

        Visit(context.file_body());

        if (Context.ModuleInitializerParts.Count > 0)
        {
            MethodBuilder cctor = Context.Module.DefineGlobalMethod(".cctor", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, typeof(void), []);
            ILGenerator il = cctor.GetILGenerator();

            foreach (MethodInfo part in Context.ModuleInitializerParts)
                il.Emit(OpCodes.Call, part);

            il.Emit(OpCodes.Ret);
        }

        return typeof(void);
    }

    public override Type VisitBasic_import([NotNull] DassieParser.Basic_importContext context)
    {
        foreach (var id in context.full_identifier())
        {
            string ns = id.GetText();
            SymbolResolver.TryGetType(ns, out Type t, 0, 0, 0, true);

            if (t != null)
            {
                if (!(t.IsAbstract && t.IsSealed))
                {
                    EmitErrorMessage(
                        id.Start.Line,
                        id.Start.Column,
                        ns.Length,
                        DS0077_InvalidImport,
                        "Only namespaces and modules can be imported.");
                }

                if (context.Exclamation_Mark() != null)
                {
                    Context.GlobalTypeImports.Add(ns);
                    continue;
                }

                CurrentFile.ImportedTypes.Add(ns);
                continue;
            }

            if (context.Exclamation_Mark() == null)
            {
                CurrentFile.Imports.Add(ns);
                continue;
            }

            Context.GlobalImports.Add(ns);
        }

        return typeof(void);
    }

    public override Type VisitExport_directive([NotNull] DassieParser.Export_directiveContext context)
    {
        CurrentFile.ExportedNamespace = context.full_identifier().GetText();
        CurrentFile.Imports.Add(CurrentFile.ExportedNamespace);

        CurrentFile.Fragments.Add(new()
        {
            Color = Color.Namespace,
            Column = context.full_identifier().Start.Column,
            Line = context.full_identifier().Start.Line,
            Length = context.full_identifier().GetText().Length,
            ToolTip = TooltipGenerator.Namespace(context.full_identifier().GetText())
        });

        return typeof(void);
    }

    public override Type VisitFull_program([NotNull] DassieParser.Full_programContext context)
    {
        foreach (IParseTree type in context.type() ?? [])
            Visit(type);

        return typeof(void);
    }

    public override Type VisitType([NotNull] DassieParser.TypeContext context)
    {
        TypeDeclarationGeneration.GenerateType(context, null);

        if (context.type_block() != null && context.type_block().type_member() != null)
        {
            foreach (DassieParser.Type_memberContext member in context.type_block().type_member())
                VisitType_member(member);
        }

        return typeof(void);
    }

    // TODO
    public override object VisitType_member([NotNull] DassieParser.Type_memberContext context)
    {
        return typeof(void);
    }
}