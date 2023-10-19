using Antlr4.Runtime.Misc;
using LoschScript.CLI;
using LoschScript.Meta;
using LoschScript.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace LoschScript.CodeGeneration;

internal class SymbolListener : LoschScriptParserBaseListener
{
    public override void EnterType([NotNull] LoschScriptParser.TypeContext context)
    {
        if (context.Identifier().GetText().Length + (CurrentFile.ExportedNamespace ?? "").Length > 1024)
        {
            EmitErrorMessage(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetText().Length,
                LS0073_TypeNameTooLong,
                "A type name cannot be longer than 1024 characters.");

            return;
        }

        Type parent = typeof(object);
        List<Type> interfaces = new();

        if (context.type_kind().Val() != null)
            parent = typeof(ValueType);

        if (context.inheritance_list() != null)
        {
            List<Type> inherited = Helpers.GetInheritedTypes(context.inheritance_list());

            foreach (Type type in inherited)
            {
                if (type.IsClass)
                    parent = type;
                else
                    interfaces.Add(type);
            }
        }

        TypeBuilder tb;

        if (context.Parent is not LoschScriptParser.Type_blockContext)
        {
            tb = Context.Module.DefineType(
                $"{(string.IsNullOrEmpty(CurrentFile.ExportedNamespace) ? "" : $"{CurrentFile.ExportedNamespace}.")}{context.Identifier().GetText()}",
                Helpers.GetTypeAttributes(context.type_kind(), context.type_access_modifier(), context.nested_type_access_modifier(), context.type_special_modifier(), false),
                parent);
        }
        else
        {
            TypeBuilder enclosingType = null;

            var enclosingTypeTree = (context.Parent as LoschScriptParser.Type_blockContext).Parent as LoschScriptParser.TypeContext;

            if (Context.Types.Select(t => t.Builder).Any(t => t.Name == enclosingTypeTree.Identifier().GetText()))
                enclosingType = Context.Types.Select(t => t.Builder).First(t => t.Name == enclosingTypeTree.Identifier().GetText());

            tb = enclosingType.DefineNestedType(
                context.Identifier().GetText(),
                Helpers.GetTypeAttributes(context.type_kind(), context.type_access_modifier(), context.nested_type_access_modifier(), context.type_special_modifier(), true),
                parent);
        }

        foreach (Type _interface in interfaces)
            tb.AddInterfaceImplementation(_interface);

        TypeContext tc = new()
        {
            Builder = tb
        };

        tc.FilesWhereDefined.Add(CurrentFile.Path);

        Context.Types.Add(tc);
    }
}