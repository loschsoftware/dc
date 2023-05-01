using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using LoschScript.CLI;
using LoschScript.Meta;
using LoschScript.Parser;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LoschScript.CodeGeneration;

internal class Visitor : LoschScriptParserBaseVisitor<Type>
{
    public override Type VisitCompilation_unit([NotNull] LoschScriptParser.Compilation_unitContext context)
    {
        foreach (IParseTree tree in context.import_directive())
            Visit(tree);

        if (context.export_directive() != null)
            Visit(context.export_directive());

        Visit(context.file_body());

        return typeof(void);
    }

    public override Type VisitFile_body([NotNull] LoschScriptParser.File_bodyContext context)
    {
        if (context.top_level_statements() != null)
        {
            Visit(context.top_level_statements());
            return typeof(void);
        }

        Visit(context.full_program());

        return typeof(void);
    }

    public override Type VisitFull_program([NotNull] LoschScriptParser.Full_programContext context)
    {
        foreach (IParseTree type in context.type_definition())
            Visit(type);

        return typeof(void);
    }

    public override Type VisitType_definition([NotNull] LoschScriptParser.Type_definitionContext context)
    {
        return typeof(void);
    }

    public override Type VisitBasic_import([NotNull] LoschScriptParser.Basic_importContext context)
    {
        foreach (string ns in context.full_identifier().Select(f => f.GetText()))
        {
            if (context.Exclamation_Mark() == null)
            {
                CurrentFile.Imports.Add(ns);
                return typeof(void);
            }

            Context.GlobalImports.Add(ns);
        }

        return typeof(void);
    }

    public override Type VisitType_import([NotNull] LoschScriptParser.Type_importContext context)
    {
        foreach (string ns in context.full_identifier().Select(f => f.GetText()))
        {
            if (context.Exclamation_Mark() == null)
            {
                CurrentFile.ImportedTypes.Add(ns);
                return typeof(void);
            }

            Context.GlobalTypeImports.Add(ns);
        }

        return typeof(void);
    }

    public override Type VisitAlias([NotNull] LoschScriptParser.AliasContext context)
    {
        for (int i = 0; i < context.Identifier().Length; i++)
        {
            if (context.Exclamation_Mark() == null)
            {
                CurrentFile.Aliases.Add((context.full_identifier()[i].GetText(), context.Identifier()[i].GetText()));
                return typeof(void);
            }

            Context.GlobalAliases.Add((context.full_identifier()[i].GetText(), context.Identifier()[i].GetText()));
        }

        return typeof(void);
    }

    public override Type VisitExport_directive([NotNull] LoschScriptParser.Export_directiveContext context)
    {
        CurrentFile.ExportedNamespace = context.full_identifier().GetText();

        return typeof(void);
    }

    public override Type VisitTop_level_statements([NotNull] LoschScriptParser.Top_level_statementsContext context)
    {
        TypeBuilder tb = Context.Module.DefineType($"{(string.IsNullOrEmpty(CurrentFile.ExportedNamespace) ? "" : $"{CurrentFile.ExportedNamespace}.")}Program");

        TypeContext tc = new()
        {
            Builder = tb
        };

        tc.FilesWhereDefined.Add(CurrentFile.Path);

        MethodBuilder mb = tb.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(void), new Type[] { typeof(string[]) });
        ILGenerator il = mb.GetILGenerator();
        MethodContext mc = new()
        {
            Builder = mb,
            IL = il
        };

        mc.FilesWhereDefined.Add(CurrentFile.Path);

        tc.Methods.Add(mc);

        Context.Types.Add(tc);

        foreach (IParseTree tree in context.expression().Take(context.expression().Length - 1))
            Visit(tree);

        // Last expression is like return statement
        Type ret = Visit(context.expression().Last());

        CurrentMethod.IL.Emit(OpCodes.Ret);
        Context.Assembly.SetEntryPoint(mb);

