using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using LoschScript.CLI;
using LoschScript.CompilerServices;
using LoschScript.Core;
using LoschScript.Meta;
using LoschScript.Parser;
using LoschScript.Runtime;
using LoschScript.Text;
using LoschScript.Text.Tooltips;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Windows;

namespace LoschScript.CodeGeneration;

internal class Visitor : LoschScriptParserBaseVisitor<Type>
{
    private readonly bool createAssembly = false;

    public Visitor(bool createAssembly = true)
    {
        this.createAssembly = createAssembly;
    }

    public override Type VisitCompilation_unit([NotNull] LoschScriptParser.Compilation_unitContext context)
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

        return typeof(void);
    }

    public override Type VisitFile_body([NotNull] LoschScriptParser.File_bodyContext context)
    {
        // Create throwaway assembly
        if (!createAssembly)
        {
            Context.Assembly = AssemblyBuilder.DefineDynamicAssembly(new("throwaway"), AssemblyBuilderAccess.Run);
            Context.Module = Context.Assembly.DefineDynamicModule("throwaway");
        }

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
        foreach (IParseTree type in context.type())
            Visit(type);

        return typeof(void);
    }

    public override Type VisitType([NotNull] LoschScriptParser.TypeContext context)
    {
        VisitType(context, null);
        return typeof(void);
    }

    private void VisitType(LoschScriptParser.TypeContext context, TypeBuilder enclosingType)
    {
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

        if (enclosingType == null)
        {
            tb = Context.Module.DefineType(
                $"{(string.IsNullOrEmpty(CurrentFile.ExportedNamespace) ? "" : $"{CurrentFile.ExportedNamespace}.")}{context.Identifier().GetText()}",
                Helpers.GetTypeAttributes(context.type_kind(), context.type_access_modifier(), context.nested_type_access_modifier(), context.type_special_modifier(), false),
                parent);
        }
        else
        {
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

        foreach (LoschScriptParser.TypeContext nestedType in context.type_block().type())
            VisitType(nestedType, tb);

        foreach (LoschScriptParser.Type_memberContext member in context.type_block().type_member())
            Visit(member);

        foreach (var ctor in TypeContext.Current.Constructors)
            HandleConstructor(ctor);

        if (TypeContext.Current.Constructors.Count == 0 && TypeContext.Current.FieldInitializers.Count > 0)
        {
            ConstructorBuilder cb = TypeContext.Current.Builder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.HasThis,
                Type.EmptyTypes);

            CurrentMethod = new()
            {
                ConstructorBuilder = cb,
                IL = cb.GetILGenerator()
            };

            HandleFieldInitializersAndDefaultConstructor();
            CurrentMethod.IL.Emit(OpCodes.Ret);
        }

        tb.CreateType();

        CurrentFile.Fragments.Add(new()
        {
            Color = TooltipGenerator.ColorForType(tb.CreateTypeInfo()),
            Line = context.Identifier().Symbol.Line,
            Column = context.Identifier().Symbol.Column,
            Length = context.Identifier().GetText().Length,
            ToolTip = TooltipGenerator.Type(tb.CreateTypeInfo(), true, true)
        });
    }

    private void HandleFieldInitializersAndDefaultConstructor()
    {
        foreach (var (field, value) in TypeContext.Current.FieldInitializers)
        {
            if (!field.IsStatic)
                CurrentMethod.IL.Emit(OpCodes.Ldarg_S, (byte)0);

            Type t = Visit(value);

            if (field.FieldType != t)
            {
                EmitErrorMessage(
                    value.Start.Line,
                    value.Start.Column,
                    value.GetText().Length,
                    LS0054_WrongFieldType,
                    $"Expected expression of type '{field.FieldType.FullName}', but got type '{t.FullName}'.");
            }

            if (field.IsStatic)
                CurrentMethod.IL.Emit(OpCodes.Stsfld, field);
            else
                CurrentMethod.IL.Emit(OpCodes.Stfld, field);
        }

        CurrentMethod.IL.Emit(OpCodes.Ldarg_S, (byte)0);
        CurrentMethod.IL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
    }

    public override Type VisitBlock_expression([NotNull] LoschScriptParser.Block_expressionContext context)
    {
        return Visit(context.code_block());
    }

    private void HandleConstructor(LoschScriptParser.Type_memberContext context)
    {
        CallingConventions callingConventions = CallingConventions.HasThis;

        if (context.member_special_modifier().Any(m => m.Static() != null))
            callingConventions = CallingConventions.Standard;

        var paramTypes = ResolveParameterList(context.parameter_list());

        ConstructorBuilder cb = TypeContext.Current.Builder.DefineConstructor(
        Helpers.GetMethodAttributes(context.member_access_modifier(), context.member_oop_modifier(), context.member_special_modifier()),
        callingConventions,
        paramTypes.Select(p => p.Type).ToArray());

        CurrentMethod = new()
        {
            ConstructorBuilder = cb,
            IL = cb.GetILGenerator()
        };

        CurrentMethod.FilesWhereDefined.Add(CurrentFile.Path);

        foreach (var param in paramTypes)
        {
            ParameterBuilder pb = cb.DefineParameter(
                CurrentMethod.ParameterIndex++,
                Helpers.GetParameterAttributes(param.Context.parameter_modifier(), param.Context.Equals() != null),
                param.Context.Identifier().GetText());

            CurrentMethod.Parameters.Add((param.Context.Identifier().GetText(), param.Type, pb, CurrentMethod.ParameterIndex, new()));
        }

        HandleFieldInitializersAndDefaultConstructor();

        Visit(context.expression());

        CurrentMethod.IL.Emit(OpCodes.Ret);

        List<(Type, string)> _params = new();
        foreach (var param in paramTypes)
            _params.Add((param.Type, param.Context.Identifier().GetText()));

        CurrentFile.Fragments.Add(new()
        {
            Color = TooltipGenerator.ColorForType(TypeContext.Current.Builder),
            Line = context.Identifier().Symbol.Line,
            Column = context.Identifier().Symbol.Column,
            Length = context.Identifier().GetText().Length,
            ToolTip = TooltipGenerator.Constructor(TypeContext.Current.Builder, _params)
        });
    }

    public override Type VisitType_member([NotNull] LoschScriptParser.Type_memberContext context)
    {
        if (context.Identifier().GetText() == TypeContext.Current.Builder.Name)
        {
            // Defer constructors for field initializers
            TypeContext.Current.Constructors.Add(context);

            return typeof(void);
        }

        Helpers.CreateFakeMethod();
        Type _tReturn = Visit(context.expression());

        if (context.parameter_list() != null || _tReturn == typeof(void))
        {
            Type tReturn = _tReturn; // TODO: Add proper type inference

            if (context.type_name() != null)
                tReturn = Helpers.ResolveTypeName(context.type_name());

            CallingConventions callingConventions = CallingConventions.HasThis;

            if (context.member_special_modifier().Any(m => m.Static() != null))
                callingConventions = CallingConventions.Standard;

            var paramTypes = ResolveParameterList(context.parameter_list());

            MethodBuilder mb = TypeContext.Current.Builder.DefineMethod(
                context.Identifier().GetText(),
                Helpers.GetMethodAttributes(context.member_access_modifier(), context.member_oop_modifier(), context.member_special_modifier()),
                callingConventions,
                tReturn,
                paramTypes.Select(p => p.Type).ToArray());

            CurrentMethod = new()
            {
                Builder = mb,
                IL = mb.GetILGenerator()
            };

            CurrentMethod.FilesWhereDefined.Add(CurrentFile.Path);

            foreach (var param in paramTypes)
            {
                ParameterBuilder pb = mb.DefineParameter(
                    CurrentMethod.ParameterIndex++,
                    Helpers.GetParameterAttributes(param.Context.parameter_modifier(), param.Context.Equals() != null),
                    param.Context.Identifier().GetText());

                CurrentMethod.Parameters.Add((param.Context.Identifier().GetText(), param.Type, pb, CurrentMethod.ParameterIndex, new()));
            }

            _tReturn = Visit(context.expression());

            if (_tReturn != tReturn)
            {
                EmitErrorMessage(
                    context.expression().Start.Line,
                    context.expression().Start.Column,
                    context.expression().GetText().Length,
                    LS0053_WrongReturnType,
                    $"Expected expression of type '{tReturn.FullName}', but got type '{_tReturn.FullName}'.");
            }

            CurrentMethod.IL.Emit(OpCodes.Ret);

            List<(string, Type)> _params = new();
            foreach (var param in CurrentMethod.Parameters)
                _params.Add((param.Name, param.Type));

            CurrentFile.Fragments.Add(new()
            {
                Color = Color.Function,
                Line = context.Identifier().Symbol.Line,
                Column = context.Identifier().Symbol.Column,
                Length = context.Identifier().GetText().Length,
                ToolTip = TooltipGenerator.Function(context.Identifier().GetText(), tReturn, _params.ToArray())
            });

            // TODO: Ignore "Attribute" suffix for attributes
            if (context.attribute() != null && Helpers.ResolveTypeName(context.attribute().type_name()) == typeof(EntryPointAttribute))
            {
                if (Context.EntryPointIsSet)
                {
                    EmitErrorMessage(
                        context.attribute().Start.Line,
                        context.attribute().Start.Column,
                        context.attribute().GetText().Length,
                        LS0055_MultipleEntryPoints,
                        "Only one function can be declared as an entry point.");
                }

                if (!mb.IsStatic)
                {
                    EmitErrorMessage(
                        context.Identifier().Symbol.Line,
                        context.Identifier().Symbol.Column,
                        context.Identifier().GetText().Length,
                        LS0035_EntryPointNotStatic,
                        "The application entry point must be static.");
                }

                Context.EntryPointIsSet = true;

                Context.Assembly.SetEntryPoint(mb);
            }

            return typeof(void);
        }

        Helpers.CreateFakeMethod();

        Type _type = typeof(object);

        if (context.expression() != null)
            _type = Visit(context.expression());

        Type type = _type;

        if (context.type_name() != null)
            type = Helpers.ResolveTypeName(context.type_name());

        FieldBuilder fb = TypeContext.Current.Builder.DefineField(
            context.Identifier().GetText(),
            type,
            Helpers.GetFieldAttributes(context.member_access_modifier(), context.member_oop_modifier(), context.member_special_modifier()));

        if (context.expression() != null)
            TypeContext.Current.FieldInitializers.Add((fb, context.expression()));

        CurrentFile.Fragments.Add(new()
        {
            Color = Color.Field,
            Column = context.Identifier().Symbol.Column,
            Line = context.Identifier().Symbol.Line,
            Length = context.Identifier().GetText().Length,
            ToolTip = TooltipGenerator.Field(fb)
        });

        return typeof(void);
    }

    private (Type Type, LoschScriptParser.ParameterContext Context)[] ResolveParameterList(LoschScriptParser.Parameter_listContext paramList)
    {
        if (paramList == null)
            return Array.Empty<(Type, LoschScriptParser.ParameterContext)>();

        List<(Type, LoschScriptParser.ParameterContext)> types = new();

        foreach (var param in paramList.parameter())
            types.Add((ResolveParameter(param), param));

        return types.ToArray();
    }

    private Type ResolveParameter(LoschScriptParser.ParameterContext param)
    {
        Type t = typeof(object);

        if (param.type_name() != null)
        {
            t = Helpers.ResolveTypeName(param.type_name());

            if (t != null)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Color = TooltipGenerator.ColorForType(t.GetTypeInfo()),
                    Line = param.type_name().Start.Line,
                    Column = param.type_name().Start.Column,
                    Length = param.type_name().GetText().Length,
                    ToolTip = TooltipGenerator.Type(t.GetTypeInfo(), true, true)
                });
            }
        }

        CurrentFile.Fragments.Add(new()
        {
            Color = Color.LocalValue,
            Line = param.Identifier().Symbol.Line,
            Column = param.Identifier().Symbol.Column,
            Length = param.Identifier().GetText().Length,
        });

        return t;
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

    public override Type VisitTop_level_statements([NotNull] LoschScriptParser.Top_level_statementsContext context)
    {
        TypeBuilder tb = Context.Module.DefineType($"{(string.IsNullOrEmpty(CurrentFile.ExportedNamespace) ? "" : $"{CurrentFile.ExportedNamespace}.")}Program");

        TypeContext tc = new()
        {
            Builder = tb
        };

        tc.FilesWhereDefined.Add(CurrentFile.Path);

        MethodBuilder mb = tb.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(int), new Type[] { typeof(string[]) });
        ILGenerator il = mb.GetILGenerator();
        MethodContext mc = new()
        {
            Builder = mb,
            IL = il
        };

        mc.FilesWhereDefined.Add(CurrentFile.Path);

        tc.Methods.Add(mc);

        Context.Types.Add(tc);

        if (context.expression().Length == 0)
        {
            EmitErrorMessage(0, 0, context.GetText().Length, LS0027_EmptyProgram, "The program does not contain any executable code.");
            return typeof(void);
        }

        foreach (IParseTree child in context.children.Take(context.children.Count - 1))
        {
            Type _t = Visit(child);

            if (_t != typeof(void) && CurrentMethod.SkipPop)
                CurrentMethod.IL.Emit(OpCodes.Pop);

            if (CurrentMethod.SkipPop)
                CurrentMethod.SkipPop = false;
        }

        // Last expression is like return statement
        Type ret = Visit(context.children.Last());

        if (ret != typeof(void) && ret != typeof(int) && ret != null)
        {
            EmitErrorMessage(context.expression().Last().Start.Line,
                context.expression().Last().Start.Column,
                context.expression().Last().GetText().Length,
                LS0050_ExpectedIntegerReturnValue,
                $"Expected expression of type 'int32' or 'void', but got type '{ret.FullName}'.");

            return ret;
        }

        if (ret != typeof(int) && ret != null)
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)0);

        CurrentMethod.IL.Emit(OpCodes.Ret);

        if (Context.Configuration.ApplicationType != Losch.LoschScript.Configuration.ApplicationType.Library && createAssembly)
            Helpers.SetEntryPoint(Context.Assembly, mb);

        tb.CreateType();
        return ret;
    }

    public override Type VisitExpression_atom([NotNull] LoschScriptParser.Expression_atomContext context)
    {
        return Visit(context.expression());
    }

    public override Type VisitNewlined_expression([NotNull] LoschScriptParser.Newlined_expressionContext context)
    {
        return Visit(context.expression());
    }

    public override Type VisitEquality_expression([NotNull] LoschScriptParser.Equality_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if ((Helpers.IsNumericType(t) && Helpers.IsNumericType(t2)) || (t == typeof(bool) && t2 == typeof(bool)))
        {
            if (Helpers.IsFloatingPointType(t) && !Helpers.IsFloatingPointType(t2))
            {
                CurrentMethod.IL.Emit(OpCodes.Conv_R8);
            }
            else if (Helpers.IsFloatingPointType(t2) && !Helpers.IsFloatingPointType(t))
            {
                CurrentMethod.IL.Emit(OpCodes.Pop);
                CurrentMethod.IL.Emit(OpCodes.Conv_R8);
                Visit(context.expression()[1]);
            }

            CurrentMethod.IL.Emit(OpCodes.Ceq);

            if (context.op.Text == "!=")
            {
                CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)0);
                CurrentMethod.IL.Emit(OpCodes.Ceq);
            }

            return typeof(bool);
        }

        if (context.op.Text == "==")
        {

            MethodInfo op = t.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

            if (op == null)
            {
                EmitErrorMessage(
                        context.op.Line,
                        context.op.Column,
                        context.op.Text.Length,
                        LS0036_ArithmeticError,
                        $"The type '{t.Name}' does not implement the equality operation.",
                        Path.GetFileName(CurrentFile.Path));

                return typeof(bool);
            }

            CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        }
        else
        {
            MethodInfo op = t.GetMethod("op_Inequality", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

            if (op == null)
            {
                EmitErrorMessage(
                        context.op.Line,
                        context.op.Column,
                        context.op.Text.Length,
                        LS0036_ArithmeticError,
                        $"The type '{t.Name}' does not implement the inequality operation.",
                        Path.GetFileName(CurrentFile.Path));

                return typeof(bool);
            }

            CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        }

        return typeof(bool);
    }

    public override Type VisitComparison_expression([NotNull] LoschScriptParser.Comparison_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if ((Helpers.IsNumericType(t) && Helpers.IsNumericType(t2)) || (t == typeof(bool) && t2 == typeof(bool)))
        {
            if (Helpers.IsFloatingPointType(t) && !Helpers.IsFloatingPointType(t2))
            {
                CurrentMethod.IL.Emit(OpCodes.Conv_R8);
            }
            else if (Helpers.IsFloatingPointType(t2) && !Helpers.IsFloatingPointType(t))
            {
                CurrentMethod.IL.Emit(OpCodes.Pop);
                CurrentMethod.IL.Emit(OpCodes.Conv_R8);
                Visit(context.expression()[1]);
            }

            if (context.op.Text == "<" || context.op.Text == ">=")
                EmitClt(CurrentMethod.IL, t);
            else
                EmitCgt(CurrentMethod.IL, t);

            if (context.op.Text == "<=" || context.op.Text == ">=")
            {
                CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)0);
                CurrentMethod.IL.Emit(OpCodes.Ceq);
            }

            return typeof(bool);
        }

        string methodName = $"op_{context.op.Text switch
        {
            "<" => "LessThan",
            ">" => "GreaterThan",
            "<=" => "LessThanOrEqual",
            _ => "GreaterThanOrEqual"
        }}";

        MethodInfo op = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                    context.op.Line,
                    context.op.Column,
                    context.op.Text.Length,
                    LS0036_ArithmeticError,
                    $"The type '{t.Name}' does not implement this comparison operation.",
                    Path.GetFileName(CurrentFile.Path));

            return typeof(bool);
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return typeof(bool);
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
                    context.Minus().Symbol.Line,
                        context.Minus().Symbol.Column,
                        context.Minus().GetText().Length,
                    LS0036_ArithmeticError,
                    $"The type '{t.Name}' does not implement the unary negation operation.",
                    Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    //public override Type VisitUnary_plus_expression([NotNull] LoschScriptParser.Unary_plus_expressionContext context)
    //{
    //    Type t = Visit(context.expression());

    //    if (Helpers.IsNumericType(t))
    //        return t;

    //    MethodInfo op = t.GetMethod("op_UnaryPlus", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t }, null);

    //    if (op == null)
    //    {
    //        EmitErrorMessage(
    //                context.Start.Line,
    //                context.Start.Column,
    //                LS0036_ArithmeticError,
    //                $"The type '{t.Name}' does not implement the unary plus operation.",
    //                Path.GetFileName(CurrentFile.Path));

    //        return t;
    //    }

    //    CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

    //    return t;
    //}

    public override Type VisitLogical_negation_expression([NotNull] LoschScriptParser.Logical_negation_expressionContext context)
    {
        Type t = Visit(context.expression());

        if (t == typeof(bool))
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)0);
            CurrentMethod.IL.Emit(OpCodes.Ceq);

            return t;
        }

        MethodInfo op = t.GetMethod("op_LogicalNot", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Exclamation_Mark().Symbol.Line,
                context.Exclamation_Mark().Symbol.Column,
                context.Exclamation_Mark().GetText().Length,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement the logical negation operation.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

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
            context.Double_Ampersand().Symbol.Line,
            context.Double_Ampersand().Symbol.Column,
            context.Double_Ampersand().GetText().Length,
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
            context.Double_Bar().Symbol.Line,
            context.Double_Bar().Symbol.Column,
            context.Double_Bar().GetText().Length,
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
                context.Bar().Symbol.Line,
                context.Bar().Symbol.Column,
                context.Bar().GetText().Length,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a bitwise or operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

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
                context.Ampersand().Symbol.Line,
                context.Ampersand().Symbol.Column,
                context.Ampersand().GetText().Length,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a bitwise and operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

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
                context.Caret().Symbol.Line,
                context.Caret().Symbol.Column,
                context.Caret().GetText().Length,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement an exclusive or operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

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

        MethodInfo op = t.GetMethod("op_OnesComplement", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Tilde().Symbol.Line,
                context.Tilde().Symbol.Column,
                context.Tilde().GetText().Length,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a complement operation.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitMultiply_expression([NotNull] LoschScriptParser.Multiply_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsNumericType(t))
        {
            EmitMul(CurrentMethod.IL, t);
            return t;
        }

        MethodInfo op = t.GetMethod("op_Multiply", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Asterisk().Symbol.Line,
                context.Asterisk().Symbol.Column,
                context.Asterisk().GetText().Length,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a multiplication operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitDivide_expression([NotNull] LoschScriptParser.Divide_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsNumericType(t))
        {
            EmitDiv(CurrentMethod.IL, t);
            return t;
        }

        MethodInfo op = t.GetMethod("op_Division", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Slash().Symbol.Line,
                context.Slash().Symbol.Column,
                context.Slash().GetText().Length,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a division operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitAddition_expression([NotNull] LoschScriptParser.Addition_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsNumericType(t) && Helpers.IsNumericType(t2))
        {
            EmitAdd(CurrentMethod.IL, t);
            return t;
        }

        if (t == typeof(string) || t2 == typeof(string))
        {
            if (t2 != typeof(string))
            {
                CurrentMethod.IL.DeclareLocal(t2);
                EmitStloc(CurrentMethod.IL, ++CurrentMethod.LocalIndex);
                CurrentMethod.IL.Emit(OpCodes.Ldloca, CurrentMethod.LocalIndex);

                MethodInfo toString = t2.GetMethod("ToString", Array.Empty<Type>());
                CurrentMethod.IL.EmitCall(Helpers.GetCallOpCode(t2), toString, null);
            }
            else if (t != typeof(string))
            {
                // TODO: Fix this mess ASAP

                CurrentMethod.IL.Emit(OpCodes.Pop);

                CurrentMethod.IL.DeclareLocal(t);
                EmitStloc(CurrentMethod.IL, ++CurrentMethod.LocalIndex);
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
                context.Plus().Symbol.Line,
                context.Plus().Symbol.Column,
                context.Plus().GetText().Length,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement an addition operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitSubtraction_expression([NotNull] LoschScriptParser.Subtraction_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsNumericType(t))
        {
            EmitSub(CurrentMethod.IL, t);
            return t;
        }

        MethodInfo op = t.GetMethod("op_Subtraction", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Minus().Symbol.Line,
                context.Minus().Symbol.Column,
                context.Minus().GetText().Length,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a subtraction operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitRemainder_expression([NotNull] LoschScriptParser.Remainder_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsNumericType(t))
        {
            EmitRem(CurrentMethod.IL, t);
            return t;
        }

        MethodInfo op = t.GetMethod("op_Modulus", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Percent().Symbol.Line,
                context.Percent().Symbol.Column,
                context.Percent().GetText().Length,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a remainder operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

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
                context.Double_Asterisk().Symbol.Line,
                context.Double_Asterisk().Symbol.Column,
                context.Double_Asterisk().GetText().Length,
                LS0036_ArithmeticError,
                $"The power operation is not supported by the types '{t.Name}' and '{t2.Name}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, m, null);

        return m.ReturnType;
    }

    public override Type VisitLeft_shift_expression([NotNull] LoschScriptParser.Left_shift_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsIntegerType(t))
        {
            CurrentMethod.IL.Emit(OpCodes.Shl);
            return t;
        }

        MethodInfo op = t.GetMethod("op_LeftShift", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Double_Less_Than().Symbol.Line,
                context.Double_Less_Than().Symbol.Column,
                context.Double_Less_Than().GetText().Length,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a left shift operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitRight_shift_expression([NotNull] LoschScriptParser.Right_shift_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (Helpers.IsIntegerType(t))
        {
            EmitShr(CurrentMethod.IL, t);
            return t;
        }

        MethodInfo op = t.GetMethod("op_RightShift", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Double_Greater_Than().Symbol.Line,
                context.Double_Greater_Than().Symbol.Column,
                context.Double_Greater_Than().GetText().Length,
                LS0002_MethodNotFound,
                $"The type '{t.Name}' does not implement a right shift operation with the specified parameter types.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitTypeof_expression([NotNull] LoschScriptParser.Typeof_expressionContext context)
    {
        Type t = Helpers.ResolveTypeName(context.Identifier().ToString(), context.Identifier().Symbol.Line, context.Identifier().Symbol.Column, context.Identifier().GetText().Length);
        CurrentMethod.IL.Emit(OpCodes.Ldtoken, t);

        MethodInfo typeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });
        CurrentMethod.IL.EmitCall(OpCodes.Call, typeFromHandle, null);

        return typeof(Type);
    }

    public Type GetConstructorOrCast(Type cType, LoschScriptParser.ArglistContext arglist, int line, int column, int length)
    {
        if (arglist != null)
        {
            Visit(arglist);
            ConstructorInfo cinf = null;

            ConstructorInfo[] constructors = cType.GetConstructors();

            bool success = false;

            foreach (ConstructorInfo possible in constructors)
            {
                ParameterInfo[] param = possible.GetParameters();

                if (param.Length != CurrentMethod.ArgumentTypesForNextMethodCall.Count)
                    continue;

                for (int i = 0; i < param.Length; i++)
                {
                    if (param[i].ParameterType != CurrentMethod.ArgumentTypesForNextMethodCall[i] && CurrentMethod.ArgumentTypesForNextMethodCall[i] != null)
                        continue;

                    success = true;
                }

                if (success)
                {
                    cinf = possible;
                    break;
                }
            }

            if (!success)
            {
                foreach (ConstructorInfo possible in constructors)
                {
                    ParameterInfo[] param = possible.GetParameters();

                    if (param.Length != CurrentMethod.ArgumentTypesForNextMethodCall.Count)
                        continue;

                    for (int i = 0; i < param.Length; i++)
                    {
                        if (param[i].ParameterType != CurrentMethod.ArgumentTypesForNextMethodCall[i])
                        {
                            if (param[i].ParameterType == typeof(object))
                            {
                                if (CurrentMethod.ArgumentTypesForNextMethodCall[i] != null)
                                    CurrentMethod.IL.Emit(OpCodes.Box, CurrentMethod.ArgumentTypesForNextMethodCall[i]);
                            }
                            else
                                continue;

                            success = true;
                        }

                        success = true;
                    }

                    if (success)
                    {
                        cinf = possible;
                        break;
                    }
                }
            }

            if (cinf == null)
            {
                if (CurrentMethod.ArgumentTypesForNextMethodCall.Count != 1)
                {
                    EmitErrorMessage(line, column, length, LS0002_MethodNotFound, $"The type '{cType.Name}' does not contain a constructor with the specified argument types.");
                    CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                    return cType;
                }

                string aqn = cType.AssemblyQualifiedName;

                if (aqn == typeof(object).AssemblyQualifiedName)
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, CurrentMethod.ArgumentTypesForNextMethodCall[0]);
                    CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                    return cType;
                }

                if (aqn == typeof(int).AssemblyQualifiedName || aqn == typeof(uint).AssemblyQualifiedName)
                {
                    CurrentMethod.IL.Emit(OpCodes.Conv_I4);
                    CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                    return cType;
                }

                if (aqn == typeof(long).AssemblyQualifiedName || aqn == typeof(ulong).AssemblyQualifiedName)
                {
                    CurrentMethod.IL.Emit(OpCodes.Conv_I8);
                    CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                    return cType;
                }

                if (aqn == typeof(nint).AssemblyQualifiedName || aqn == typeof(nuint).AssemblyQualifiedName)
                {
                    CurrentMethod.IL.Emit(OpCodes.Conv_I);
                    CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                    return cType;
                }

                if (aqn == typeof(byte).AssemblyQualifiedName || aqn == typeof(sbyte).AssemblyQualifiedName)
                {
                    CurrentMethod.IL.Emit(OpCodes.Conv_I1);
                    CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                    return cType;
                }

                if (aqn == typeof(short).AssemblyQualifiedName || aqn == typeof(ushort).AssemblyQualifiedName)
                {
                    CurrentMethod.IL.Emit(OpCodes.Conv_I2);
                    CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                    return cType;
                }

                if (aqn == typeof(float).AssemblyQualifiedName)
                {
                    CurrentMethod.IL.Emit(OpCodes.Conv_R4);
                    CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                    return cType;
                }

                if (aqn == typeof(double).AssemblyQualifiedName)
                {
                    CurrentMethod.IL.Emit(OpCodes.Conv_R8);
                    CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                    return cType;
                }

                if (aqn == typeof(bool).AssemblyQualifiedName)
                {
                    CurrentMethod.IL.Emit(OpCodes.Conv_I4);
                    CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                    return cType;
                }

                if (aqn == typeof(char).AssemblyQualifiedName)
                {
                    CurrentMethod.IL.Emit(OpCodes.Conv_I4);
                    CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                    return cType;
                }

                var mImplicit = cType.GetMethod("op_Implicit", CurrentMethod.ArgumentTypesForNextMethodCall.ToArray());
                var mExplicit = cType.GetMethod("op_Explicit", CurrentMethod.ArgumentTypesForNextMethodCall.ToArray());

                if (mImplicit != null)
                {
                    CurrentMethod.IL.EmitCall(OpCodes.Call, mImplicit, null);
                    CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                    return cType;
                }

                if (mExplicit != null)
                {
                    CurrentMethod.IL.EmitCall(OpCodes.Call, mExplicit, null);
                    CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                    return cType;
                }

                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();

                return cType;
            }

            CurrentMethod.IL.Emit(OpCodes.Newobj, cinf);
            EmitErrorMessage(line, column, length, LS0002_MethodNotFound, $"The type '{cType.Name}' does not contain a constructor or conversion with the specified argument types.");
            CurrentMethod.ArgumentTypesForNextMethodCall.Clear();

            return cType;
        }

        ConstructorInfo c = cType.GetConstructor(Type.EmptyTypes);

        if (c == null)
        {
            EmitErrorMessage(line, column, length, LS0002_MethodNotFound, $"The type '{cType.Name}' does not specify a parameterless constructor.");
            CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
            return cType;
        }

        CurrentMethod.IL.Emit(OpCodes.Newobj, c);

        CurrentMethod.ArgumentTypesForNextMethodCall.Clear();

        return cType;
    }

    public Type GetMember(Type type, string name, LoschScriptParser.ArglistContext arglist, int line, int column, int length, int local = 0)
    {
        // Handle special functions of CodeGeneration class
        if (type == typeof(CompilerServices.CodeGeneration) && Helpers.HandleSpecialFunction(name, arglist, line, column, length))
            return typeof(void);

        // Check if it is a method with parameters
        if (arglist != null)
        {
            Visit(arglist);
            MethodInfo m = null;

            MethodInfo[] methods = type.GetMethods().Where(m => m.Name == name).ToArray();

            bool success = false;

            foreach (MethodInfo possible in methods)
            {
                if (possible.GetParameters().Length != CurrentMethod.ArgumentTypesForNextMethodCall.Count)
                    continue;

                string[] _params = possible.GetParameters().Select(p => p.ParameterType.FullName).ToArray();
                string[] _params2 = CurrentMethod.ArgumentTypesForNextMethodCall.Select(t => t == null ? "" : t.FullName).ToArray();

                for (int i = 0; i < _params.Length; i++)
                {
                    if (_params[i] != _params2[i] && _params2[i] != "")
                        continue;

                    success = true;
                }

                if (success)
                {
                    m = possible;
                    break;
                }
            }

            // Check again and box if neccessary
            if (!success)
            {
                foreach (MethodInfo possible in methods)
                {
                    if (possible.GetParameters().Length != CurrentMethod.ArgumentTypesForNextMethodCall.Count)
                        continue;

                    string[] _params = possible.GetParameters().Select(p => p.ParameterType.FullName).ToArray();
                    string[] _params2 = CurrentMethod.ArgumentTypesForNextMethodCall.Select(t => t == null ? "" : t.FullName).ToArray();

                    for (int i = 0; i < _params.Length; i++)
                    {
                        if (_params[i] != _params2[i])
                        {
                            if (Type.GetType(_params[i]) == typeof(object))
                            {
                                if (_params2[i] != "")
                                {
                                    CurrentMethod.IL.Emit(OpCodes.Box, Helpers.ResolveTypeName(_params2[i], line, column, length, true));
                                }
                            }
                            else
                                continue;
                        }

                        success = true;
                    }

                    if (success)
                    {
                        m = possible;
                        break;
                    }
                }
            }

            CurrentMethod.ArgumentTypesForNextMethodCall.Clear();

            if (m != null)
            {
                CurrentFile.Fragments.Add(new()
                {
                    Line = line,
                    Column = column,
                    Length = length,
                    Color = Color.Function,
                    ToolTip = TooltipGenerator.Function(m)
                });

                if (m.IsStatic || type.IsValueType)
                    CurrentMethod.IL.EmitCall(OpCodes.Call, m, null);
                else
                    CurrentMethod.IL.EmitCall(OpCodes.Callvirt, m, null);

                return m.ReturnType;
            }
            else
            {
                EmitErrorMessage(
                    line,
                    column,
                    length,
                    LS0002_MethodNotFound,
                    $"The type \"{type.Name}\" does not contain a function called \"{name}\" with the specified argument types.");

                return typeof(void);
            }
        }

        MethodInfo parameterlessFunc = type.GetMethod(name, Array.Empty<Type>());
        if (parameterlessFunc != null)
        {
            if (parameterlessFunc.IsStatic || type.IsValueType)
            {
                if (parameterlessFunc.DeclaringType == typeof(object))
                {
                    CurrentMethod.IL.Emit(OpCodes.Pop);
                    CurrentMethod.IL.Emit(OpCodes.Ldloc, local);
                    CurrentMethod.IL.Emit(OpCodes.Box, CurrentMethod.Locals.Where(l => l.Index == local).First().Builder.LocalType);
                }

                CurrentMethod.IL.EmitCall(OpCodes.Call, parameterlessFunc, null);
            }
            else
                CurrentMethod.IL.EmitCall(OpCodes.Callvirt, parameterlessFunc, null);

            CurrentFile.Fragments.Add(new()
            {
                Line = line,
                Column = column,
                Length = length,
                Color = Color.Function,
                ToolTip = TooltipGenerator.Function(parameterlessFunc)
            });

            return parameterlessFunc.ReturnType;
        }

        MethodInfo property = type.GetMethod($"get_{name}");
        if (property != null)
        {
            if (property.IsStatic || type.IsValueType)
                CurrentMethod.IL.EmitCall(OpCodes.Call, property, null);
            else
                CurrentMethod.IL.EmitCall(OpCodes.Callvirt, property, null);

            CurrentFile.Fragments.Add(new()
            {
                Line = line,
                Column = column,
                Length = length,
                Color = Color.Property,
                ToolTip = TooltipGenerator.Property(type.GetProperty(name))
            });

            return property.ReturnType;
        }

        FieldInfo f = type.GetField(name);
        if (f != null)
        {
            try
            {
                // Constant
                EmitConst(CurrentMethod.IL, f.GetRawConstantValue());
            }
            catch (Exception)
            {
                // Not a constant

                if (f.IsStatic)
                    CurrentMethod.IL.Emit(OpCodes.Ldsfld, f);
                else
                    CurrentMethod.IL.Emit(OpCodes.Ldfld, f);
            }

            CurrentFile.Fragments.Add(new()
            {
                Line = line,
                Column = column,
                Length = length,
                Color = Color.Field,
                ToolTip = TooltipGenerator.Field(f)
            });

            return f.FieldType;
        }
        else
        {
            EmitErrorMessage(
                line,
                column,
                length,
                LS0039_FieldNotFound,
                $"The type \"{type.Name}\" does not contain a field or property called \"{name}\".");

            return typeof(void);
        }
    }

    public override Type VisitMember_access_expression([NotNull] LoschScriptParser.Member_access_expressionContext context)
    {
        // Check for local or argument of this name
        var localOrParam = Helpers.LoadLocalOrParameter(context.Identifier().GetText());

        if (localOrParam.Result)
            return localOrParam.Type;

        Type t = Visit(context.expression());

        if (t.IsValueType)
        {
            CurrentMethod.IL.DeclareLocal(t);
            CurrentMethod.LocalIndex++;
            CurrentMethod.IL.Emit(OpCodes.Stloc, CurrentMethod.LocalIndex);
            EmitLdloca(CurrentMethod.IL, CurrentMethod.LocalIndex);
        }

        return GetMember(t, context.Identifier().GetText(), context.arglist(), context.Identifier().Symbol.Line, context.Identifier().Symbol.Column, context.Identifier().GetText().Length);
    }

    public override Type VisitFull_identifier_member_access_expression([NotNull] LoschScriptParser.Full_identifier_member_access_expressionContext context)
    {
        Type type;

        (string Name, LocalBuilder Builder, bool IsConstant, int Index, UnionValue Union) local = default;
        (string Name, Type Type, ParameterBuilder Builder, int Index, UnionValue Union) param = default;

        // Check for local or argument of this name
        var localOrParam = Helpers.GetLocalOrParameter(context.full_identifier().Identifier()[0].GetText());

        if (localOrParam != default)
        {
            if (localOrParam.IsParameter)
            {
                param = CurrentMethod.Parameters.First(p => p.Index == localOrParam.Index);
                type = param.Type;
            }
            else
            {
                local = CurrentMethod.Locals.First(l => l.Index == localOrParam.Index);
                type = local.Builder.LocalType;
            }

            CurrentFile.Fragments.Add(new()
            {
                Line = context.full_identifier().Identifier()[0].Symbol.Line,
                Column = context.full_identifier().Identifier()[0].Symbol.Column,
                Length = context.full_identifier().Identifier()[0].GetText().Length,
                Color = Color.LocalValue,
                ToolTip = local == default ?
                    TooltipGenerator.Parameter(param.Name, param.Type)
                    : TooltipGenerator.Local(local.Name, !local.IsConstant, local.Builder)
            });

            if (context.full_identifier().Identifier().Length == 1)
            {
                if (type == typeof(UnionValue))
                {
                    if (local == default)
                        EmitLdarga(CurrentMethod.IL, param.Index);
                    else
                        EmitLdloca(CurrentMethod.IL, local.Index);

                    MethodInfo getter = type.GetMethod("get_Value");
                    CurrentMethod.IL.EmitCall(OpCodes.Call, getter, null);

                    return typeof(object);
                }

                if (local == default)
                    EmitLdarg(CurrentMethod.IL, param.Index);
                else
                    EmitLdloc(CurrentMethod.IL, local.Index);

                return type;
            }

            if (local == default)
            {
                if (param.Type.IsValueType)
                    EmitLdarga(CurrentMethod.IL, param.Index);
                else
                    EmitLdarg(CurrentMethod.IL, param.Index);
            }
            else
            {
                if (local.Builder.LocalType.IsValueType)
                    EmitLdloca(CurrentMethod.IL, local.Index);
                else
                    EmitLdloc(CurrentMethod.IL, local.Index);
            }
        }
        else
        {
            if (context.full_identifier().Identifier().Length == 1)
            {
                // Global Method (Type Import)
                Type t = Helpers.ResolveGlobalMethod(context.full_identifier().GetText(), context.full_identifier().Identifier().Last().Symbol.Line, context.full_identifier().Identifier().Last().Symbol.Column, context.full_identifier().Identifier().Last().GetText().Length).Type;

                if (t != null)
                    return GetMember(t, context.full_identifier().GetText(), context.arglist(), context.full_identifier().Identifier().Last().Symbol.Line, context.full_identifier().Identifier().Last().Symbol.Column, context.full_identifier().Identifier().Last().GetText().Length, local.Index);

                // Constructor
                Type cType = Helpers.ResolveTypeName(context.full_identifier().GetText(), context.full_identifier().Identifier().Last().Symbol.Line, context.full_identifier().Identifier().Last().Symbol.Column, context.full_identifier().Identifier().Last().GetText().Length);
                return GetConstructorOrCast(cType, context.arglist(), context.full_identifier().Identifier().Last().Symbol.Line, context.full_identifier().Identifier().Last().Symbol.Column, context.full_identifier().Identifier().Last().GetText().Length);
            }

            type = Helpers.ResolveTypeName(
                string.Join(".", context.full_identifier().Identifier()[0..^1].Select(i => i.GetText())),
                context.full_identifier().Identifier()[^2].Symbol.Line, context.full_identifier().Identifier()[^2].Symbol.Column, context.full_identifier().Identifier()[^2].GetText().Length,
                false);
        }

        return GetMember(type, context.full_identifier().Identifier().Last().GetText(), context.arglist(), context.full_identifier().Identifier().Last().Symbol.Line, context.full_identifier().Identifier().Last().Symbol.Column, context.full_identifier().Identifier().Last().GetText().Length, local.Index);
    }

    public override Type VisitArglist([NotNull] LoschScriptParser.ArglistContext context)
    {
        foreach (IParseTree tree in context.expression())
            CurrentMethod.ArgumentTypesForNextMethodCall.Add(Visit(tree));

        return typeof(void);
    }

    public override Type VisitCode_block([NotNull] LoschScriptParser.Code_blockContext context)
    {
        CurrentFile.FoldingRegions.Add(new()
        {
            StartLine = context.Open_Brace().Symbol.Line,
            StartColumn = context.Open_Brace().Symbol.Column,
            EndLine = context.Close_Brace().Symbol.Line,
            EndColumn = context.Close_Brace().Symbol.Column + 1
        });

        CurrentFile.GuideLines.Add(new()
        {
            StartLine = context.Open_Brace().Symbol.Line,
            EndLine = context.Close_Brace().Symbol.Line,
            Column = context.Close_Brace().Symbol.Column // TOOD: A highly questionable implementation
        });

        if (context.expression().Length == 0)
            return typeof(void);

        foreach (IParseTree tree in context.expression().Take(context.expression().Length - 1))
        {
            Type _t = Visit(tree);

            if (_t != typeof(void) && !CurrentMethod.SkipPop)
                CurrentMethod.IL.Emit(OpCodes.Pop);

            if (CurrentMethod.SkipPop)
                CurrentMethod.SkipPop = false;
        }

        return Visit(context.expression().Last());
    }

    public override Type VisitIdentifier_atom([NotNull] LoschScriptParser.Identifier_atomContext context)
    {
        if (CurrentMethod != null && CurrentMethod.Locals.Where(l => l.Name == context.Identifier().GetText()).Any())
        {
            var local = CurrentMethod.Locals.Where(l => l.Name == context.Identifier().GetText()).First();
            CurrentMethod.IL.Emit(OpCodes.Ldloc, local.Index);

            return local.Builder.LocalType;
        }

        return Helpers.ResolveTypeName(context.Identifier().GetText(), context.Identifier().Symbol.Line, context.Identifier().Symbol.Column, context.Identifier().GetText().Length);
    }

    public override Type VisitIdentifier_expression([NotNull] LoschScriptParser.Identifier_expressionContext context)
    {
        return Visit(context.Identifier());
    }

    public override Type VisitFull_identifier([NotNull] LoschScriptParser.Full_identifierContext context)
    {
        return Helpers.ResolveTypeName(context.GetText(), context.Identifier().Last().Symbol.Line, context.Identifier().Last().Symbol.Column, context.Identifier().Last().GetText().Length);
    }

    public override Type VisitFull_identifier_expression([NotNull] LoschScriptParser.Full_identifier_expressionContext context)
    {
        return Visit(context.full_identifier());
    }

    public override Type VisitPrefix_if_expression([NotNull] LoschScriptParser.Prefix_if_expressionContext context)
    {
        Type t;
        List<Type> t2 = new();
        Type t3 = null;

        Label falseBranch = CurrentMethod.IL.DefineLabel();
        Label restBranch = CurrentMethod.IL.DefineLabel();

        // Comparative expression
        Type ct = Visit(context.if_branch().expression()[0]);

        if (ct != typeof(bool))
        {
            EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    context.Start.Text.Length,
                    LS0038_ConditionalExpressionClauseNotBoolean,
                    $"The condition of a conditional expression has to be a boolean.");
        }

        CurrentMethod.IL.Emit(OpCodes.Brfalse, falseBranch);

        if (context.if_branch().code_block() != null)
            t = Visit(context.if_branch().code_block());
        else
            t = Visit(context.if_branch().expression().Last());

        CurrentMethod.IL.Emit(OpCodes.Br, restBranch);

        CurrentMethod.IL.MarkLabel(falseBranch);

        if (context.elif_branch() != null)
        {
            foreach (LoschScriptParser.Elif_branchContext tree in context.elif_branch())
            {
                Label stillFalseBranch = CurrentMethod.IL.DefineLabel();

                Type _ct = Visit(tree.expression()[0]);

                if (_ct != typeof(bool))
                {
                    EmitErrorMessage(
                            context.Start.Line,
                            context.Start.Column,
                            context.Start.Text.Length,
                            LS0038_ConditionalExpressionClauseNotBoolean,
                            $"The condition of a conditional expression has to be a boolean.");
                }

                CurrentMethod.IL.Emit(OpCodes.Brfalse, stillFalseBranch);

                if (tree.code_block() != null)
                    t2.Add(Visit(tree.code_block()));
                else
                    t2.Add(Visit(tree.expression().Last()));

                CurrentMethod.IL.Emit(OpCodes.Br, restBranch);
                CurrentMethod.IL.MarkLabel(stillFalseBranch);
            }
        }

        if (context.else_branch() != null)
        {
            if (context.else_branch().code_block() != null)
                t3 = Visit(context.else_branch().code_block());
            else
                t3 = Visit(context.else_branch().expression());
        }

        CurrentMethod.IL.MarkLabel(restBranch);

        bool allEqual = t2.Select(t => t.Name).Distinct().Count() == 1;

        if (t2.Count == 0)
        {
            allEqual = true;
            t2.Add(t);
        }

        if (allEqual && t3 == null)
            return t;

        if (!allEqual || t != t3 || t != t2[0])
        {
            EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    context.GetText().Length,
                    LS0037_BranchExpressionTypesUnequal,
                    $"The return types of the branches of the conditional expression do not match.");
        }

        return t;
    }

    public override Type VisitPostfix_if_expression([NotNull] LoschScriptParser.Postfix_if_expressionContext context)
    {
        Label fb = CurrentMethod.IL.DefineLabel();
        Label rest = CurrentMethod.IL.DefineLabel();

        // Comparative expression
        Type ct = Visit(context.postfix_if_branch().expression());

        if (ct != typeof(bool))
        {
            EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    context.Start.Text.Length,
                    LS0038_ConditionalExpressionClauseNotBoolean,
                    $"The condition of a conditional expression has to be a boolean.");
        }

        CurrentMethod.IL.Emit(OpCodes.Brfalse, fb);

        Type t = Visit(context.expression());

        CurrentMethod.IL.MarkLabel(fb);
        CurrentMethod.IL.Emit(OpCodes.Br, rest);

        CurrentMethod.IL.MarkLabel(rest);

        return t;
    }

    public override Type VisitBlock_postfix_if_expression([NotNull] LoschScriptParser.Block_postfix_if_expressionContext context)
    {
        Label fb = CurrentMethod.IL.DefineLabel();
        Label rest = CurrentMethod.IL.DefineLabel();

        // Comparative expression
        Type ct = Visit(context.postfix_if_branch().expression());

        if (ct != typeof(bool))
        {
            EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    context.Start.Text.Length,
                    LS0038_ConditionalExpressionClauseNotBoolean,
                    $"The condition of a conditional expression has to be a boolean.");
        }

        CurrentMethod.IL.Emit(OpCodes.Brfalse, fb);

        Type t = Visit(context.code_block());

        CurrentMethod.IL.MarkLabel(fb);
        CurrentMethod.IL.Emit(OpCodes.Br, rest);

        CurrentMethod.IL.MarkLabel(rest);

        return t;
    }

    public override Type VisitPrefix_unless_expression([NotNull] LoschScriptParser.Prefix_unless_expressionContext context)
    {

        Type t;
        List<Type> t2 = new();
        Type t3 = null;

        Label falseBranch = CurrentMethod.IL.DefineLabel();
        Label restBranch = CurrentMethod.IL.DefineLabel();

        // Comparative expression
        Type ct = Visit(context.unless_branch().expression()[0]);

        if (ct != typeof(bool))
        {
            EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    context.Start.Text.Length,
                    LS0038_ConditionalExpressionClauseNotBoolean,
                    $"The condition of a conditional expression has to be a boolean.");
        }

        CurrentMethod.IL.Emit(OpCodes.Brtrue, falseBranch);

        if (context.unless_branch().code_block() != null)
            t = Visit(context.unless_branch().code_block());
        else
            t = Visit(context.unless_branch().expression().Last());

        CurrentMethod.IL.Emit(OpCodes.Br, restBranch);

        CurrentMethod.IL.MarkLabel(falseBranch);

        if (context.else_unless_branch() != null)
        {
            foreach (LoschScriptParser.Else_unless_branchContext tree in context.else_unless_branch())
            {
                Label stillFalseBranch = CurrentMethod.IL.DefineLabel();

                Type _ct = Visit(tree.expression()[0]);

                if (_ct != typeof(bool))
                {
                    EmitErrorMessage(
                            context.Start.Line,
                            context.Start.Column,
                            context.Start.Text.Length,
                            LS0038_ConditionalExpressionClauseNotBoolean,
                            $"The condition of a conditional expression has to be a boolean.");
                }

                CurrentMethod.IL.Emit(OpCodes.Brtrue, stillFalseBranch);

                if (tree.code_block() != null)
                    t2.Add(Visit(tree.code_block()));
                else
                    t2.Add(Visit(tree.expression().Last()));

                CurrentMethod.IL.Emit(OpCodes.Br, restBranch);
                CurrentMethod.IL.MarkLabel(stillFalseBranch);
            }
        }

        if (context.else_branch() != null)
        {
            if (context.else_branch().code_block() != null)
                t3 = Visit(context.else_branch().code_block());
            else
                t3 = Visit(context.else_branch().expression());
        }

        CurrentMethod.IL.MarkLabel(restBranch);

        bool allEqual = t2.Select(t => t.Name).Distinct().Count() == 1;

        if (t2.Count == 0)
        {
            allEqual = true;
            t2.Add(t);
        }

        if (allEqual && t3 == null)
            return t;

        if (!allEqual || t != t3 || t != t2[0])
        {
            EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    context.GetText().Length,
                    LS0037_BranchExpressionTypesUnequal,
                    $"The return types of the branches of the conditional expression do not match.");
        }

        return t;
    }

    public override Type VisitBlock_postfix_unless_expression([NotNull] LoschScriptParser.Block_postfix_unless_expressionContext context)
    {
        Label fb = CurrentMethod.IL.DefineLabel();
        Label rest = CurrentMethod.IL.DefineLabel();

        // Comparative expression
        Type ct = Visit(context.postfix_unless_branch().expression());

        if (ct != typeof(bool))
        {
            EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    context.Start.Text.Length,
                    LS0038_ConditionalExpressionClauseNotBoolean,
                    $"The condition of a conditional expression has to be a boolean.");
        }

        CurrentMethod.IL.Emit(OpCodes.Brtrue, fb);

        Type t = Visit(context.code_block());

        CurrentMethod.IL.MarkLabel(fb);
        CurrentMethod.IL.Emit(OpCodes.Br, rest);

        CurrentMethod.IL.MarkLabel(rest);

        return t;
    }

    public override Type VisitPostfix_unless_expression([NotNull] LoschScriptParser.Postfix_unless_expressionContext context)
    {
        Label fb = CurrentMethod.IL.DefineLabel();
        Label rest = CurrentMethod.IL.DefineLabel();

        // Comparative expression
        Type ct = Visit(context.postfix_unless_branch().expression());

        if (ct != typeof(bool))
        {
            EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    context.Start.Text.Length,
                    LS0038_ConditionalExpressionClauseNotBoolean,
                    $"The condition of a conditional expression has to be a boolean.");
        }

        CurrentMethod.IL.Emit(OpCodes.Brtrue, fb);

        Type t = Visit(context.expression());

        CurrentMethod.IL.MarkLabel(fb);
        CurrentMethod.IL.Emit(OpCodes.Br, rest);

        CurrentMethod.IL.MarkLabel(rest);

        return t;
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

        if (text.EndsWith("sb", StringComparison.OrdinalIgnoreCase))
        {
            EmitLdcI4(CurrentMethod.IL, sbyte.Parse(text[0..^2].Replace("'", "")));
            return typeof(sbyte);
        }

        if (text.EndsWith("b", StringComparison.OrdinalIgnoreCase))
        {
            text += "0";

            EmitLdcI4(CurrentMethod.IL, byte.Parse(text[0..^2].Replace("'", "")));
            return typeof(byte);
        }

        if (text.EndsWith("us", StringComparison.OrdinalIgnoreCase))
        {
            EmitLdcI4(CurrentMethod.IL, ushort.Parse(text[0..^2].Replace("'", "")));
            return typeof(ushort);
        }

        if (text.EndsWith("s", StringComparison.OrdinalIgnoreCase))
        {
            text += "0";

            EmitLdcI4(CurrentMethod.IL, short.Parse(text[0..^2].Replace("'", "")));
            return typeof(short);
        }

        if (text.EndsWith("ul", StringComparison.OrdinalIgnoreCase))
        {
            CurrentMethod.IL.Emit(OpCodes.Ldc_I8, long.Parse(text[0..^2].Replace("'", "")));
            return typeof(ulong);
        }

        if (text.EndsWith("u", StringComparison.OrdinalIgnoreCase))
        {
            text += "0";

            EmitLdcI4(CurrentMethod.IL, uint.Parse(text[0..^2].Replace("'", "")));
            return typeof(uint);
        }

        if (text.EndsWith("l", StringComparison.OrdinalIgnoreCase))
        {
            text += "0";

            CurrentMethod.IL.Emit(OpCodes.Ldc_I8, long.Parse(text[0..^2].Replace("'", "")));
            return typeof(long);
        }

        if (text.EndsWith("un", StringComparison.OrdinalIgnoreCase))
        {
            EmitLdcI4(CurrentMethod.IL, int.Parse(text[0..^2].Replace("'", "")));
            return typeof(nuint);
        }

        if (text.EndsWith("n", StringComparison.OrdinalIgnoreCase))
        {
            text += "0";

            EmitLdcI4(CurrentMethod.IL, int.Parse(text[0..^2].Replace("'", "")));
            return typeof(nint);
        }

        text += "00";

        EmitLdcI4(CurrentMethod.IL, int.Parse(text[0..^2].Replace("'", "")));
        return typeof(int);
    }

    public override Type VisitString_atom([NotNull] LoschScriptParser.String_atomContext context)
    {
        string text = context.GetText()[1..^1];

        if (context.Verbatim_String_Literal() != null)
        {
            string verbatimText = text[1..];

            Regex doubleQuote = new(@"""""");
            foreach (Match match in doubleQuote.Matches(verbatimText))
            {
                CurrentFile.Fragments.Add(new()
                {
                    Color = Color.StringEscapeSequence,
                    Length = match.Length,
                    Line = context.Start.Line,
                    Column = context.Start.Column + match.Index + 1,
                });
            }

            CurrentMethod.IL.Emit(OpCodes.Ldstr, verbatimText);
            return typeof(string);
        }

        Regex escapeSequenceRegex = new(@"\^(?:['""^0abfnrtv]|[0-9A-Fa-f]{1,4})");
        foreach (Match match in escapeSequenceRegex.Matches(text))
        {
            CurrentFile.Fragments.Add(new()
            {
                Color = Color.StringEscapeSequence,
                Length = match.Length,
                Line = context.Start.Line,
                Column = context.Start.Column + match.Index + 1,
            });
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
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)1);
            return typeof(bool);
        }

        CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)0);

        return typeof(bool);
    }

    public override Type VisitLocal_declaration_or_assignment([NotNull] LoschScriptParser.Local_declaration_or_assignmentContext context)
    {
        var localOrParam = Helpers.GetLocalOrParameter(context.Identifier().GetText());

        if (localOrParam != default)
        {
            if (localOrParam.IsParameter)
            {
                EmitErrorMessage(
                    context.Equals().Symbol.Line,
                    context.Equals().Symbol.Column,
                    context.Equals().GetText().Length,
                    LS0018_ImmutableValueReassignment,
                    $"'{CurrentMethod.Parameters.First(p => p.Index == localOrParam.Index).Name}' is immutable and cannot be changed.");

                return localOrParam.Type;
            }

            var local = CurrentMethod.Locals.Where(e => e.Name == context.Identifier().GetText()).First();

            if (local.IsConstant)
            {
                EmitErrorMessage(
                    context.Equals().Symbol.Line,
                    context.Equals().Symbol.Column,
                    context.Equals().GetText().Length,
                    LS0018_ImmutableValueReassignment,
                    $"'{local.Name}' is immutable and cannot be changed.");

                return local.Builder.LocalType;
            }

            Type type = Visit(context.expression());

            LocalBuilder tempLocalBuilder = CurrentMethod.IL.DeclareLocal(type);
            tempLocalBuilder.SetLocalSymInfo(GetTempVariableName(CurrentMethod.TempValueIndex++));
            CurrentMethod.Locals.Add((GetTempVariableName(CurrentMethod.TempValueIndex), tempLocalBuilder, true, CurrentMethod.LocalIndex++, new(null, type)));

            EmitStloc(CurrentMethod.IL, CurrentMethod.LocalIndex);

            if (type != local.Builder.LocalType)
            {
                if (local.Builder.LocalType == typeof(UnionValue))
                {
                    if (local.Union.AllowedTypes.Contains(type))
                    {
                        EmitLdloca(CurrentMethod.IL, local.Index);

                        EmitLdloc(CurrentMethod.IL, CurrentMethod.LocalIndex);
                        CurrentMethod.IL.Emit(OpCodes.Box, type);

                        MethodInfo m = typeof(UnionValue).GetMethod("set_Value", new Type[] { typeof(object) });
                        CurrentMethod.IL.Emit(OpCodes.Call, m);

                        CurrentMethod.SkipPop = true;
                        return local.Union.GetType();
                    }

                    EmitErrorMessage(
                        context.Equals().Symbol.Line,
                        context.Equals().Symbol.Column,
                        context.Equals().GetText().Length,
                        LS0019_GenericValueTypeInvalid,
                        $"Values of type '{type}' are not supported by union type '{local.Union.ToTypeString()}'.");

                    return local.Union.GetType();
                }

                EmitErrorMessage(
                    context.Equals().Symbol.Line,
                    context.Equals().Symbol.Column,
                    context.Equals().GetText().Length,
                    LS0006_VariableTypeChanged,
                    $"The type of the new value of '{local.Name}' does not match the type of the old value.");

                return type;
            }

            EmitLdloc(CurrentMethod.IL, CurrentMethod.LocalIndex);

            EmitStloc(CurrentMethod.IL, local.Index);

            EmitLdloc(CurrentMethod.IL, local.Index);

            return local.Builder.LocalType;
        }

        Type t = Visit(context.expression());

        Type t1 = t;

        if (context.type_name() != null)
        {
            Type t2 = Visit(context.type_name());

            // TODO: Check if types are compatible and possibly call op_Implicit / op_Explicit...

            t = t2;
        }

        LocalBuilder lb = CurrentMethod.IL.DeclareLocal(t);

        CurrentFile.Fragments.Add(new()
        {
            Line = context.Identifier().Symbol.Line,
            Column = context.Identifier().Symbol.Column,
            Length = context.Identifier().GetText().Length,
            Color = context.Var() == null ? Color.LocalValue : Color.LocalVariable,
            ToolTip = TooltipGenerator.Local(context.Identifier().GetText(), context.Var() != null, lb)
        });

        if (Context.Configuration.Configuration == Losch.LoschScript.Configuration.Configuration.Debug && createAssembly)
            Helpers.SetLocalSymInfo(lb, context.Identifier().GetText());

        CurrentMethod.LocalIndex++;

        CurrentMethod.Locals.Add((context.Identifier().GetText(), lb, context.Var() == null, CurrentMethod.LocalIndex, CurrentMethod.CurrentUnion));

        if (t == typeof(UnionValue))
        {
            CurrentMethod.IL.Emit(OpCodes.Box, t1);

            ConstructorInfo constructor = t.GetConstructor(new Type[] { typeof(object), typeof(Type[]) });

            UnionValue union = CurrentMethod.CurrentUnion;

            EmitLdcI4(CurrentMethod.IL, union.AllowedTypes.Length);
            CurrentMethod.IL.Emit(OpCodes.Newarr, typeof(Type));
            CurrentMethod.IL.Emit(OpCodes.Dup);

            for (int i = 0; i < union.AllowedTypes.Length; i++)
            {
                EmitLdcI4(CurrentMethod.IL, i);
                CurrentMethod.IL.Emit(OpCodes.Ldtoken, union.AllowedTypes[i]);

                MethodInfo getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });
                CurrentMethod.IL.Emit(OpCodes.Call, getTypeFromHandle);
                CurrentMethod.IL.Emit(OpCodes.Stelem_Ref);

                CurrentMethod.IL.Emit(OpCodes.Dup);
            }

            CurrentMethod.IL.Emit(OpCodes.Pop);

            CurrentMethod.IL.Emit(OpCodes.Newobj, constructor);
        }

        EmitStloc(CurrentMethod.IL, CurrentMethod.LocalIndex);

        EmitLdloc(CurrentMethod.IL, CurrentMethod.LocalIndex);

        return t;
    }

    public override Type VisitType_name([NotNull] LoschScriptParser.Type_nameContext context)
    {
        if (context.identifier_atom() != null)
            return Visit(context.identifier_atom());

        if (context.builtin_type_alias() != null)
        {
            string dotNetTypeName = $"System.{context.GetText() switch
            {
                "int8" => "SByte",
                "uint8" => "Byte",
                "int16" => "Int16",
                "uint16" => "UInt16",
                "int32" => "Int32",
                "uint32" => "UInt32",
                "int64" => "Int64",
                "uint64" => "UInt64",
                "float32" => "Single",
                "float64" => "Double",
                "decimal" => "Decimal",
                "native" => "IntPtr",
                "unative" => "UIntPtr",
                "bool" => "Boolean",
                "string" => "String",
                "char" => "Char",
                _ => "Object"
            }}";

            return Helpers.ResolveTypeName(
                dotNetTypeName,
                context.Start.Line,
                context.Start.Column,
                context.GetText().Length);
        }

        if (context.Bar() != null)
        {
            UnionValue union = new(null, context.type_name().Select(VisitType_name).ToArray());
            CurrentMethod.CurrentUnion = union;

            if (union.AllowedTypes.Distinct().Count() < union.AllowedTypes.Length)
            {
                EmitWarningMessage(
                    context.Start.Line,
                    context.Start.Column,
                    context.GetText().Length,
                    LS0047_UnionTypeDuplicate,
                    "The union type contains duplicate cases.");
            }

            return union.GetType();
        }

        // TODO: Implement the other types
        return typeof(object);
    }

    public override Type VisitTuple_expression([NotNull] LoschScriptParser.Tuple_expressionContext context)
    {
        List<Type> types = new();

        for (int i = 0; i < context.expression().Length; i++)
            types.Add(Visit(context.expression()[i]));

        // If more than 8 tuple items are specified, split the tuples into multiple ones
        // This stupid algorithm took AGES to create...

        List<Type> _types = types.ToList();
        List<string> _intermediateTuples = new();

        string typeId = $"System.ValueTuple`{Math.Min(_types.Count, 8)}[";

        for (int k = 0; k < types.Count; k += 7)
        {
            if (_types.Count <= 7)
            {
                for (int i = 0; i < _types.Count - 1; i++)
                {
                    string _middlePart = $"[{_types[i].AssemblyQualifiedName}],";

                    if (_intermediateTuples.Any())
                        _intermediateTuples[^1] += _middlePart;

                    typeId += _middlePart;
                }

                string _endPart = $"[{_types.Last().AssemblyQualifiedName}]]";

                if (_intermediateTuples.Any())
                    _intermediateTuples[^1] += _endPart;

                typeId += _endPart;
                break;
            }

            Type[] proper = _types.ToArray()[(k * 8)..7];
            for (int j = 0; j < proper.Length; j++)
                typeId += $"[{proper[j].AssemblyQualifiedName}],";

            string imTupleStart = $"[System.ValueTuple`{Math.Min(_types.Count - 7, 8)}[";

            typeId += imTupleStart;
            _intermediateTuples.Add(imTupleStart);

            _types.RemoveRange(k, 7);
        }

        if (types.Count > 7)
            typeId += "]]";

        for (int i = 0; i < _intermediateTuples.Count; i++)
            _intermediateTuples[i] = _intermediateTuples[i][1..];

        List<Type> imTuples = _intermediateTuples.Select(Type.GetType).ToList();
        foreach (Type t in imTuples)
        {
            ConstructorInfo imConstructor = t.GetConstructor(t.GenericTypeArguments);
            CurrentMethod.IL.Emit(OpCodes.Newobj, imConstructor);
        }

        Type _tupleType = Type.GetType(typeId);

        ConstructorInfo _c = _tupleType.GetConstructor(_tupleType.GenericTypeArguments);
        CurrentMethod.IL.Emit(OpCodes.Newobj, _c);
        return _tupleType;
    }

    public override Type VisitArray_expression([NotNull] LoschScriptParser.Array_expressionContext context)
    {
        Type arrayType = Visit(context.expression()[0]);
        CurrentMethod.IL.Emit(OpCodes.Pop);

        EmitLdcI4(CurrentMethod.IL, context.expression().Length);
        CurrentMethod.IL.Emit(OpCodes.Newarr, arrayType);

        int index = 0;
        foreach (IParseTree tree in context.expression())
        {
            CurrentMethod.IL.Emit(OpCodes.Dup);
            EmitLdcI4(CurrentMethod.IL, index++);
            Type t = Visit(tree);

            if (t != arrayType)
            {
                EmitErrorMessage(context.expression()[index - 1].Start.Line, context.expression()[index - 1].Start.Column, context.expression()[index - 1].Start.Text.Length, LS0041_ListItemsHaveDifferentTypes, "An array or list can only contain one type of value.");
                return arrayType.MakeArrayType();
            }

            CurrentMethod.IL.Emit(OpCodes.Stelem, t);
        }

        return arrayType.MakeArrayType();
    }

    public override Type VisitEmpty_atom([NotNull] LoschScriptParser.Empty_atomContext context)
    {
        CurrentMethod.IL.Emit(OpCodes.Ldnull);
        return null;
    }

    public override Type VisitIndex_expression([NotNull] LoschScriptParser.Index_expressionContext context)
    {
        Type t1 = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (t1 == typeof(string) && t2 == typeof(Range))
        {
            // TODO: Implement the conversion from Range to two ints
            CurrentMethod.IL.EmitCall(OpCodes.Call, typeof(string).GetMethod("Substring", new Type[] { typeof(int), typeof(int) }), null);
            return typeof(string);
        }

        MethodInfo getChars = t1.GetMethod("get_Chars", new Type[] { t2 });
        if (getChars != null && t2 == typeof(int))
        {
            CurrentMethod.IL.EmitCall(OpCodes.Callvirt, getChars, null);
            return typeof(char);
        }

        MethodInfo indexer = t1.GetMethod("get_Item", new Type[] { t2 });
        if (indexer != null)
        {
            CurrentMethod.IL.EmitCall(OpCodes.Callvirt, indexer, null);
            return indexer.ReturnType;
        }

        // Array Index
        if (t2 == typeof(int))
        {
            CurrentMethod.IL.Emit(OpCodes.Ldelem, t1.GetEnumeratedType());
            return t1.GetEnumeratedType();
        }

        return t1;
    }

    public override Type VisitRange_expression([NotNull] LoschScriptParser.Range_expressionContext context)
    {
        return Visit(context.range());
    }

    public override Type VisitRange([NotNull] LoschScriptParser.RangeContext context)
    {
        if (context.index().Length == 2)
        {
            Visit(context.index()[0]);
            Visit(context.index()[1]);

            CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(Range).GetConstructor(new Type[] { typeof(Index), typeof(Index) }));

            return typeof(Range);
        }

        if (context.index().Length == 0)
        {
            CurrentMethod.IL.EmitCall(OpCodes.Call, typeof(Range).GetMethod("get_All", Type.EmptyTypes), null);
            return typeof(Range);
        }

        if (context.GetText().EndsWith(".."))
        {
            Visit(context.index()[0]);
            CurrentMethod.IL.EmitCall(OpCodes.Call, typeof(Range).GetMethod("StartAt", new Type[] { typeof(Index) }), null);
            return typeof(Range);
        }

        Visit(context.index()[0]);
        CurrentMethod.IL.EmitCall(OpCodes.Call, typeof(Range).GetMethod("EndAt", new Type[] { typeof(Index) }), null);

        return typeof(Range);
    }

    public override Type VisitIndex([NotNull] LoschScriptParser.IndexContext context)
    {
        Visit(context.integer_atom());

        EmitLdcI4(CurrentMethod.IL, context.Caret() == null ? 0 : 1);

        CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(Index).GetConstructor(new Type[] { typeof(int), typeof(bool) }));
        return typeof(Index);
    }

    public override Type VisitArray_element_assignment([NotNull] LoschScriptParser.Array_element_assignmentContext context)
    {
        Type arrayType = Visit(context.expression()[0]);

        Type index = Visit(context.expression()[1]);

        if (index == typeof(int))
        {
            Type t = Visit(context.expression()[2]);

            if (t != arrayType.GetEnumeratedType())
            {
                EmitErrorMessage(context.expression()[2].Start.Line, context.expression()[2].Start.Column, context.expression()[2].Start.Text.Length, LS0041_ListItemsHaveDifferentTypes, "The type of the new value of the specified array item does not match the type of the old one.");
                return t;
            }

            CurrentMethod.IL.Emit(OpCodes.Stelem, t);

            return t;
        }

        EmitErrorMessage(context.expression()[1].Start.Line, context.expression()[1].Start.Column, context.expression()[1].Start.Text.Length, LS0042_ArrayElementAssignmentIndexExpressionNotInteger, "The index expression has to be of type Int32.");

        return arrayType.GetEnumeratedType();
    }

    public override Type VisitWhile_loop([NotNull] LoschScriptParser.While_loopContext context)
    {
        Type t = Visit(context.expression().First());
        Type tReturn = null;

        if (t == typeof(int))
        {
            // Build the array of return values
            // (A for loop returns an array containing the return
            // values of every iteration of the loop)
            // The length of the array is already on the stack
            CurrentMethod.IL.Emit(OpCodes.Newarr, typeof(object));

            // A local that saves the returning array
            LocalBuilder returnBuilder = CurrentMethod.IL.DeclareLocal(typeof(object).MakeArrayType());

            CurrentMethod.Locals.Add((GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex++), returnBuilder, false, CurrentMethod.LocalIndex++, new(null, typeof(int))));

            EmitStloc(CurrentMethod.IL, CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);

            if (Context.Configuration.Configuration == Losch.LoschScript.Configuration.Configuration.Debug && createAssembly)
                Helpers.SetLocalSymInfo(returnBuilder, GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1));

            Label loop = CurrentMethod.IL.DefineLabel();
            Label start = CurrentMethod.IL.DefineLabel();

            LocalBuilder lb = CurrentMethod.IL.DeclareLocal(typeof(int));
            CurrentMethod.Locals.Add((GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex++), lb, false, CurrentMethod.LocalIndex++, new(null, typeof(int))));

            if (Context.Configuration.Configuration == Losch.LoschScript.Configuration.Configuration.Debug && createAssembly)
                Helpers.SetLocalSymInfo(lb, (GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex - 1)));

            CurrentMethod.IL.Emit(OpCodes.Br, loop);

            CurrentMethod.IL.MarkLabel(start);

            if (context.code_block() == null)
            {
                // Save the return value of the current iteration to the returning array

                // Array
                CurrentMethod.IL.Emit(OpCodes.Ldloc, CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);
                // Index
                CurrentMethod.IL.Emit(OpCodes.Ldloc, CurrentMethod.Locals.Where(l => l.Name == GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex - 1)).First().Index + 1);

                tReturn = Visit(context.expression().Last());

                if (tReturn == typeof(void))
                {
                    CurrentMethod.IL.Emit(OpCodes.Ldnull);
                    CurrentMethod.IL.Emit(OpCodes.Stelem, typeof(object));
                }
                else
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, tReturn);
                    CurrentMethod.IL.Emit(OpCodes.Stelem, typeof(object));
                }
            }
            else
            {
                foreach (IParseTree tree in context.code_block().expression()[..^1])
                    Visit(tree);

                // Save the return value of the current iteration to the returning array

                // Array
                CurrentMethod.IL.Emit(OpCodes.Ldloc, CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);
                // Index
                CurrentMethod.IL.Emit(OpCodes.Ldloc, CurrentMethod.Locals.Where(l => l.Name == GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex - 1)).First().Index + 1);

                tReturn = Visit(context.code_block().expression().Last());

                if (tReturn == typeof(void))
                {
                    CurrentMethod.IL.Emit(OpCodes.Ldnull);
                    CurrentMethod.IL.Emit(OpCodes.Stelem, typeof(object));
                }
                else
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, tReturn);
                    CurrentMethod.IL.Emit(OpCodes.Stelem, typeof(object));
                }
            }

            CurrentMethod.IL.Emit(OpCodes.Ldloc, CurrentMethod.Locals.Where(l => l.Name == GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex - 1)).First().Index + 1);
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)1);
            CurrentMethod.IL.Emit(OpCodes.Add);
            EmitStloc(CurrentMethod.IL, CurrentMethod.Locals.Where(l => l.Name == GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex - 1)).First().Index + 1);


            CurrentMethod.IL.MarkLabel(loop);

            CurrentMethod.IL.Emit(OpCodes.Ldloc, CurrentMethod.Locals.Where(l => l.Name == GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex - 1)).First().Index + 1);
            Visit(context.expression().First());
            CurrentMethod.IL.Emit(OpCodes.Blt, start);

            CurrentMethod.IL.Emit(OpCodes.Ldloc, CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);

            return typeof(object[]);
        }

        if (t == typeof(bool))
        {
            CurrentMethod.IL.Emit(OpCodes.Pop);

            CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(List<object>).GetConstructor(Type.EmptyTypes));

            // A local that saves the returning list
            LocalBuilder returnBuilder = CurrentMethod.IL.DeclareLocal(typeof(List<object>));

            CurrentMethod.Locals.Add((GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex++), returnBuilder, false, CurrentMethod.LocalIndex++, new(null, typeof(List<string>))));

            EmitStloc(CurrentMethod.IL, CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);

            if (Context.Configuration.Configuration == Losch.LoschScript.Configuration.Configuration.Debug && createAssembly)
                Helpers.SetLocalSymInfo(returnBuilder, GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1));

            Label loop = CurrentMethod.IL.DefineLabel();
            Label start = CurrentMethod.IL.DefineLabel();

            CurrentMethod.IL.Emit(OpCodes.Br, loop);

            CurrentMethod.IL.MarkLabel(start);

            if (context.code_block() == null)
            {
                CurrentMethod.IL.Emit(OpCodes.Ldloc, CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);
                tReturn = Visit(context.expression().Last());

                if (tReturn == typeof(void))
                {
                    CurrentMethod.IL.Emit(OpCodes.Ldnull);
                }
                else
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, tReturn);
                }

                CurrentMethod.IL.EmitCall(OpCodes.Callvirt, typeof(List<object>).GetMethod("Add", new Type[] { typeof(object) }), null);
            }
            else
            {
                foreach (IParseTree tree in context.code_block().expression()[..^1])
                    Visit(tree);

                CurrentMethod.IL.Emit(OpCodes.Ldloc, CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);
                tReturn = Visit(context.code_block().expression().Last());

                if (tReturn == typeof(void))
                {
                    CurrentMethod.IL.Emit(OpCodes.Ldnull);
                }
                else
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, tReturn);
                }

                CurrentMethod.IL.EmitCall(OpCodes.Callvirt, typeof(List<object>).GetMethod("Add", new Type[] { typeof(object) }), null);
            }

            CurrentMethod.IL.MarkLabel(loop);

            Visit(context.expression().First());
            CurrentMethod.IL.Emit(OpCodes.Brtrue, start);

            CurrentMethod.IL.Emit(OpCodes.Ldloc, CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);

            return typeof(List<object>);
        }

        EmitWarningMessage(
            context.expression().First().Start.Line,
            context.expression().First().Start.Column,
            context.expression().First().Start.Text.Length,
            LS0043_PossiblyUnintentionalInfiniteLoop,
            "The condition of the while loop is not a boolean. This loop will run indefinetly.");

        CurrentMethod.IL.Emit(OpCodes.Pop);

        Label infiniteLoop = CurrentMethod.IL.DefineLabel();
        CurrentMethod.IL.MarkLabel(infiniteLoop);

        if (context.code_block() == null)
            tReturn = Visit(context.expression().Last());
        else
        {
            foreach (IParseTree tree in context.code_block().expression()[..^1])
                Visit(tree);

            tReturn = Visit(context.code_block().expression().Last());
        }

        CurrentMethod.IL.Emit(OpCodes.Br, infiniteLoop);

        return typeof(object[]);
    }

    public override Type VisitLoop_expression([NotNull] LoschScriptParser.Loop_expressionContext context)
    {
        if (context.Identifier().Length > 2)
        {
            EmitErrorMessage(
                context.Identifier()[2].Symbol.Line,
                context.Identifier()[2].Symbol.Column,
                context.Identifier()[2].GetText().Length,
                LS0049_InvalidForLoopSyntax,
                "The loop syntax is invalid, it can contain at most 3 linked expressions.");

            return null;
        }

        return null;
    }

    public override Type VisitPlaceholder([NotNull] LoschScriptParser.PlaceholderContext context)
    {
        CurrentMethod.IL.Emit(OpCodes.Nop);
        return null;
    }

    public override Type VisitThis_atom([NotNull] LoschScriptParser.This_atomContext context)
    {
        CurrentMethod.IL.Emit(OpCodes.Ldarg_S, (byte)0);
        return TypeContext.Current.Builder;
    }
}