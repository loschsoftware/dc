using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using LoschScript.CLI;
using LoschScript.Meta;
using LoschScript.Parser;
using System;
using System.Globalization;
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
                continue;
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
                continue;
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
                continue;
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

    public override Type VisitExpression_atom([NotNull] LoschScriptParser.Expression_atomContext context)
    {
        return Visit(context.expression());
    }

    public override Type VisitUnary_negation_expression([NotNull] LoschScriptParser.Unary_negation_expressionContext context)
    {
        Type t = Visit(context.expression());

        if (Helpers.IsNumericType(t))
        {
            CurrentMethod.IL.Emit(OpCodes.Neg);
            return t;
        }

        MethodInfo op = t.GetMethod("op_UnaryNegation", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t }, null);

        if (op == null)
        {
            EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    LS0036_ArithmeticError,
                    $"The type '{t.Name}' does not implement the unary negation operation.",
                    Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Callvirt, op, null);

        return t;
    }

    public override Type VisitUnary_plus_expression([NotNull] LoschScriptParser.Unary_plus_expressionContext context)
    {
        Type t = Visit(context.expression());

        if (Helpers.IsNumericType(t))
            return t;

        MethodInfo op = t.GetMethod("op_UnaryPlus", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t }, null);

        if (op == null)
        {
            EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    LS0036_ArithmeticError,
                    $"The type '{t.Name}' does not implement the unary plus operation.",
                    Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Callvirt, op, null);

        return t;
    }

    public override Type VisitLogical_negation_expression([NotNull] LoschScriptParser.Logical_negation_expressionContext context)
    {
        Type t = Visit(context.expression());

        if (t == typeof(bool))
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4_0);
            CurrentMethod.IL.Emit(OpCodes.Ceq);

            return t;
        }

        MethodInfo op = t.GetMethod("op_LogicalNot", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement the logical negation operation.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Callvirt, op, null);

        return t;
    }

    public override Type VisitLogical_and_expression([NotNull] LoschScriptParser.Logical_and_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (t == typeof(bool) && t2 == typeof(bool))
        {
            CurrentMethod.IL.Emit(OpCodes.And);
            return t;
        }

        EmitErrorMessage(
            context.Start.Line,
            context.Start.Column,
            LS0002_MethodNotFound,
            $"The logical and operation is only supported by boolean types.",
            Path.GetFileName(CurrentFile.Path));

        return t;
    }

    public override Type VisitLogical_or_expression([NotNull] LoschScriptParser.Logical_or_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (t == typeof(bool) && t2 == typeof(bool))
        {
            CurrentMethod.IL.Emit(OpCodes.Or);
            return t;
        }

        EmitErrorMessage(
            context.Start.Line,
            context.Start.Column,
            LS0002_MethodNotFound,
            $"The logical or operation is only supported by boolean types.",
            Path.GetFileName(CurrentFile.Path));

        return t;
    }

    public override Type VisitOr_expression([NotNull] LoschScriptParser.Or_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsNumericType(t) || t == typeof(bool))
        {
            CurrentMethod.IL.Emit(OpCodes.Or);
            return t;
        }

        MethodInfo op = t.GetMethod("op_BitwiseOr", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a bitwise or operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Callvirt, op, null);

        return t;
    }

    public override Type VisitAnd_expression([NotNull] LoschScriptParser.And_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsNumericType(t) || t == typeof(bool))
        {
            CurrentMethod.IL.Emit(OpCodes.And);
            return t;
        }

        MethodInfo op = t.GetMethod("op_BitwiseAnd", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a bitwise and operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Callvirt, op, null);

        return t;
    }

    public override Type VisitXor_expression([NotNull] LoschScriptParser.Xor_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsNumericType(t))
        {
            CurrentMethod.IL.Emit(OpCodes.Xor);
            return t;
        }

        MethodInfo op = t.GetMethod("op_ExclusiveOr", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement an exclusive or operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Callvirt, op, null);

        return t;
    }

    public override Type VisitBitwise_complement_expression([NotNull] LoschScriptParser.Bitwise_complement_expressionContext context)
    {
        Type t = Visit(context.expression());

        if (Helpers.IsNumericType(t))
        {
            CurrentMethod.IL.Emit(OpCodes.Not);
            return t;
        }

        MethodInfo op = t.GetMethod("op_OnesComplement", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t}, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a complement operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Callvirt, op, null);

        return t;
    }

    public override Type VisitMultiply_expression([NotNull] LoschScriptParser.Multiply_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsNumericType(t))
        {
            CurrentMethod.IL.Emit(OpCodes.Mul);
            return t;
        }

        MethodInfo op = t.GetMethod("op_Multiply", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a multiplication operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Callvirt, op, null);

        return t;
    }

    public override Type VisitDivide_expression([NotNull] LoschScriptParser.Divide_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsNumericType(t))
        {
            CurrentMethod.IL.Emit(OpCodes.Div);
            return t;
        }

        MethodInfo op = t.GetMethod("op_Division", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a division operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Callvirt, op, null);

        return t;
    }

    public override Type VisitAddition_expression([NotNull] LoschScriptParser.Addition_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsNumericType(t) && Helpers.IsNumericType(t2))
        {
            CurrentMethod.IL.Emit(OpCodes.Add);
            return t;
        }

        if (t == typeof(string) || t2 == typeof(string))
        {
            if (t2 != typeof(string))
            {
                CurrentMethod.IL.DeclareLocal(t2);
                CurrentMethod.IL.Emit(OpCodes.Stloc, ++CurrentMethod.LocalIndex);
                CurrentMethod.IL.Emit(OpCodes.Ldloca, CurrentMethod.LocalIndex);

                MethodInfo toString = t2.GetMethod("ToString", Array.Empty<Type>());
                CurrentMethod.IL.EmitCall(Helpers.GetCallOpCode(t2), toString, null);
            }
            else
            {
                // TODO: Fix this mess ASAP

                CurrentMethod.IL.Emit(OpCodes.Pop);

                CurrentMethod.IL.DeclareLocal(t);
                CurrentMethod.IL.Emit(OpCodes.Stloc, ++CurrentMethod.LocalIndex);
                CurrentMethod.IL.Emit(OpCodes.Ldloca, CurrentMethod.LocalIndex);

                MethodInfo toString = t.GetMethod("ToString", Array.Empty<Type>());
                CurrentMethod.IL.EmitCall(Helpers.GetCallOpCode(t), toString, null);

                Visit(context.expression()[1]);
            }

            MethodInfo concat = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
            CurrentMethod.IL.EmitCall(OpCodes.Call, concat, null);

            return typeof(string);
        }

        MethodInfo op = t.GetMethod("op_Addition", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement an addition operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Callvirt, op, null);

        return t;
    }

    public override Type VisitSubtraction_expression([NotNull] LoschScriptParser.Subtraction_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsNumericType(t))
        {
            CurrentMethod.IL.Emit(OpCodes.Sub);
            return t;
        }

        MethodInfo op = t.GetMethod("op_Subtraction", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a subtraction operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Callvirt, op, null);

        return t;
    }

    public override Type VisitRemainder_expression([NotNull] LoschScriptParser.Remainder_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsNumericType(t))
        {
            CurrentMethod.IL.Emit(OpCodes.Rem);
            return t;
        }

        MethodInfo op = t.GetMethod("op_Modulus", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a remainder operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Callvirt, op, null);

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

    public override Type VisitTypeof_expression([NotNull] LoschScriptParser.Typeof_expressionContext context)
    {
        Type t = Helpers.ResolveTypeName(context.Identifier().ToString());
        CurrentMethod.IL.Emit(OpCodes.Ldtoken, t);
        
        MethodInfo typeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });
        CurrentMethod.IL.EmitCall(OpCodes.Call, typeFromHandle, null);

        return typeof(Type);
    }

    public override Type VisitMember_access_expression([NotNull] LoschScriptParser.Member_access_expressionContext context)
    {
        Type t = Visit(context.expression());

        if (context.arglist() != null)
        {
            Visit(context.arglist());

            MethodInfo m = null;

            MethodInfo[] methods = t.GetMethods().Where(m => m.Name == context.Identifier().GetText()).ToArray();

            bool success = false;

            foreach (MethodInfo possible in methods)
            {
                if (possible.GetParameters().Length != CurrentMethod.ArgumentTypesForNextMethodCall.Count)
                    continue;

                string[] _params = possible.GetParameters().Select(p => p.ParameterType.FullName).ToArray();
                string[] _params2 = CurrentMethod.ArgumentTypesForNextMethodCall.Select(t => t.FullName).ToArray();

                for (int i = 0; i < _params.Length; i++)
                {
                    if (_params[i] != _params2[i])
                        continue;

                    success = true;
                }

                if (success)
                {
                    m = possible;
                    break;
                }
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

        MethodInfo parameterlessFunc = t.GetMethod(context.Identifier().GetText(), Array.Empty<Type>());
        if (parameterlessFunc != null)
        {
            CurrentMethod.IL.EmitCall(OpCodes.Call, parameterlessFunc, null);
            return parameterlessFunc.ReturnType;
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

    public override Type VisitReal_atom([NotNull] LoschScriptParser.Real_atomContext context)
    {
        string text = context.GetText();

        if (text.EndsWith("s"))
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_R4, float.Parse(text[0..^1].Replace("'", ""), CultureInfo.GetCultureInfo("en-US")));
            return typeof(float);
        }

        if (text.EndsWith("d"))
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_R8, double.Parse(text[0..^1].Replace("'", ""), CultureInfo.GetCultureInfo("en-US")));
            return typeof(double);
        }

        if (text.EndsWith("m"))
        {
            // TODO: Apparently decimals are a pain in the ass... For now we'll cheat and emit doubles instead
            CurrentMethod.IL.Emit(OpCodes.Ldc_R8, double.Parse(text[0..^1].Replace("'", ""), CultureInfo.GetCultureInfo("en-US")));
            return typeof(double);
        }

        CurrentMethod.IL.Emit(OpCodes.Ldc_R8, double.Parse(text.Replace("'", ""), CultureInfo.GetCultureInfo("en-US")));
        return typeof(double);
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