        tb.CreateType();
        return ret;
    }

    public override Type VisitMultiply_expression([NotNull] LoschScriptParser.Multiply_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Visit(context.expression()[1]);

        CurrentMethod.IL.Emit(OpCodes.Mul);

        return t;
    }

    public override Type VisitDivide_expression([NotNull] LoschScriptParser.Divide_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Visit(context.expression()[1]);

        CurrentMethod.IL.Emit(OpCodes.Div);

        return t;
    }

    public override Type VisitAddition_expression([NotNull] LoschScriptParser.Addition_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Visit(context.expression()[1]);

        CurrentMethod.IL.Emit(OpCodes.Add);

        return t;
    }

    public override Type VisitSubtraction_expression([NotNull] LoschScriptParser.Subtraction_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Visit(context.expression()[1]);

        CurrentMethod.IL.Emit(OpCodes.Sub);

        return t;
    }

    public override Type VisitRemainder_expression([NotNull] LoschScriptParser.Remainder_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Visit(context.expression()[1]);

        CurrentMethod.IL.Emit(OpCodes.Rem);

        return t;
    }

    public override Type VisitPower_expression([NotNull] LoschScriptParser.Power_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        MethodInfo m = typeof(Math)
            .GetMethod("Pow", new Type[]
            {
                t,
                t2
            });

        if (m == null)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                LS0036_ArithmeticError,
                $"The power operation is not supported by the types '{t.Name}' and '{t2.Name}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, m, null);

        return t;
    }

    public override Type VisitMember_access_expression([NotNull] LoschScriptParser.Member_access_expressionContext context)
    {
        Type t = Visit(context.expression());

        if (context.arglist() != null)
        {
            Visit(context.arglist());

            MethodInfo m = null;

            MethodInfo[] methods = t.GetMethods().Where(m => m.Name == context.Identifier().GetText()).ToArray();

            foreach (MethodInfo possible in methods)
            {
                if (possible.GetParameters().Length != CurrentMethod.ArgumentTypesForNextMethodCall.Count)
                    continue;

                for (int i = 0; i < possible.GetParameters().Length; i++)
                {
                    if (possible.GetParameters()[i].ParameterType != CurrentMethod.ArgumentTypesForNextMethodCall[i])
                        continue;
                }

                m = possible;
            }

            CurrentMethod.ArgumentTypesForNextMethodCall.Clear();

            if (m != null)
            {
                CurrentMethod.IL.EmitCall(OpCodes.Call, m, null);
                return m.ReturnType;
            }
            else
            {
                EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    LS0002_MethodNotFound,
                    $"The type \"{t.Name}\" does not contain a function called \"{context.Identifier().GetText()}\" with the specified argument types.");

                return typeof(void);
            }
        }

        FieldInfo f = t.GetField(context.Identifier().GetText());
        if (f != null)
        {
            CurrentMethod.IL.Emit(OpCodes.Ldfld, f);
            return f.FieldType;
        }
        else
        {
            EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    LS0002_MethodNotFound,
                    $"The type \"{t.Name}\" does not contain a field called \"{context.Identifier().GetText()}\".");

            return typeof(void);
        }
    }

    public override Type VisitArglist([NotNull] LoschScriptParser.ArglistContext context)
    {
        foreach (IParseTree tree in context.expression())
            CurrentMethod.ArgumentTypesForNextMethodCall.Add(Visit(tree));

        return typeof(void);
    }

    public override Type VisitIdentifier_atom([NotNull] LoschScriptParser.Identifier_atomContext context)
    {
        return Helpers.ResolveTypeName(context.Identifier().GetText());
    }

    public override Type VisitFull_identifier([NotNull] LoschScriptParser.Full_identifierContext context)
    {
        return Helpers.ResolveTypeName(context.GetText());
    }

    public override Type VisitInteger_atom([NotNull] LoschScriptParser.Integer_atomContext context)
    {
        string text = context.GetText();

        if (text.EndsWith("sb"))
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4, sbyte.Parse(text[0..^2].Replace("'", "")));
            return typeof(sbyte);
        }

        if (text.EndsWith("b"))
        {
            text += "0";

            CurrentMethod.IL.Emit(OpCodes.Ldc_I4, byte.Parse(text[0..^2].Replace("'", "")));
            return typeof(byte);
        }

        if (text.EndsWith("us"))
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4, ushort.Parse(text[0..^2].Replace("'", "")));
            return typeof(ushort);
        }

        if (text.EndsWith("s"))
        {
            text += "0";

            CurrentMethod.IL.Emit(OpCodes.Ldc_I4, short.Parse(text[0..^2].Replace("'", "")));
            return typeof(short);
        }

        if (text.EndsWith("ul"))
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_I8, ulong.Parse(text[0..^2].Replace("'", "")));
            return typeof(ulong);
        }

        if (text.EndsWith("u"))
        {
            text += "0";

            CurrentMethod.IL.Emit(OpCodes.Ldc_I4, uint.Parse(text[0..^2].Replace("'", "")));
            return typeof(uint);
        }

        if (text.EndsWith("l"))
        {
            text += "0";

            CurrentMethod.IL.Emit(OpCodes.Ldc_I8, long.Parse(text[0..^2].Replace("'", "")));
            return typeof(long);
        }

        if (text.EndsWith("un"))
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4, int.Parse(text[0..^2].Replace("'", "")));
            return typeof(nuint);
        }

        if (text.EndsWith("n"))
        {
            text += "0";

            CurrentMethod.IL.Emit(OpCodes.Ldc_I4, int.Parse(text[0..^2].Replace("'", "")));
            return typeof(nint);
        }

        text += "00";

        CurrentMethod.IL.Emit(OpCodes.Ldc_I4, int.Parse(text[0..^2].Replace("'", "")));
        return typeof(int);
    }

    public override Type VisitString_atom([NotNull] LoschScriptParser.String_atomContext context)
    {
        if (context.Verbatim_String_Literal() != null)
        {
            CurrentMethod.IL.Emit(OpCodes.Ldstr, context.GetText()[2..^1]);
            return typeof(string);
        }

        string rawText = context.GetText()[1..^1]
            .Replace("^'", "'")
            .Replace("^\"", "\"")
            .Replace("^^", "^")
            .Replace("^0", "\0")
            .Replace("^a", "\a")
            .Replace("^b", "\b")
            .Replace("^f", "\f")
            .Replace("^n", "\n")
            .Replace("^r", "\r")
            .Replace("^t", "\t")
            .Replace("^v", "\v");

        // TODO: Handle Hex and Unicode escape sequences

        CurrentMethod.IL.Emit(OpCodes.Ldstr, rawText);

        return typeof(string);
    }

    public override Type VisitCharacter_atom([NotNull] LoschScriptParser.Character_atomContext context)
    {
        char rawChar = char.Parse(context.GetText()
            .Replace("^'", "'")
            .Replace("^\"", "\"")
            .Replace("^^", "^")
            .Replace("^0", "\0")
            .Replace("^a", "\a")
            .Replace("^b", "\b")
            .Replace("^f", "\f")
            .Replace("^n", "\n")
            .Replace("^r", "\r")
            .Replace("^t", "\t")
            .Replace("^v", "\v")[1..^1]);

        CurrentMethod.IL.Emit(OpCodes.Ldc_I4, rawChar);

        return typeof(char);
    }

    public override Type VisitBoolean_atom([NotNull] LoschScriptParser.Boolean_atomContext context)
    {
        if (context.True() != null)
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4_1);
            return typeof(bool);
        }

        CurrentMethod.IL.Emit(OpCodes.Ldc_I4_0);

        return typeof(bool);
    }
}