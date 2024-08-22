using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Dassie.CLI;
using Dassie.CLI.Helpers;
using Dassie.Core;
using Dassie.Meta;
using Dassie.Parser;
using Dassie.Runtime;
using Dassie.Text;
using Dassie.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Color = Dassie.Text.Color;
using static Dassie.CLI.Helpers.TypeHelpers;

namespace Dassie.CodeGeneration;

internal class Visitor : DassieParserBaseVisitor<Type>
{
    private readonly ExpressionEvaluator eval;

    public Visitor(ExpressionEvaluator evaluator) => eval = evaluator;

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

        return typeof(void);
    }

    public override Type VisitFile_body([NotNull] DassieParser.File_bodyContext context)
    {
        if (context.top_level_statements() != null)
        {
            Visit(context.top_level_statements());
            return typeof(void);
        }

        Visit(context.full_program());

        return typeof(void);
    }

    public override Type VisitFull_program([NotNull] DassieParser.Full_programContext context)
    {
        foreach (IParseTree type in context.type())
            Visit(type);

        return typeof(void);
    }

    public override Type VisitType([NotNull] DassieParser.TypeContext context)
    {
        VisitType(context, null);
        return typeof(void);
    }

    private void VisitType(DassieParser.TypeContext context, TypeBuilder enclosingType)
    {
        if (context.Identifier().GetText().Length + (CurrentFile.ExportedNamespace ?? "").Length > 1024)
        {
            EmitErrorMessage(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetText().Length,
                DS0073_TypeNameTooLong,
                "A type name cannot be longer than 1024 characters.");

            return;
        }

        Type parent = typeof(object);
        List<Type> interfaces = new();

        if (context.type_kind().Val() != null)
            parent = typeof(ValueType);

        if (context.inheritance_list() != null)
        {
            List<Type> inherited = CliHelpers.GetInheritedTypes(context.inheritance_list());

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
                CliHelpers.GetTypeAttributes(context.type_kind(), context.type_access_modifier(), context.nested_type_access_modifier(), context.type_special_modifier(), false),
                parent);
        }
        else
        {
            tb = enclosingType.DefineNestedType(
                context.Identifier().GetText(),
                CliHelpers.GetTypeAttributes(context.type_kind(), context.type_access_modifier(), context.nested_type_access_modifier(), context.type_special_modifier(), true),
                parent);
        }

        foreach (Type _interface in interfaces)
            tb.AddInterfaceImplementation(_interface);

        TypeContext tc = new()
        {
            Builder = tb,
            FullName = tb.FullName
        };

        tc.FilesWhereDefined.Add(CurrentFile.Path);

        foreach (DassieParser.TypeContext nestedType in context.type_block().type())
            VisitType(nestedType, tb);

        foreach (DassieParser.Type_memberContext member in context.type_block().type_member())
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
            ToolTip = TooltipGenerator.Type(tb.CreateTypeInfo(), true, true),
            IsNavigationTarget = true
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
                    DS0054_WrongFieldType,
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

    public override Type VisitBlock_expression([NotNull] DassieParser.Block_expressionContext context)
    {
        return Visit(context.code_block());
    }

    private void HandleConstructor(DassieParser.Type_memberContext context)
    {
        CallingConventions callingConventions = CallingConventions.HasThis;

        if (context.member_special_modifier().Any(m => m.Static() != null))
            callingConventions = CallingConventions.Standard;

        var paramTypes = ResolveParameterList(context.parameter_list());

        MethodAttributes attribs = CliHelpers.GetMethodAttributes(context.member_access_modifier(), context.member_oop_modifier(), context.member_special_modifier());
        if (attribs.HasFlag(MethodAttributes.Virtual))
            attribs &= ~MethodAttributes.Virtual;

        ConstructorBuilder cb = TypeContext.Current.Builder.DefineConstructor(attribs, callingConventions, paramTypes.Select(p => p.Type).ToArray());

        CurrentMethod = new()
        {
            ConstructorBuilder = cb,
            IL = cb.GetILGenerator()
        };

        CurrentMethod.FilesWhereDefined.Add(CurrentFile.Path);
        TypeContext.Current.ConstructorContexts.Add(CurrentMethod);

        foreach (var param in paramTypes)
        {
            ParameterBuilder pb = cb.DefineParameter(
                CurrentMethod.ParameterIndex++,
                CliHelpers.GetParameterAttributes(param.Context.parameter_modifier(), param.Context.Equals() != null),
                param.Context.Identifier().GetText());

            CurrentMethod.Parameters.Add(new(param.Context.Identifier().GetText(), param.Type, pb, CurrentMethod.ParameterIndex, new(), param.Context.Var() != null));
        }

        if (CurrentMethod.ConstructorBuilder.IsStatic)
        {
            foreach (var param in CurrentMethod.Parameters)
                param.Index--;
        }

        HandleFieldInitializersAndDefaultConstructor();

        Type t = Visit(context.expression());

        if (t != typeof(void))
        {
            EmitErrorMessage(
                context.Equals().Symbol.Line,
                context.Equals().Symbol.Column,
                context.expression().Start.Column - context.Equals().Symbol.Column,
                DS0093_ConstructorReturnsValue,
                $"Expected expression of type 'null' but found type '{t.FullName}'.");
        }

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
            ToolTip = TooltipGenerator.Constructor(TypeContext.Current.Builder, _params),
            NavigationTargetKind = Fragment.NavigationKind.Constructor
        });
    }

    public override Type VisitAnonymous_function_expression([NotNull] DassieParser.Anonymous_function_expressionContext context)
    {
        TypeBuilder closureType = TypeContext.Current.Builder.DefineNestedType(
            GetClosureTypeName(CurrentMethod.ClosureIndex),
            TypeAttributes.NestedPrivate | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass);

        TypeContext.Current.Children.Add(new()
        {
            Builder = closureType
        });

        foreach (var local in CurrentMethod.Locals)
            closureType.DefineField(local.Name, local.Builder.LocalType, FieldAttributes.Public);

        Type ret = typeof(object); // TODO: Add type inference

        if (context.type_name() != null)
            ret = CliHelpers.ResolveTypeName(context.type_name());

        var paramList = ResolveParameterList(context.parameter_list());

        MethodBuilder invokeMethod = closureType.DefineMethod(
            "Invoke",
            MethodAttributes.Assembly | MethodAttributes.HideBySig,
            CallingConventions.HasThis,
            ret,
            paramList.Select(p => p.Type).ToArray());

        MethodContext current = CurrentMethod;

        CurrentMethod = new()
        {
            Builder = invokeMethod,
            IL = invokeMethod.GetILGenerator()
        };

        Visit(context.expression());
        CurrentMethod.IL.Emit(OpCodes.Ret);

        closureType.CreateType();

        CurrentMethod = current;
        CurrentMethod.ClosureIndex++;
        return ret;
    }

    public override Type VisitType_member([NotNull] DassieParser.Type_memberContext context)
    {
        if (Context.Configuration.Verbosity >= 1)
            EmitBuildLogMessage($"    Generating code for '{TypeContext.Current.Builder.FullName}::{context.Identifier().GetText()}'...");

        if (context.Identifier().GetText() == TypeContext.Current.Builder.Name)
        {
            // Defer constructors for field initializers
            TypeContext.Current.Constructors.Add(context);

            return typeof(void);
        }

        //CliHelpers.CreateFakeMethod();
        //Type _tReturn = Visit(context.expression());

        if (context.Var() != null && context.parameter_list() != null)
        {
            EmitErrorMessage(
                context.Var().Symbol.Line,
                context.Var().Symbol.Column,
                context.Var().GetText().Length,
                DS0083_InvalidVarModifier,
                "The modifier 'var' cannot be used on methods.");
        }

        Type _tReturn = typeof(object);

        if (context.parameter_list() != null || _tReturn == typeof(void))
        {
            Type tReturn = _tReturn; // TODO: Add proper type inference

            if (context.type_name() != null)
                tReturn = CliHelpers.ResolveTypeName(context.type_name());

            CallingConventions callingConventions = CallingConventions.HasThis;

            if (context.member_special_modifier().Any(m => m.Static() != null) || (TypeContext.Current.Builder.IsSealed && TypeContext.Current.Builder.IsAbstract))
                callingConventions = CallingConventions.Standard;

            var paramTypes = ResolveParameterList(context.parameter_list());

            MethodAttributes attrib = CliHelpers.GetMethodAttributes(
                    context.member_access_modifier(),
                    context.member_oop_modifier(),
                    context.member_special_modifier());

            if (attrib.HasFlag(MethodAttributes.PinvokeImpl))
            {
                // TODO: Implement P/Invoke methods
                //MethodBuilder pInvokeMethod = TypeContext.Current.Builder.DefinePInvokeMethod();

                return tReturn;
            }

            MethodBuilder mb = TypeContext.Current.Builder.DefineMethod(
                context.Identifier().GetText(),
                attrib,
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
                    CurrentMethod.ParameterIndex++ + 1, // Add 1 so parameter indices start at 1 -> 0 is always the current instance of the containing type
                    CliHelpers.GetParameterAttributes(param.Context.parameter_modifier(), param.Context.Equals() != null),
                    param.Context.Identifier().GetText());

                CurrentMethod.Parameters.Add(new(param.Context.Identifier().GetText(), param.Type, pb, CurrentMethod.ParameterIndex, new(), param.Context.Var() != null));
            }

            if (CurrentMethod.Builder.IsStatic)
            {
                foreach (var param in CurrentMethod.Parameters)
                    param.Index--;
            }

            object ctx = context.expression();
            if (ctx is DassieParser.Newlined_expressionContext)
            {
                while (ctx is DassieParser.Newlined_expressionContext expr)
                    ctx = expr.expression();
            }

            bool allowTailCall = ctx is DassieParser.Member_access_expressionContext or DassieParser.Full_identifier_member_access_expressionContext;
            CurrentMethod.EmitTailCall = allowTailCall;
            CurrentMethod.AllowTailCallEmission = allowTailCall || ctx is DassieParser.Block_expressionContext;

            _tReturn = Visit(context.expression());

            if (_tReturn != tReturn)
            {
                if (tReturn == typeof(object))
                {
                    if (_tReturn.IsValueType)
                        CurrentMethod.IL.Emit(OpCodes.Box, _tReturn);
                }
                else
                {
                    EmitErrorMessage(
                        context.expression().Start.Line,
                        context.expression().Start.Column,
                        context.expression().GetText().Length,
                        DS0053_WrongReturnType,
                        $"Expected expression of type '{tReturn.FullName}', but got type '{_tReturn.FullName}'.");
                }
            }

            CurrentMethod.IL.Emit(OpCodes.Ret);

            CurrentFile.FunctionParameterConstraints.TryGetValue(context.Identifier().GetText(), out Dictionary<string, string> constraintsForCurrentFunction);
            constraintsForCurrentFunction ??= [];

            List<Parameter> _params = [];
            foreach (var param in CurrentMethod.Parameters)
            {
                constraintsForCurrentFunction.TryGetValue(param.Name, out string constraint);

                _params.Add(new()
                {
                    Name = param.Name,
                    Type = param.Type,
                    Constraint = constraint
                });
            }

            CurrentFile.Fragments.Add(new()
            {
                Color = Color.Function,
                Line = context.Identifier().Symbol.Line,
                Column = context.Identifier().Symbol.Column,
                Length = context.Identifier().GetText().Length,
                ToolTip = TooltipGenerator.Function(context.Identifier().GetText(), tReturn, _params.ToArray()),
                IsNavigationTarget = true
            });

            if (context.attribute() != null)
            {
                Type attribType = null;

                if (context.attribute().type_name().GetText() == "EntryPoint")
                    attribType = typeof(EntryPointAttribute);
                else
                    attribType = CliHelpers.ResolveTypeName(context.attribute().type_name());

                if (attribType == typeof(EntryPointAttribute))
                {
                    if (Context.EntryPointIsSet)
                    {
                        EmitErrorMessage(
                            context.attribute().Start.Line,
                            context.attribute().Start.Column,
                            context.attribute().GetText().Length,
                            DS0055_MultipleEntryPoints,
                            "Only one function can be declared as an entry point.");
                    }

                    if (!mb.IsStatic)
                    {
                        EmitErrorMessage(
                            context.Identifier().Symbol.Line,
                            context.Identifier().Symbol.Column,
                            context.Identifier().GetText().Length,
                            DS0035_EntryPointNotStatic,
                            "The application entry point must be static.");
                    }

                    Context.EntryPointIsSet = true;

                    Context.EntryPoint = mb;

                    CurrentMethod.Builder.SetCustomAttribute(new(typeof(EntryPointAttribute).GetConstructor(Type.EmptyTypes), Array.Empty<object>()));
                }

                else if (attribType != null)
                {
                    // TODO: Support attributes on functions
                    //CurrentMethod.Builder.SetCustomAttribute(cab);
                }
            }

            return typeof(void);
        }

        CliHelpers.CreateFakeMethod();

        Type _type = typeof(object);

        if (context.expression() != null)
            _type = Visit(context.expression());

        Type type = _type;

        if (context.type_name() != null)
            type = CliHelpers.ResolveTypeName(context.type_name());

        FieldBuilder fb = TypeContext.Current.Builder.DefineField(
            context.Identifier().GetText(),
            type,
            CliHelpers.GetFieldAttributes(context.member_access_modifier(), context.member_oop_modifier(), context.member_special_modifier(), context.Val() != null));

        TypeContext.Current.Fields.Add(new(context.Identifier().GetText(), fb, default));

        if (context.expression() != null)
            TypeContext.Current.FieldInitializers.Add((fb, context.expression()));

        CurrentFile.Fragments.Add(new()
        {
            Color = Color.Field,
            Column = context.Identifier().Symbol.Column,
            Line = context.Identifier().Symbol.Line,
            Length = context.Identifier().GetText().Length,
            ToolTip = TooltipGenerator.Field(fb),
            IsNavigationTarget = true
        });

        return typeof(void);
    }

    private (Type Type, DassieParser.ParameterContext Context)[] ResolveParameterList(DassieParser.Parameter_listContext paramList)
    {
        if (paramList == null)
            return Array.Empty<(Type, DassieParser.ParameterContext)>();

        List<(Type, DassieParser.ParameterContext)> types = new();

        foreach (var param in paramList.parameter())
            types.Add((ResolveParameter(param), param));

        return types.ToArray();
    }

    private Type ResolveParameter(DassieParser.ParameterContext param)
    {
        Type t = typeof(object);

        if (param.type_name() != null)
        {
            t = CliHelpers.ResolveTypeName(param.type_name());

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

    public override Type VisitBasic_import([NotNull] DassieParser.Basic_importContext context)
    {
        foreach (var id in context.full_identifier())
        {
            string ns = id.GetText();

            if (Type.GetType(ns) != null)
            {
                Type t = Type.GetType(ns);

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

    public override Type VisitAlias([NotNull] DassieParser.AliasContext context)
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

    public override Type VisitTop_level_statements([NotNull] DassieParser.Top_level_statementsContext context)
    {
        if (Context.Files.Count > 0)
        {
            if ((context.expression().Length == 0 && Context.Files.Count < 2) || (Context.Files.Last() == CurrentFile && Context.ShouldThrowDS0027))
            {
                EmitErrorMessage(0, 0, context.GetText().Length, DS0027_EmptyProgram, "The program does not contain any executable code.");
                return typeof(void);
            }

            else if (context.expression().Length == 0)
                Context.ShouldThrowDS0027 = true;
        }

        if (context.children == null)
            return typeof(void);

        TypeBuilder tb = Context.Module.DefineType($"{(string.IsNullOrEmpty(CurrentFile.ExportedNamespace) ? "" : $"{CurrentFile.ExportedNamespace}.")}Program");

        TypeContext tc = new()
        {
            Builder = tb
        };

        if (Context.Configuration.Verbosity >= 1)
            EmitBuildLogMessage($"    Generating code for '{tb.FullName}::Main'...");

        tc.FilesWhereDefined.Add(CurrentFile.Path);

        Context.EntryPointIsSet = true;
        CustomAttributeBuilder entryPointAttribute = new(typeof(EntryPointAttribute).GetConstructor(Type.EmptyTypes), Array.Empty<object>());

        MethodBuilder mb = tb.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(int), new Type[] { typeof(string[]) });
        mb.SetCustomAttribute(entryPointAttribute);

        ILGenerator il = mb.GetILGenerator();
        MethodContext mc = new()
        {
            Builder = mb,
            IL = il
        };

        mc.Parameters.Add(new("args", typeof(string[]), mb.DefineParameter(0, ParameterAttributes.None, "args"), 0, default, false));

        mc.FilesWhereDefined.Add(CurrentFile.Path);

        tc.Methods.Add(mc);

        Context.Types.Add(tc);

        foreach (IParseTree child in context.children.Take(context.children.Count - 1))
        {
            Type _t = Visit(child);

            if (_t != typeof(void) && !CurrentMethod.SkipPop)
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
                DS0050_ExpectedIntegerReturnValue,
                $"Expected expression of type 'int32' or 'void', but got type '{ret.FullName}'.",
                tip: "You may use the function 'ignore' to discard a value and return 'void'.");

            return ret;
        }

        if (ret != typeof(int) && ret != null)
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)0);

        CurrentMethod.IL.Emit(OpCodes.Ret);

        Context.EntryPoint = mb;

        tb.CreateType();
        return ret;
    }

    public override Type VisitExpression_atom([NotNull] DassieParser.Expression_atomContext context)
    {
        return Visit(context.expression());
    }

    //public override Type VisitPrefix_newlined_expression([NotNull] DassieParser.Prefix_newlined_expressionContext context)
    //{
    //    return Visit(context.expression());
    //}

    public override Type VisitNewlined_expression([NotNull] DassieParser.Newlined_expressionContext context)
    {
        return Visit(context.expression());
    }

    public override Type VisitEquality_expression([NotNull] DassieParser.Equality_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        EnsureBoolean(t, throwError: false);
        
        Type t2 = Visit(context.expression()[1]);
        EnsureBoolean(t2, throwError: false);
        
        MethodInfo op_eq = t.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);
        MethodInfo op_ineq = t.GetMethod("op_Inequality", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if ((op_eq == null && op_ineq == null) || (CliHelpers.IsNumericType(t) && CliHelpers.IsNumericType(t2)))
        {
            if ((CliHelpers.IsNumericType(t) && CliHelpers.IsNumericType(t2)) || (IsBoolean(t) && IsBoolean(t2)))
            {
                if (CliHelpers.IsFloatingPointType(t) && !CliHelpers.IsFloatingPointType(t2))
                {
                    CurrentMethod.IL.Emit(OpCodes.Conv_R8);
                }
                else if (CliHelpers.IsFloatingPointType(t2) && !CliHelpers.IsFloatingPointType(t))
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
        }

        if (context.op.Text == "==")
        {
            if (op_eq == null)
            {
                EmitErrorMessage(
                        context.op.Line,
                        context.op.Column,
                        context.op.Text.Length,
                        DS0036_ArithmeticError,
                        $"The type '{t.FullName}' does not implement an equality operation with the operand type '{t2.FullName}'.",
                        Path.GetFileName(CurrentFile.Path));

                return typeof(bool);
            }

            CurrentMethod.IL.EmitCall(OpCodes.Call, op_eq, null);
        }
        else
        {
            if (op_ineq == null)
            {
                EmitErrorMessage(
                        context.op.Line,
                        context.op.Column,
                        context.op.Text.Length,
                        DS0036_ArithmeticError,
                        $"The type '{t.FullName}' does not implement an inequality operation with the operand type '{t2.FullName}'.",
                        Path.GetFileName(CurrentFile.Path));

                return typeof(bool);
            }

            CurrentMethod.IL.EmitCall(OpCodes.Call, op_ineq, null);
        }

        return typeof(bool);
    }

    public override Type VisitComparison_expression([NotNull] DassieParser.Comparison_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (CliHelpers.IsNumericType(t) && CliHelpers.IsNumericType(t2))
        {
            if (CliHelpers.IsFloatingPointType(t) && !CliHelpers.IsFloatingPointType(t2))
            {
                CurrentMethod.IL.Emit(OpCodes.Conv_R8);
            }
            else if (CliHelpers.IsFloatingPointType(t2) && !CliHelpers.IsFloatingPointType(t))
            {
                CurrentMethod.IL.Emit(OpCodes.Pop);
                CurrentMethod.IL.Emit(OpCodes.Conv_R8);
                Visit(context.expression()[1]);
            }

            if (context.op.Text == "<" || context.op.Text == ">=")
                EmitClt(t);
            else
                EmitCgt(t);

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
                    DS0036_ArithmeticError,
                    $"The type '{t.FullName}' does not implement a comparison operation with the operand type '{t2.FullName}'.",
                    Path.GetFileName(CurrentFile.Path));

            return typeof(bool);
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return typeof(bool);
    }

    //public override Type VisitUnary_negation_expression([NotNull] DassieParser.Unary_negation_expressionContext context)
    //{
    //    Type t = Visit(context.expression());

    //    if (Helpers.IsNumericType(t))
    //    {
    //        CurrentMethod.IL.Emit(OpCodes.Neg);
    //        return t;
    //    }

    //    MethodInfo op = t.GetMethod("op_UnaryNegation", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t }, null);

    //    if (op == null)
    //    {
    //        EmitErrorMessage(
    //                context.Minus().Symbol.Line,
    //                    context.Minus().Symbol.Column,
    //                    context.Minus().GetText().Length,
    //                DS0036_ArithmeticError,
    //                $"The type '{t.FullName}' does not implement the unary negation operation.",
    //                Path.GetFileName(CurrentFile.Path));

    //        return t;
    //    }

    //    CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

    //    return t;
    //}

    //public override Type VisitUnary_plus_expression([NotNull] DassieParser.Unary_plus_expressionContext context)
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
    //                DS0036_ArithmeticError,
    //                $"The type '{t.FullName}' does not implement the unary plus operation.",
    //                Path.GetFileName(CurrentFile.Path));

    //        return t;
    //    }

    //    CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

    //    return t;
    //}

    public override Type VisitLogical_negation_expression([NotNull] DassieParser.Logical_negation_expressionContext context)
    {
        Type t = Visit(context.expression());

        if (IsBoolean(t))
        {
            EnsureBoolean(t);
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)0);
            CurrentMethod.IL.Emit(OpCodes.Ceq);
            return typeof(bool);
        }

        MethodInfo op = t.GetMethod("op_LogicalNot", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Exclamation_Mark().Symbol.Line,
                context.Exclamation_Mark().Symbol.Column,
                context.Exclamation_Mark().GetText().Length,
                DS0002_MethodNotFound,
                $"The type '{t.FullName}' does not implement a logical negation operation.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    // TODO: Implement short-circuiting
    public override Type VisitLogical_and_expression([NotNull] DassieParser.Logical_and_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        EnsureBoolean(t, throwError: false);

        Type t2 = Visit(context.expression()[1]);
        EnsureBoolean(t2, throwError: false);

        if (IsBoolean(t) && IsBoolean(t2))
        {
            CurrentMethod.IL.Emit(OpCodes.And);
            return typeof(bool);
        }

        EmitErrorMessage(
            context.Double_Ampersand().Symbol.Line,
            context.Double_Ampersand().Symbol.Column,
            context.Double_Ampersand().GetText().Length,
            DS0002_MethodNotFound,
            $"The logical and operation is only supported by the type '{typeof(bool).FullName}'.",
            Path.GetFileName(CurrentFile.Path));

        return t;
    }

    // TODO: Implement short-circuiting
    public override Type VisitLogical_or_expression([NotNull] DassieParser.Logical_or_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        EnsureBoolean(t, throwError: false);

        Type t2 = Visit(context.expression()[1]);
        EnsureBoolean(t2, throwError: false);

        if (IsBoolean(t) && IsBoolean(t2))
        {
            CurrentMethod.IL.Emit(OpCodes.Or);
            return typeof(bool);
        }

        EmitErrorMessage(
            context.Double_Bar().Symbol.Line,
            context.Double_Bar().Symbol.Column,
            context.Double_Bar().GetText().Length,
            DS0002_MethodNotFound,
            $"The logical or operation is only supported by the type '{typeof(bool).FullName}'.",
            Path.GetFileName(CurrentFile.Path));

        return t;
    }

    public override Type VisitOr_expression([NotNull] DassieParser.Or_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        EnsureBoolean(t, throwError: false);

        Type t2 = Visit(context.expression()[1]);
        EnsureBoolean(t2, throwError: false);

        if (IsBoolean(t) && IsBoolean(t2))
        {
            CurrentMethod.IL.Emit(OpCodes.Or);
            return typeof(bool);
        }

        if (CliHelpers.IsNumericType(t))
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
                DS0002_MethodNotFound,
                $"The type '{t.FullName}' does not implement a bitwise or operation with the operand type '{t2.FullName}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitAnd_expression([NotNull] DassieParser.And_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        EnsureBoolean(t, throwError: false);

        Type t2 = Visit(context.expression()[1]);
        EnsureBoolean(t2, throwError: false);

        if (IsBoolean(t) && IsBoolean(t2))
        {
            CurrentMethod.IL.Emit(OpCodes.And);
            return typeof(bool);
        }

        if (CliHelpers.IsNumericType(t))
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
                DS0002_MethodNotFound,
                $"The type '{t.FullName}' does not implement a bitwise and operation with the operand type '{t2.FullName}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitXor_expression([NotNull] DassieParser.Xor_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (CliHelpers.IsNumericType(t))
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
                DS0002_MethodNotFound,
                $"The type '{t.FullName}' does not implement an exclusive or operation with the operand type '{t2.FullName}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitBitwise_complement_expression([NotNull] DassieParser.Bitwise_complement_expressionContext context)
    {
        Type t = Visit(context.expression());

        if (CliHelpers.IsNumericType(t))
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
                DS0002_MethodNotFound,
                $"The type '{t.FullName}' does not implement a complement operation.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitMultiply_expression([NotNull] DassieParser.Multiply_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (CliHelpers.IsNumericType(t))
        {
            EmitMul(t);
            return t;
        }

        MethodInfo op = t.GetMethod("op_Multiply", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Asterisk().Symbol.Line,
                context.Asterisk().Symbol.Column,
                context.Asterisk().GetText().Length,
                DS0002_MethodNotFound,
                $"The type '{t.FullName}' does not implement a multiplication operation with the operand type '{t2.FullName}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitDivide_expression([NotNull] DassieParser.Divide_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (CliHelpers.IsNumericType(t))
        {
            EmitDiv(t);
            return t;
        }

        MethodInfo op = t.GetMethod("op_Division", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Slash().Symbol.Line,
                context.Slash().Symbol.Column,
                context.Slash().GetText().Length,
                DS0002_MethodNotFound,
                $"The type '{t.FullName}' does not implement a division operation with the operand type '{t2.FullName}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitAddition_expression([NotNull] DassieParser.Addition_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (CliHelpers.IsNumericType(t) && CliHelpers.IsNumericType(t2))
        {
            EmitAdd(t);
            return t;
        }

        if (t == typeof(string) || t2 == typeof(string))
        {
            if (t2 != typeof(string))
            {
                CurrentMethod.LocalIndex++;

                LocalBuilder lb = CurrentMethod.IL.DeclareLocal(t2);
                SetLocalSymInfo(lb, $"<g>{CurrentMethod.LocalIndex}");

                EmitStloc(CurrentMethod.LocalIndex);
                EmitLdloca(CurrentMethod.LocalIndex);

                MethodInfo toString = t2.GetMethod("ToString", Array.Empty<Type>());
                CurrentMethod.IL.EmitCall(CliHelpers.GetCallOpCode(t2), toString, null);
            }
            else if (t != typeof(string))
            {
                // TODO: Fix this mess ASAP

                CurrentMethod.IL.Emit(OpCodes.Pop);

                LocalBuilder lb = CurrentMethod.IL.DeclareLocal(t);
                lb.SetLocalSymInfo($"<g>{CurrentMethod.LocalIndex + 1}");

                EmitStloc(++CurrentMethod.LocalIndex);
                EmitLdloca(CurrentMethod.LocalIndex);

                MethodInfo toString = t.GetMethod("ToString", Array.Empty<Type>());
                CurrentMethod.IL.EmitCall(CliHelpers.GetCallOpCode(t), toString, null);

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
                DS0002_MethodNotFound,
                $"The type '{t.FullName}' does not implement an addition operation with the operand type '{t2.FullName}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitSubtraction_expression([NotNull] DassieParser.Subtraction_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (CliHelpers.IsNumericType(t))
        {
            EmitSub(t);
            return t;
        }

        MethodInfo op = t.GetMethod("op_Subtraction", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Minus().Symbol.Line,
                context.Minus().Symbol.Column,
                context.Minus().GetText().Length,
                DS0002_MethodNotFound,
                $"The type '{t.FullName}' does not implement a subtraction operation with the operand type '{t2.FullName}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitRemainder_expression([NotNull] DassieParser.Remainder_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (CliHelpers.IsNumericType(t))
        {
            EmitRem(t);
            return t;
        }

        MethodInfo op = t.GetMethod("op_Modulus", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Percent().Symbol.Line,
                context.Percent().Symbol.Column,
                context.Percent().GetText().Length,
                DS0002_MethodNotFound,
                $"The type '{t.FullName}' does not implement a remainder operation with the operand type '{t2.FullName}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitPower_expression([NotNull] DassieParser.Power_expressionContext context)
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
                DS0036_ArithmeticError,
                $"The type '{t.FullName}' does not implement a exponentiation operation with the operand type '{t2.FullName}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, m, null);

        return m.ReturnType;
    }

    public override Type VisitLeft_shift_expression([NotNull] DassieParser.Left_shift_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (CliHelpers.IsIntegerType(t))
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
                DS0002_MethodNotFound,
                $"The type '{t.FullName}' does not implement a left shift operation with the operand type '{t2.FullName}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitRight_shift_expression([NotNull] DassieParser.Right_shift_expressionContext context)
    {
        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (CliHelpers.IsIntegerType(t))
        {
            EmitShr(t);
            return t;
        }

        MethodInfo op = t.GetMethod("op_RightShift", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Double_Greater_Than().Symbol.Line,
                context.Double_Greater_Than().Symbol.Column,
                context.Double_Greater_Than().GetText().Length,
                DS0002_MethodNotFound,
                $"The type '{t.FullName}' does not implement a right shift operation with the operand type '{t2.FullName}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

        return t;
    }

    public override Type VisitTypeof_expression([NotNull] DassieParser.Typeof_expressionContext context)
    {
        CurrentFile.Fragments.Add(new()
        {
            Color = Color.Word,
            Line = context.Caret_Backslash().Symbol.Line,
            Column = context.Caret_Backslash().Symbol.Column,
            Length = context.Caret_Backslash().GetText().Length,
        });

        Type t = CliHelpers.ResolveTypeName(context.type_name());
        CurrentMethod.IL.Emit(OpCodes.Ldtoken, t);

        MethodInfo typeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });
        CurrentMethod.IL.EmitCall(OpCodes.Call, typeFromHandle, null);

        return typeof(Type);
    }

    public override Type VisitNameof_expression([NotNull] DassieParser.Nameof_expressionContext context)
    {
        CurrentFile.Fragments.Add(new()
        {
            Color = Color.Word,
            Line = context.Dollar_Backslash().Symbol.Line,
            Column = context.Dollar_Backslash().Symbol.Column,
            Length = context.Dollar_Backslash().GetText().Length,
        });

        CurrentMethod.IL.DeclareLocal(typeof(int).MakeByRefType());

        CurrentFile.Fragments.Add(new()
        {
            Color = Color.ExpressionString,
            Line = context.expression().Start.Line,
            Column = context.expression().Start.Column,
            Length = context.expression().GetText().Length,
        });

        CurrentMethod.IL.Emit(OpCodes.Ldstr, context.expression().GetText());
        return typeof(string);
    }

    public override Type VisitByref_expression([NotNull] DassieParser.Byref_expressionContext context)
    {
        Type t = Visit(context.expression());
        return t;
    }

    public Type GetConstructorOrCast(Type cType, DassieParser.ArglistContext arglist, int line, int column, int length)
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
                    EmitErrorMessage(line, column, length, DS0002_MethodNotFound, $"The type '{cType.Name}' does not contain a constructor with the specified argument types.");
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
            EmitErrorMessage(line, column, length, DS0002_MethodNotFound, $"The type '{cType.Name}' does not contain a constructor or conversion with the specified argument types.");
            CurrentMethod.ArgumentTypesForNextMethodCall.Clear();

            return cType;
        }

        ConstructorInfo c = cType.GetConstructor(Type.EmptyTypes);

        if (c == null)
        {
            EmitErrorMessage(line, column, length, DS0002_MethodNotFound, $"The type '{cType.Name}' does not specify a parameterless constructor.");
            CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
            return cType;
        }

        CurrentMethod.IL.Emit(OpCodes.Newobj, c);

        CurrentMethod.ArgumentTypesForNextMethodCall.Clear();

        return cType;
    }

    int memberIndex = -1;
    bool notLoadAddress = false;

    public override Type VisitFull_identifier_member_access_expression([NotNull] DassieParser.Full_identifier_member_access_expressionContext context)
    {
        memberIndex++;

        Type[] typeArgs = null;
        if (context.type_arg_list() != null)
        {
            typeArgs = new Type[context.type_arg_list().type_name().Length];
            for (int i = 0; i < context.type_arg_list().type_name().Length; i++)
                typeArgs[i] = CliHelpers.ResolveTypeName(context.type_arg_list().type_name()[i]);
        }

        if (context.full_identifier().Identifier().Length > 1)
            CurrentMethod.ShouldLoadAddressIfValueType = true;

        if (CliHelpers.HandleSpecialFunction(
            context.full_identifier().Identifier().Last().GetText(),
            context.arglist(),
            context.full_identifier().Identifier().Last().Symbol.Line,
            context.full_identifier().Identifier().Last().Symbol.Column,
            context.full_identifier().Identifier().Last().GetText().Length))
            return typeof(void);

        object o = SymbolResolver.GetSmallestTypeFromLeft(
            context.full_identifier(),
            typeArgs,
            context.full_identifier().Start.Line,
            context.full_identifier().Start.Column,
            context.full_identifier().GetText().Length,
            out int firstIndex);

        Type t = null;
        bool exitEarly = true;

        if (o is Type type)
        {
            t = type;
            exitEarly = false;
        }
        else
        {
            if (o == null)
            {
                EmitErrorMessage(
                    context.full_identifier().Identifier()[0].Symbol.Line,
                    context.full_identifier().Identifier()[0].Symbol.Column,
                    context.full_identifier().Identifier()[0].GetText().Length,
                    DS0056_SymbolResolveError,
                    $"The name '{context.full_identifier().Identifier()[0].GetText()}' could not be resolved.");

                return null;
            }

            if (o is ParamInfo p)
            {
                SymbolInfo s = new()
                {
                    Parameter = p,
                    SymbolType = SymbolInfo.SymType.Parameter
                };

                if (CurrentMethod.ShouldLoadAddressIfValueType && !notLoadAddress)
                    s.LoadAddressIfValueType();
                else
                    s.Load();

                t = s.Type();
            }

            else if (o is LocalInfo l)
            {
                SymbolInfo s = new()
                {
                    Local = l,
                    SymbolType = SymbolInfo.SymType.Local
                };

                if ((CurrentMethod.ShouldLoadAddressIfValueType && !notLoadAddress) || (l.Builder.LocalType.IsValueType && context.full_identifier().Identifier().Length > 1))
                    s.LoadAddressIfValueType();
                else
                    s.Load();

                t = s.Type();
                notLoadAddress = false;

                CurrentFile.Fragments.Add(s.GetFragment(
                    context.Start.Line,
                    context.Start.Column,
                    context.GetText().Length,
                    true));
            }

            else if (o is FieldInfo f)
            {
                if (TryGetConstantValue(f, out object v))
                {
                    EmitConst(v);
                    return f.FieldType;
                }

                if (f.IsStatic)
                    CurrentMethod.IL.Emit(OpCodes.Ldsfld, f);

                else if (TypeContext.Current.Fields.Any(_f => _f.Builder == f))
                {
                    CurrentMethod.IL.Emit(OpCodes.Ldarg_0);
                    CurrentMethod.IL.Emit(OpCodes.Ldfld, f);
                }

                //return f.FieldType;
                t = f.FieldType;
            }

            else if (o is SymbolResolver.EnumValueInfo e)
            {
                EmitLdcI4((int)e.Value);
                t = e.EnumType;
            }

            else if (o is MethodBuilder m)
            {
                for (int i = 0; i < m.GetParameters().Length; i++)
                {
                    ParameterInfo param = m.GetParameters()[i];

                    if (param.ParameterType.IsByRef /*|| param.ParameterType.IsByRefLike*/)
                        CurrentMethod.ByRefArguments.Add(i);
                }

                bool allowTailCall = CurrentMethod.AllowTailCallEmission;
                CurrentMethod.AllowTailCallEmission = false;

                if (context.arglist() != null)
                    Visit(context.arglist());

                CurrentMethod.AllowTailCallEmission = allowTailCall;

                EmitTailcall();
                EmitCall(m.DeclaringType, m);

                if (m.ReturnType.IsValueType && CurrentMethod.ShouldLoadAddressIfValueType && !notLoadAddress)
                {
                    CurrentMethod.IL.DeclareLocal(m.ReturnType);
                    CurrentMethod.LocalIndex++;
                    EmitStloc(CurrentMethod.LocalIndex);
                    EmitLdloca(CurrentMethod.LocalIndex);
                }

                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                t = m.ReturnType;
            }

            // Global method
            else if (o is List<MethodInfo> methods)
            {
                //int boxMemberIndex = memberIndex;

                bool allowTailCall = CurrentMethod.AllowTailCallEmission;
                CurrentMethod.AllowTailCallEmission = false;

                if (context.arglist() != null)
                    Visit(context.arglist());

                CurrentMethod.AllowTailCallEmission = allowTailCall;

                Type[] argumentTypes = (CurrentMethod.ArgumentTypesForNextMethodCall ?? Type.EmptyTypes.ToList()).ToArray();
                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                MethodInfo final = null;

                if (methods.Any())
                {
                    foreach (MethodInfo possibleMethod in methods)
                    {
                        if (final != null)
                            break;

                        if (possibleMethod.GetParameters().Length == 0 && argumentTypes.Length == 0)
                        {
                            final = possibleMethod;
                            break;
                        }

                        if (!CurrentMethod.ParameterBoxIndices.ContainsKey(memberIndex))
                            CurrentMethod.ParameterBoxIndices.Add(memberIndex, new());

                        for (int i = 0; i < possibleMethod.GetParameters().Length; i++)
                        {
                            if (argumentTypes[i] == possibleMethod.GetParameters()[i].ParameterType || possibleMethod.GetParameters()[i].ParameterType == typeof(object))
                            {
                                if (possibleMethod.GetParameters()[i].ParameterType == typeof(object) && argumentTypes[i] != typeof(object))
                                    CurrentMethod.ParameterBoxIndices[memberIndex].Add(i);

                                if (i == possibleMethod.GetParameters().Length - 1)
                                {
                                    final = possibleMethod;
                                    break;
                                }
                            }

                            else
                                break;
                        }
                    }

                    if (final == null)
                    {
                        EmitErrorMessage(
                            context.full_identifier().Identifier()[0].Symbol.Line,
                            context.full_identifier().Identifier()[0].Symbol.Column,
                            context.full_identifier().Identifier()[0].GetText().Length,
                            DS0002_MethodNotFound,
                            $"Could not resolve global function '{context.full_identifier().Identifier()[0].GetText()}'.");
                    }

                    CurrentFile.Fragments.Add(new()
                    {
                        Line = context.full_identifier().Identifier()[0].Symbol.Line,
                        Column = context.full_identifier().Identifier()[0].Symbol.Column,
                        Length = context.full_identifier().Identifier()[0].GetText().Length,
                        Color = Dassie.Text.Color.Function,
                        IsNavigationTarget = false,
                        ToolTip = TooltipGenerator.Function(final)
                    });
                }

                EmitCall(final.DeclaringType, final);
                return final.ReturnType;
            }
        }

        if (context.full_identifier().Identifier().Length == 1 && exitEarly)
            return t;

        BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

        ITerminalNode[] nextNodes = [];
        if (context.full_identifier().Identifier().Last().GetText() == t.Name)
            nextNodes = [(context.full_identifier().Identifier().Last())];
        else
            nextNodes = context.full_identifier().Identifier()[firstIndex..];

        foreach (ITerminalNode identifier in nextNodes)
        {
            Type[] _params = null;

            if (identifier == context.full_identifier().Identifier().Last() && context.arglist() != null)
            {
                notLoadAddress = true;

                CliHelpers.RedirectEmitterToNullStream();
                Visit(context.arglist());
                CliHelpers.ResetNullStream();

                _params = CurrentMethod.ArgumentTypesForNextMethodCall.ToArray();
                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
            }

            object member = SymbolResolver.ResolveMember(
                t,
                identifier.GetText(),
                identifier.Symbol.Line,
                identifier.Symbol.Column,
                identifier.GetText().Length,
                false,
                _params,
                flags);

            if (identifier == context.full_identifier().Identifier().Last() && context.arglist() != null)
            {
                Visit(context.arglist());
                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
            }

            if (member == null)
                return null;

            if (member is FieldInfo f)
            {
                if (TryGetConstantValue(f, out object v))
                    EmitConst(v);
                else
                    LoadField(f);

                t = f.FieldType;
            }

            else if (member is SymbolResolver.EnumValueInfo e)
            {
                EmitLdcI4((int)e.Value);
                t = e.EnumType;
            }

            else if (member is PropertyInfo p)
            {
                EmitCall(t, p.GetGetMethod());
                t = p.PropertyType;

                if (identifier != context.full_identifier().Identifier().Last() && t.IsValueType)
                {
                    CurrentMethod.IL.DeclareLocal(t);
                    CurrentMethod.LocalIndex++;
                    EmitStloc(CurrentMethod.LocalIndex);
                    EmitLdloca(CurrentMethod.LocalIndex);
                }
            }

            else if (member is ConstructorInfo c)
            {
                if (CurrentMethod.BoxCallingType)
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, t);
                    CurrentMethod.BoxCallingType = false;
                }
                else if (VisitorStep1CurrentMethod != null && VisitorStep1CurrentMethod.BoxCallingType)
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, t);
                    VisitorStep1CurrentMethod.BoxCallingType = false;
                }

                CurrentMethod.IL.Emit(OpCodes.Newobj, c);
                t = c.DeclaringType;

                if (VisitorStep1CurrentMethod != null)
                    CurrentMethod.ParameterBoxIndices.Clear();
            }

            else if (member is MethodInfo m)
            {
                if (CurrentMethod.BoxCallingType)
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, t);
                    CurrentMethod.BoxCallingType = false;
                }
                else if (VisitorStep1CurrentMethod != null && VisitorStep1CurrentMethod.BoxCallingType)
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, t);
                    VisitorStep1CurrentMethod.BoxCallingType = false;
                }
                else if (t.IsValueType)
                {
                    //CurrentMethod.IL.DeclareLocal(t);
                    //CurrentMethod.LocalIndex++;
                    //EmitStloc(CurrentMethod.LocalIndex);
                    //EmitLdloca(CurrentMethod.LocalIndex);
                }

                EmitTailcall();
                EmitCall(t, m);
                t = m.ReturnType;

                if (VisitorStep1CurrentMethod != null)
                    CurrentMethod.ParameterBoxIndices.Clear();
            }
        }

        //CurrentMethod.ParameterBoxIndices.Clear();
        return t;
    }

    public override Type VisitMember_access_expression([NotNull] DassieParser.Member_access_expressionContext context)
    {
        memberIndex++;

        CurrentMethod.ShouldLoadAddressIfValueType = true;
        CurrentMethod.IgnoreTypesInSymbolResolve = true;

        Type t = Visit(context.expression());
        BindingFlags flags = BindingFlags.Public;

        if (CurrentMethod.StaticCallType != null)
        {
            t = CurrentMethod.StaticCallType;
            flags |= BindingFlags.Static;

            CurrentMethod.StaticCallType = null;
        }

        foreach (ITerminalNode identifier in context.Identifier())
        {
            Type[] _params = null;

            if (identifier == context.Identifier().Last() && context.arglist() != null)
            {
                CliHelpers.RedirectEmitterToNullStream();
                Visit(context.arglist());
                CliHelpers.ResetNullStream();

                _params = CurrentMethod.ArgumentTypesForNextMethodCall.ToArray();
                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
            }

            object member = SymbolResolver.ResolveMember(
                t,
                identifier.GetText(),
                identifier.Symbol.Line,
                identifier.Symbol.Column,
                identifier.GetText().Length,
                false,
                _params,
                flags);

            if (identifier == context.Identifier().Last() && context.arglist() != null)
            {
                Visit(context.arglist());
                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
            }

            if (member == null)
                return null;

            if (member is FieldInfo f)
            {
                if (TryGetConstantValue(f, out object v))
                    EmitConst(v);
                else
                    LoadField(f);

                t = f.FieldType;
            }

            else if (member is SymbolResolver.EnumValueInfo e)
            {
                EmitLdcI4((int)e.Value);
                t = e.EnumType;
            }

            else if (member is PropertyInfo p)
            {
                EmitCall(t, p.GetGetMethod());
                t = p.PropertyType;
            }

            else if (member is MethodInfo m)
            {
                if (CurrentMethod.BoxCallingType)
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, t);
                    CurrentMethod.BoxCallingType = false;
                }
                else if (t.IsValueType)
                {
                    CurrentMethod.IL.DeclareLocal(t);
                    CurrentMethod.LocalIndex++;
                    EmitStloc(CurrentMethod.LocalIndex);
                    EmitLdloca(CurrentMethod.LocalIndex);
                }

                EmitTailcall();
                EmitCall(t, m);
                t = m.ReturnType;

                if (VisitorStep1CurrentMethod != null)
                    CurrentMethod.ParameterBoxIndices.Clear();
            }
        }

        //CurrentMethod.ParameterBoxIndices.Clear();
        return t;
    }

    public override Type VisitArglist([NotNull] DassieParser.ArglistContext context)
    {
        CurrentMethod.CurrentArg = 0;
        CurrentMethod.ArgumentTypesForNextMethodCall.Clear();

        for (int i = 0; i < context.expression().Length; i++)
        {
            CurrentMethod.CurrentArg = i;

            IParseTree tree = context.expression()[i];
            Type t = Visit(tree);

            if (CurrentMethod.ByRefArguments.Contains(i))
            {
                // TODO: The following code ignores newlined_expression and parenthesized_expression as well as semicolon-delimited
                // expressions. Not sure how to deal with this, but for now it seems way too complicated...

                //if (tree is not DassieParser.Byref_expressionContext)
                //{
                //    EmitErrorMessage(
                //        context.expression()[i].Start.Line,
                //        context.expression()[i].Start.Column,
                //        context.expression()[i].GetText().Length,
                //        DS0096_PassByReferenceWithoutOperator,
                //        "Passing by reference requires the '&' operator.");
                //}
                //else if (!TreeHelpers.CanBePassedByReference(tree))
                //{
                //    EmitErrorMessage(
                //        context.expression()[i].Start.Line,
                //        context.expression()[i].Start.Column,
                //        context.expression()[i].GetText().Length,
                //        DS0097_InvalidExpressionPassedByReference,
                //        "Only assignable symbols can be passed by reference.");
                //}
            }

            if ((VisitorStep1CurrentMethod != null) && !VisitorStep1CurrentMethod.ParameterBoxIndices.ContainsKey(memberIndex))
                VisitorStep1CurrentMethod.ParameterBoxIndices.Add(memberIndex, new());

            if (!CurrentMethod.ParameterBoxIndices.ContainsKey(memberIndex))
                CurrentMethod.ParameterBoxIndices.Add(memberIndex, new());

            if (CurrentMethod.ParameterBoxIndices[memberIndex].Contains(i)
                || (VisitorStep1CurrentMethod != null && VisitorStep1CurrentMethod.ParameterBoxIndices[memberIndex].Contains(i)))
            {
                Type boxedType = t;
                if (boxedType.IsByRef /*|| boxedType.IsByRefLike*/)
                    boxedType = boxedType.GetElementType();

                CurrentMethod.IL.Emit(OpCodes.Box, boxedType);
                t = typeof(object);
            }

            CurrentMethod.ArgumentTypesForNextMethodCall.Add(t);
        }

        CurrentMethod.ParameterBoxIndices[memberIndex].Clear();
        VisitorStep1CurrentMethod?.ParameterBoxIndices[memberIndex].Clear();
        CurrentMethod.ByRefArguments.Clear();

        return null;
    }

    public override Type VisitCode_block([NotNull] DassieParser.Code_blockContext context)
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

        CurrentMethod.IL.BeginScope();
        CurrentMethod.EmitTailCall = false;

        foreach (IParseTree tree in context.expression().Take(context.expression().Length - 1))
        {
            Type _t = Visit(tree);

            if (_t != typeof(void) && !CurrentMethod.SkipPop)
                CurrentMethod.IL.Emit(OpCodes.Pop);

            if (CurrentMethod.SkipPop)
                CurrentMethod.SkipPop = false;
        }

        bool allowTailCall;
        object ctx = context;
        if (context.Parent is DassieParser.Block_expressionContext block)
            ctx = block.Parent;

        if (ctx is not DassieParser.Type_memberContext)
        {
            while (((RuleContext)ctx).Parent is DassieParser.Newlined_expressionContext)
                ctx = ((RuleContext)ctx).Parent;

            ctx = ((RuleContext)ctx).Parent;
        }

        allowTailCall = ctx is DassieParser.Type_memberContext && context.expression().Last() is DassieParser.Member_access_expressionContext or DassieParser.Full_identifier_member_access_expressionContext;
        CurrentMethod.EmitTailCall = allowTailCall;

        Type ret = Visit(context.expression().Last());

        CurrentMethod.IL.EndScope();

        return ret;
    }

    public override Type VisitIdentifier_atom([NotNull] DassieParser.Identifier_atomContext context)
    {
        string text = context.Identifier() != null
            ? context.Identifier().GetText()
            : context.full_identifier().GetText();

        object obj = SymbolResolver.ResolveIdentifier(
            text,
            context.Start.Line,
            context.Start.Column,
            text.Length);

        if (obj == null)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                text.Length,
                DS0056_SymbolResolveError,
                $"The name '{text}' could not be resolved.");

            return null;
        }

        if (obj is ParamInfo p)
        {
            SymbolInfo s = new()
            {
                Parameter = p,
                SymbolType = SymbolInfo.SymType.Parameter
            };

            if (CurrentMethod.ShouldLoadAddressIfValueType)
                s.LoadAddressIfValueType();
            else
                s.Load();

            return p.Type;
        }

        else if (obj is LocalInfo l)
        {
            SymbolInfo s = new()
            {
                Local = l,
                SymbolType = SymbolInfo.SymType.Local
            };

            if (CurrentMethod.ShouldLoadAddressIfValueType)
                s.LoadAddressIfValueType();
            else
                s.Load();

            return l.Builder.LocalType;
        }

        else if (obj is MethodBuilder m)
        {
            EmitCall(m.DeclaringType, m);

            if (m.ReturnType.IsValueType && CurrentMethod.ShouldLoadAddressIfValueType)
            {
                CurrentMethod.IL.DeclareLocal(m.ReturnType);
                CurrentMethod.LocalIndex++;
                CurrentMethod.IL.Emit(OpCodes.Stloc, CurrentMethod.LocalIndex);
                EmitLdloca(CurrentMethod.LocalIndex);
            }

            return m.ReturnType;
        }

        else if (obj is Type t)
        {
            if (CurrentMethod.IgnoreTypesInSymbolResolve)
            {
                CurrentMethod.StaticCallType = t;

                CurrentMethod.IgnoreTypesInSymbolResolve = false;
                return typeof(Type);
            }

            CurrentMethod.IL.Emit(OpCodes.Ldtoken, t);
            return typeof(RuntimeTypeHandle);
        }

        return null;
    }

    //public override Type VisitFull_identifier([NotNull] DassieParser.Full_identifierContext context)
    //{
    //    return Helpers.ResolveTypeName(context.GetText(), context.Identifier().Last().Symbol.Line, context.Identifier().Last().Symbol.Column, context.Identifier().Last().GetText().Length);
    //}

    public override Type VisitPrefix_if_expression([NotNull] DassieParser.Prefix_if_expressionContext context)
    {
        Type t;
        List<Type> t2 = new();
        Type t3 = null;

        Label falseBranch = CurrentMethod.IL.DefineLabel();
        Label restBranch = CurrentMethod.IL.DefineLabel();

        // Comparative expression
        Type ct = Visit(context.if_branch().expression()[0]);

        TypeHelpers.EnsureBoolean(ct,
            context.Start.Line,
            context.Start.Column,
            context.Start.Text.Length);

        CurrentMethod.IL.Emit(OpCodes.Brfalse, falseBranch);

        if (context.if_branch().code_block() != null)
            t = Visit(context.if_branch().code_block());
        else
            t = Visit(context.if_branch().expression().Last());

        CurrentMethod.IL.Emit(OpCodes.Br, restBranch);

        CurrentMethod.IL.MarkLabel(falseBranch);

        if (context.elif_branch() != null)
        {
            foreach (DassieParser.Elif_branchContext tree in context.elif_branch())
            {
                Label stillFalseBranch = CurrentMethod.IL.DefineLabel();

                Type _ct = Visit(tree.expression()[0]);
                TypeHelpers.EnsureBoolean(_ct,
                    context.Start.Line,
                    context.Start.Column,
                    context.Start.Text.Length);

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
            return typeof(UnionValue);

        return t;
    }

    public override Type VisitPostfix_if_expression([NotNull] DassieParser.Postfix_if_expressionContext context)
    {
        Label fb = CurrentMethod.IL.DefineLabel();
        Label rest = CurrentMethod.IL.DefineLabel();

        // Comparative expression
        Type ct = Visit(context.postfix_if_branch().expression());
        TypeHelpers.EnsureBoolean(ct,
            context.Start.Line,
            context.Start.Column,
            context.Start.Text.Length);

        CurrentMethod.IL.Emit(OpCodes.Brfalse, fb);

        Type t = Visit(context.expression());

        CurrentMethod.IL.MarkLabel(fb);
        CurrentMethod.IL.Emit(OpCodes.Br, rest);

        CurrentMethod.IL.MarkLabel(rest);

        return t;
    }

    public override Type VisitPrefix_unless_expression([NotNull] DassieParser.Prefix_unless_expressionContext context)
    {

        Type t;
        List<Type> t2 = new();
        Type t3 = null;

        Label falseBranch = CurrentMethod.IL.DefineLabel();
        Label restBranch = CurrentMethod.IL.DefineLabel();

        // Comparative expression
        Type ct = Visit(context.unless_branch().expression()[0]);
        TypeHelpers.EnsureBoolean(ct,
            context.Start.Line,
            context.Start.Column,
            context.Start.Text.Length);

        CurrentMethod.IL.Emit(OpCodes.Brtrue, falseBranch);

        if (context.unless_branch().code_block() != null)
            t = Visit(context.unless_branch().code_block());
        else
            t = Visit(context.unless_branch().expression().Last());

        CurrentMethod.IL.Emit(OpCodes.Br, restBranch);

        CurrentMethod.IL.MarkLabel(falseBranch);

        if (context.else_unless_branch() != null)
        {
            foreach (DassieParser.Else_unless_branchContext tree in context.else_unless_branch())
            {
                Label stillFalseBranch = CurrentMethod.IL.DefineLabel();

                Type _ct = Visit(tree.expression()[0]);
                TypeHelpers.EnsureBoolean(_ct,
                    context.Start.Line,
                    context.Start.Column,
                    context.Start.Text.Length);

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
                    DS0037_BranchExpressionTypesUnequal,
                    $"The return types of the branches of the conditional expression do not match.");
        }

        return t;
    }

    public override Type VisitPostfix_unless_expression([NotNull] DassieParser.Postfix_unless_expressionContext context)
    {
        Label fb = CurrentMethod.IL.DefineLabel();
        Label rest = CurrentMethod.IL.DefineLabel();

        // Comparative expression
        Type ct = Visit(context.postfix_unless_branch().expression());
        TypeHelpers.EnsureBoolean(ct,
            context.Start.Line,
            context.Start.Column,
            context.Start.Text.Length);

        CurrentMethod.IL.Emit(OpCodes.Brtrue, fb);

        Type t = Visit(context.expression());

        CurrentMethod.IL.MarkLabel(fb);
        CurrentMethod.IL.Emit(OpCodes.Br, rest);

        CurrentMethod.IL.MarkLabel(rest);

        return t;
    }

    public override Type VisitReal_atom([NotNull] DassieParser.Real_atomContext context)
    {
        Expression expr = eval.VisitReal_atom(context);

        if (expr.Type == typeof(float))
            CurrentMethod.IL.Emit(OpCodes.Ldc_R4, expr.Value);
        else
            CurrentMethod.IL.Emit(OpCodes.Ldc_R8, expr.Value);

        return expr.Type;
    }

    public override Type VisitInteger_atom([NotNull] DassieParser.Integer_atomContext context)
    {
        Expression expr = eval.VisitInteger_atom(context);

        if (expr.Type == typeof(ulong) || expr.Type == typeof(long))
            CurrentMethod.IL.Emit(OpCodes.Ldc_I8, expr.Value);
        else
            EmitLdcI4(expr.Value);

        return expr.Type;
    }

    public override Type VisitString_atom([NotNull] DassieParser.String_atomContext context)
    {
        string rawText = eval.VisitString_atom(context).Value;

        CurrentMethod.IL.Emit(OpCodes.Ldstr, rawText);

        return typeof(string);
    }

    public override Type VisitCharacter_atom([NotNull] DassieParser.Character_atomContext context)
    {
        char rawChar = eval.VisitCharacter_atom(context).Value;

        CurrentMethod.IL.Emit(OpCodes.Ldc_I4, rawChar);

        return typeof(char);
    }

    public override Type VisitBoolean_atom([NotNull] DassieParser.Boolean_atomContext context)
    {
        Expression val = eval.VisitBoolean_atom(context);

        CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)(val.Value ? 1 : 0));

        return typeof(bool);
    }

    public override Type VisitAssignment([NotNull] DassieParser.AssignmentContext context)
    {
        if (context.expression()[0].GetType() != typeof(DassieParser.Full_identifier_member_access_expressionContext)
            && context.expression()[0].GetType() != typeof(DassieParser.Member_access_expressionContext))
        {
            EmitErrorMessage(
                context.expression()[0].Start.Line,
                context.expression()[0].Start.Column,
                context.expression()[0].GetText().Length,
                DS0065_AssignmentInvalidLeftSide,
                "Invalid left side of assignment.");

            return Visit(context.expression()[1]);
        }

        memberIndex++;

        CurrentMethod.ShouldLoadAddressIfValueType = true;
        CurrentMethod.IgnoreTypesInSymbolResolve = true;

        Type ret = Visit(context.expression()[1]);
        int tempIndex = ++CurrentMethod.LocalIndex;

        CurrentMethod.IL.DeclareLocal(ret);
        EmitStloc(tempIndex);

        //if (context.expression()[0].GetType() == typeof(DassieParser.Member_access_expressionContext))

        dynamic con = context.expression()[0] as DassieParser.Member_access_expressionContext;
        con ??= context.expression()[0] as DassieParser.Full_identifier_member_access_expressionContext;

        object o = null;
        int firstIndex = 0;
        bool exitEarly = true;

        if (context.expression()[0].GetType() == typeof(DassieParser.Full_identifier_member_access_expressionContext))
        {
            o = SymbolResolver.GetSmallestTypeFromLeft(
            con.full_identifier(),
            null,
            con.full_identifier().Start.Line,
            con.full_identifier().Start.Column,
            con.full_identifier().GetText().Length,
            out firstIndex);
        }

        Type t = null;

        if (o == null)
            t = Visit(con.expression());

        if (o != null && o is Type type)
        {
            t = type;
            exitEarly = false;
        }
        else
        {
            if (o == null)
            {
                EmitErrorMessage(
                    con.full_identifier().Identifier()[0].Symbol.Line,
                    con.full_identifier().Identifier()[0].Symbol.Column,
                    con.full_identifier().Identifier()[0].GetText().Length,
                    DS0056_SymbolResolveError,
                    $"The name '{con.full_identifier().Identifier()[0].GetText()}' could not be resolved.");

                return null;
            }

            if (o is ParamInfo p)
            {
                SymbolInfo s = new()
                {
                    Parameter = p,
                    SymbolType = SymbolInfo.SymType.Parameter
                };

                if (CurrentMethod.ShouldLoadAddressIfValueType)
                    s.LoadAddressIfValueType();
                else
                    s.Load();

                t = s.Type();
            }

            else if (o is LocalInfo l)
            {
                SymbolInfo s = new()
                {
                    Local = l,
                    SymbolType = SymbolInfo.SymType.Local
                };

                if (CurrentMethod.ShouldLoadAddressIfValueType)
                    s.LoadAddressIfValueType();
                else
                    s.Load();

                t = s.Type();
            }

            else if (o is FieldInfo f)
            {
                if (f.IsInitOnly)
                {
                    EmitErrorMessage(
                        context.Start.Line,
                        context.Start.Column,
                        context.GetText().Length,
                        DS0094_InitOnlyFieldAssignedOutsideOfConstructor,
                        $"The field '{f.Name}' is readonly and cannot be modified outside of a constructor.");
                }

                if (f.IsStatic)
                {
                    EmitLdloc(tempIndex);
                    CurrentMethod.IL.Emit(OpCodes.Stsfld, f);
                }

                else if (TypeContext.Current.Fields.Any(_f => _f.Builder == f))
                {
                    CurrentMethod.IL.Emit(OpCodes.Ldarg_0);
                    EmitLdloc(tempIndex);
                    CurrentMethod.IL.Emit(OpCodes.Stfld, f);
                }

                return ret;
            }

            else if (o is SymbolResolver.EnumValueInfo e)
            {
                EmitLdcI4((int)e.Value);

                return ret;
            }

            else if (o is MethodBuilder m)
            {
                if (con.arglist() != null)
                    Visit(con.arglist());

                EmitCall(m.DeclaringType, m);

                if (m.ReturnType.IsValueType && CurrentMethod.ShouldLoadAddressIfValueType)
                {
                    CurrentMethod.IL.DeclareLocal(m.ReturnType);
                    CurrentMethod.LocalIndex++;
                    CurrentMethod.IL.Emit(OpCodes.Stloc, CurrentMethod.LocalIndex);
                    EmitLdloca(CurrentMethod.LocalIndex);
                }

                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();

                return m.ReturnType;
            }

            // Global method
            else if (o is List<MethodInfo> methods)
            {
                Visit(con.arglist());
                Type[] argumentTypes = CurrentMethod.ArgumentTypesForNextMethodCall.ToArray();
                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                MethodInfo final = null;

                if (methods.Any())
                {
                    foreach (MethodInfo possibleMethod in methods)
                    {
                        if (final != null)
                            break;

                        if (possibleMethod.GetParameters().Length == 0 && argumentTypes.Length == 0)
                        {
                            final = possibleMethod;
                            break;
                        }

                        if (!CurrentMethod.ParameterBoxIndices.ContainsKey(memberIndex))
                            CurrentMethod.ParameterBoxIndices.Add(memberIndex, new());

                        for (int i = 0; i < possibleMethod.GetParameters().Length; i++)
                        {
                            if (argumentTypes[i] == possibleMethod.GetParameters()[i].ParameterType || possibleMethod.GetParameters()[i].ParameterType == typeof(object))
                            {
                                if (possibleMethod.GetParameters()[i].ParameterType == typeof(object) && argumentTypes[i] != typeof(object))
                                    CurrentMethod.ParameterBoxIndices[memberIndex].Add(i);

                                if (i == possibleMethod.GetParameters().Length - 1)
                                {
                                    final = possibleMethod;
                                    break;
                                }
                            }

                            else
                                break;
                        }
                    }

                    if (final == null)
                    {
                        EmitErrorMessage(
                            con.full_identifier().Identifier()[0].Symbol.Line,
                            con.full_identifier().Identifier()[0].Symbol.Column,
                            con.full_identifier().Identifier()[0].GetText().Length,
                            DS0002_MethodNotFound,
                            $"Could not resolve global function '{con.full_identifier().Identifier()[0].GetText()}'.");
                    }

                    CurrentFile.Fragments.Add(new()
                    {
                        Line = con.full_identifier().Identifier()[0].Symbol.Line,
                        Column = con.full_identifier().Identifier()[0].Symbol.Column,
                        Length = con.full_identifier().Identifier()[0].GetText().Length,
                        Color = Dassie.Text.Color.Function,
                        IsNavigationTarget = false,
                        ToolTip = TooltipGenerator.Function(final)
                    });
                }

                EmitCall(final.DeclaringType, final);
                return final.ReturnType;
            }
        }

        if (con.full_identifier().Identifier().Length == 1 && exitEarly)
        {
            CurrentMethod.ShouldLoadAddressIfValueType = false;
            CurrentMethod.IgnoreTypesInSymbolResolve = false;

            return ret;
        }

        BindingFlags flags = BindingFlags.Public;

        if (CurrentMethod.StaticCallType != null)
        {
            t = CurrentMethod.StaticCallType;
            flags |= BindingFlags.Static;

            CurrentMethod.StaticCallType = null;
        }

        IEnumerable<ITerminalNode> ids;

        if (context.expression()[0].GetType() == typeof(DassieParser.Full_identifier_member_access_expressionContext))
        {
            ids = con.full_identifier().Identifier();
            ids = ids.ToArray()[firstIndex..];
        }
        else
            ids = con.Identifier();

        foreach (ITerminalNode identifier in ids)
        {
            Type[] _params = null;

            if (identifier == ids.Last() && con.arglist() != null)
            {
                Visit(con.arglist());
                _params = CurrentMethod.ArgumentTypesForNextMethodCall.ToArray();
            }

            object member = SymbolResolver.ResolveMember(
                t,
                identifier.GetText(),
                identifier.Symbol.Line,
                identifier.Symbol.Column,
                identifier.GetText().Length,
                false,
                _params,
                flags);

            if (member == null)
                return null;

            if (member is FieldInfo f)
            {
                if (identifier == ids.Last())
                {
                    if (f.IsInitOnly)
                    {
                        EmitErrorMessage(
                            context.Start.Line,
                            context.Start.Column,
                            context.GetText().Length,
                            DS0094_InitOnlyFieldAssignedOutsideOfConstructor,
                            $"The field '{f.Name}' is readonly and cannot be modified outside of a constructor.");
                    }

                    EmitLdloc(tempIndex);
                    EmitStfld(f);
                    CurrentMethod.SkipPop = true;
                }
                else
                {
                    LoadField(f);
                    t = f.FieldType;
                }
            }

            else if (member is SymbolResolver.EnumValueInfo e)
            {
                EmitLdcI4((int)e.Value);
                t = e.EnumType;
            }

            else if (member is PropertyInfo p)
            {
                if (identifier == ids.Last())
                {
                    if (p.GetSetMethod() == null)
                    {
                        EmitErrorMessage(
                            con.Identifier().Last().Symbol.Line,
                            con.Identifier().Last().Symbol.Column,
                            con.Identifier().Last().GetText().Length,
                            DS0066_PropertyNoSuitableSetter,
                            $"The property '{p.Name}' does not have a suitable setter.");
                    }

                    EmitLdloc(tempIndex);
                    EmitCall(t, p.GetSetMethod());
                }
                else
                {
                    EmitCall(t, p.GetGetMethod());
                    t = p.PropertyType;
                }
            }

            else if (member is MethodInfo m)
            {
                if (CurrentMethod.BoxCallingType)
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, t);
                    CurrentMethod.BoxCallingType = false;
                }
                else if (t.IsValueType)
                {
                    CurrentMethod.IL.DeclareLocal(t);
                    CurrentMethod.LocalIndex++;
                    EmitStloc(CurrentMethod.LocalIndex);
                    EmitLdloca(CurrentMethod.LocalIndex);
                }

                EmitCall(t, m);
                t = m.ReturnType;

                if (VisitorStep1CurrentMethod != null)
                    CurrentMethod.ParameterBoxIndices.Clear();
            }
        }

        CurrentMethod.ShouldLoadAddressIfValueType = false;
        CurrentMethod.IgnoreTypesInSymbolResolve = false;

        return ret;
    }

    public override Type VisitLocal_declaration_or_assignment([NotNull] DassieParser.Local_declaration_or_assignmentContext context)
    {
        if (context.expression() is DassieParser.Try_expressionContext)
        {
            EmitErrorMessage(
                context.expression().Start.Line,
                context.expression().Start.Column,
                context.expression().GetText().Length,
                DS0064_InvalidExpression,
                "A try block cannot be used as an expression.");
        }

        SymbolInfo sym = CliHelpers.GetSymbol(context.Identifier().GetText());

        if (sym is not null)
        {
            if (!sym.IsMutable())
            {
                EmitErrorMessage(
                    context.assignment_operator().Start.Line,
                    context.assignment_operator().Start.Column,
                    context.assignment_operator().GetText().Length,
                    DS0018_ImmutableValueReassignment,
                    $"'{sym.Name()}' is immutable and cannot be modified.");

                return sym.Type();
            }

            if (sym.Field != null && !sym.Field.Builder.IsStatic)
                EmitLdarg(0);

            if (sym.Type().IsByRef /*|| sym.Type().IsByRefLike*/)
                EmitLdarg(sym.Index());

            Type type = Visit(context.expression());

            bool checkTypes = true;
            if (type != sym.Type() && !((sym.Type().IsByRef /*|| sym.Type().IsByRefLike*/) && sym.Type().GetElementType() == type))
            {
                if (CanBeConverted(type, sym.Type()))
                {
                    EmitConversionOperator(type, sym.Type());
                    checkTypes = false;
                }
            }

            sym.Set();

            if (checkTypes && type != sym.Type() && !((sym.Type().IsByRef /*|| sym.Type().IsByRefLike*/) && sym.Type().GetElementType() == type))
            {
                if (sym.Type() == typeof(UnionValue))
                {
                    if (sym.Union().AllowedTypes.Contains(type))
                    {
                        sym.LoadAddress();

                        EmitLdloc(CurrentMethod.LocalIndex);
                        CurrentMethod.IL.Emit(OpCodes.Box, type);

                        MethodInfo m = typeof(UnionValue).GetMethod("set_Value", new Type[] { typeof(object) });
                        CurrentMethod.IL.Emit(OpCodes.Call, m);

                        CurrentMethod.SkipPop = true;
                        return sym.Union().GetType();
                    }

                    EmitErrorMessage(
                        context.assignment_operator().Start.Line,
                        context.assignment_operator().Start.Column,
                        context.assignment_operator().GetText().Length,
                        DS0019_GenericValueTypeInvalid,
                        $"Values of type '{type}' are not supported by union type '{sym.Union().ToTypeString()}'.");

                    return sym.Union().GetType();
                }

                EmitErrorMessage(
                    context.assignment_operator().Start.Line,
                    context.assignment_operator().Start.Column,
                    context.assignment_operator().GetText().Length,
                    DS0006_VariableTypeChanged,
                    $"Expected expression of type '{sym.Type().FullName}', but got type '{type.FullName}'.");

                return type;
            }

            if (sym.Field != null && !sym.Field.Builder.IsStatic)
                EmitLdarg(0);

            sym.Load();

            CurrentFile.Fragments.Add(sym.GetFragment(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetText().Length,
                false));

            return sym.Type();
        }

        Type t = Visit(context.expression());

        Type t1 = t;

        if (context.type_name() != null)
        {
            Type t2 = Visit(context.type_name());

            if (t2 != t)
            {
                MethodInfo implicitConversion = t.GetMethod("op_Implicit", new Type[] { t2 }, null);
                if (implicitConversion != null)
                {
                    EmitCall(t, implicitConversion);
                }
                else if (t2 == typeof(object))
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, t);
                }
                else if (t.MakeByRefType() == t2) { }
                else
                {
                    EmitErrorMessage(
                        context.expression().Start.Line,
                        context.expression().Start.Column,
                        context.expression().GetText().Length,
                        DS0057_IncompatibleType,
                        $"An expression of type '{t.FullName}' cannot be assigned to a variable of type '{t2.FullName}'.");
                }
            }

            t = t2;
        }

        LocalBuilder lb = CurrentMethod.IL.DeclareLocal(t);

        CurrentFile.Fragments.Add(new()
        {
            Line = context.Identifier().Symbol.Line,
            Column = context.Identifier().Symbol.Column,
            Length = context.Identifier().GetText().Length,
            Color = context.Var() == null ? Color.LocalValue : Color.LocalVariable,
            ToolTip = TooltipGenerator.Local(context.Identifier().GetText(), context.Var() != null, lb),
            IsNavigationTarget = true
        });

        SetLocalSymInfo(lb, context.Identifier().GetText());
        MarkSequencePoint(context.Identifier().Symbol.Line, context.Identifier().Symbol.Column, context.Identifier().GetText().Length);

        CurrentMethod.LocalIndex++;
        CurrentMethod.Locals.Add(new(context.Identifier().GetText(), lb, context.Var() == null, CurrentMethod.LocalIndex, CurrentMethod.CurrentUnion));

        if (t == typeof(UnionValue))
        {
            CurrentMethod.IL.Emit(OpCodes.Box, t1);

            ConstructorInfo constructor = t.GetConstructor(new Type[] { typeof(object), typeof(Type[]) });

            UnionValue union = CurrentMethod.CurrentUnion;

            EmitLdcI4(union.AllowedTypes.Length);
            CurrentMethod.IL.Emit(OpCodes.Newarr, typeof(Type));
            CurrentMethod.IL.Emit(OpCodes.Dup);

            for (int i = 0; i < union.AllowedTypes.Length; i++)
            {
                EmitLdcI4(i);
                CurrentMethod.IL.Emit(OpCodes.Ldtoken, union.AllowedTypes[i]);

                MethodInfo getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });
                CurrentMethod.IL.Emit(OpCodes.Call, getTypeFromHandle);
                CurrentMethod.IL.Emit(OpCodes.Stelem_Ref);

                CurrentMethod.IL.Emit(OpCodes.Dup);
            }

            CurrentMethod.IL.Emit(OpCodes.Pop);

            CurrentMethod.IL.Emit(OpCodes.Newobj, constructor);
        }

        EmitStloc(CurrentMethod.LocalIndex);

        EmitLdloc(CurrentMethod.LocalIndex);

        return t;
    }

    public override Type VisitType_name([NotNull] DassieParser.Type_nameContext context)
    {
        return eval.VisitType_name(context).Value;
    }

    public override Type VisitTuple_expression([NotNull] DassieParser.Tuple_expressionContext context)
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

    public override Type VisitArray_expression([NotNull] DassieParser.Array_expressionContext context)
    {
        Type arrayType = Visit(context.expression()[0]);
        CurrentMethod.IL.Emit(OpCodes.Pop);

        EmitLdcI4(context.expression().Length);
        CurrentMethod.IL.Emit(OpCodes.Newarr, arrayType);

        int index = 0;
        foreach (IParseTree tree in context.expression())
        {
            CurrentMethod.IL.Emit(OpCodes.Dup);
            EmitLdcI4(index++);
            Type t = Visit(tree);

            if (t != arrayType)
            {
                EmitErrorMessage(context.expression()[index - 1].Start.Line, context.expression()[index - 1].Start.Column, context.expression()[index - 1].Start.Text.Length, DS0041_ListItemsHaveDifferentTypes, "An array or list can only contain one type of value.");
                return arrayType.MakeArrayType();
            }

            CurrentMethod.IL.Emit(OpCodes.Stelem, t);
        }

        return arrayType.MakeArrayType();
    }

    public override Type VisitEmpty_atom([NotNull] DassieParser.Empty_atomContext context)
    {
        CurrentMethod.IL.Emit(OpCodes.Ldnull);
        return typeof(object);
    }

    public override Type VisitIndex_expression([NotNull] DassieParser.Index_expressionContext context)
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

    public override Type VisitRange_expression([NotNull] DassieParser.Range_expressionContext context)
    {
        return Visit(context.range());
    }

    public override Type VisitRange([NotNull] DassieParser.RangeContext context)
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

    public override Type VisitIndex([NotNull] DassieParser.IndexContext context)
    {
        Visit(context.integer_atom());

        EmitLdcI4(context.Caret() == null ? 0 : 1);

        CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(Index).GetConstructor(new Type[] { typeof(int), typeof(bool) }));
        return typeof(Index);
    }

    public override Type VisitArray_element_assignment([NotNull] DassieParser.Array_element_assignmentContext context)
    {
        Type arrayType = Visit(context.expression()[0]);

        Type index = Visit(context.expression()[1]);

        if (index == typeof(int))
        {
            Type t = Visit(context.expression()[2]);

            if (t != arrayType.GetEnumeratedType())
            {
                EmitErrorMessage(context.expression()[2].Start.Line, context.expression()[2].Start.Column, context.expression()[2].Start.Text.Length, DS0041_ListItemsHaveDifferentTypes, "The type of the new value of the specified array item does not match the type of the old one.");
                return t;
            }

            CurrentMethod.IL.Emit(OpCodes.Stelem, t);

            return t;
        }

        EmitErrorMessage(context.expression()[1].Start.Line, context.expression()[1].Start.Column, context.expression()[1].Start.Text.Length, DS0042_ArrayElementAssignmentIndexExpressionNotInteger, "The index expression has to be of type Int32.");

        return arrayType.GetEnumeratedType();
    }

    public override Type VisitWhile_loop([NotNull] DassieParser.While_loopContext context)
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

            CurrentMethod.Locals.Add(new(GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex++), returnBuilder, false, CurrentMethod.LocalIndex++, new(null, typeof(int))));

            EmitStloc(CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);

            SetLocalSymInfo(returnBuilder,
                GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1));

            Label loop = CurrentMethod.IL.DefineLabel();
            Label start = CurrentMethod.IL.DefineLabel();

            LocalBuilder lb = CurrentMethod.IL.DeclareLocal(typeof(int));
            CurrentMethod.Locals.Add(new(GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex++), lb, false, CurrentMethod.LocalIndex++, new(null, typeof(int))));

            SetLocalSymInfo(
                lb,
                GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex - 1));

            CurrentMethod.IL.Emit(OpCodes.Br, loop);

            CurrentMethod.IL.MarkLabel(start);

            // Save the return value of the current iteration to the returning array

            // Array
            EmitLdloc(CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);
            // Index
            EmitLdloc(CurrentMethod.Locals.Where(l => l.Name == GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex - 1)).First().Index + 1);

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

            EmitLdloc(CurrentMethod.Locals.Where(l => l.Name == GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex - 1)).First().Index + 1);
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)1);
            CurrentMethod.IL.Emit(OpCodes.Add);
            EmitStloc(CurrentMethod.Locals.Where(l => l.Name == GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex - 1)).First().Index + 1);

            CurrentMethod.IL.MarkLabel(loop);

            EmitLdloc(CurrentMethod.Locals.Where(l => l.Name == GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex - 1)).First().Index + 1);
            Visit(context.expression().First());
            CurrentMethod.IL.Emit(OpCodes.Blt, start);

            EmitLdloc(CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);
            CurrentMethod.SkipPop = false;

            return typeof(object[]);
        }

        if (TypeHelpers.IsBoolean(t))
        {
            TypeHelpers.EnsureBoolean(t, 0, 0, 0);

            CurrentMethod.IL.Emit(OpCodes.Pop);

            CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(List<object>).GetConstructor(Type.EmptyTypes));

            // A local that saves the returning list
            LocalBuilder returnBuilder = CurrentMethod.IL.DeclareLocal(typeof(List<object>));

            CurrentMethod.Locals.Add(new(GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex++), returnBuilder, false, CurrentMethod.LocalIndex++, new(null, typeof(List<string>))));

            EmitStloc(CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);

            SetLocalSymInfo(
                returnBuilder,
                GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1));

            Label loop = CurrentMethod.IL.DefineLabel();
            Label start = CurrentMethod.IL.DefineLabel();

            CurrentMethod.IL.Emit(OpCodes.Br, loop);

            CurrentMethod.IL.MarkLabel(start);

            EmitLdloc(CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);
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

            CurrentMethod.IL.MarkLabel(loop);

            Visit(context.expression().First());
            CurrentMethod.IL.Emit(OpCodes.Brtrue, start);

            EmitLdloc(CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);
            CurrentMethod.SkipPop = false;

            return typeof(List<object>);
        }

        EmitWarningMessage(
            context.expression().First().Start.Line,
            context.expression().First().Start.Column,
            context.expression().First().Start.Text.Length,
            DS0043_PossiblyUnintentionalInfiniteLoop,
            "The condition of the while loop is not a boolean. This loop will run indefinetly.");

        CurrentMethod.IL.Emit(OpCodes.Pop);

        Label infiniteLoop = CurrentMethod.IL.DefineLabel();
        CurrentMethod.IL.MarkLabel(infiniteLoop);

        tReturn = Visit(context.expression().Last());

        CurrentMethod.IL.Emit(OpCodes.Br, infiniteLoop);

        return typeof(object[]);
    }

    public override Type VisitPlaceholder([NotNull] DassieParser.PlaceholderContext context)
    {
        CurrentMethod.IL.Emit(OpCodes.Nop);
        return null;
    }

    public override Type VisitThis_atom([NotNull] DassieParser.This_atomContext context)
    {
        if (CurrentMethod.Builder.IsStatic)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                context.GetText().Length,
                DS0084_ThisInStaticFunction,
                "The keyword 'this' is not valid in the current context.");

            return typeof(void);
        }

        EmitLdarg(0);
        return TypeContext.Current.Builder;
    }

    public override Type VisitRaise_expression([NotNull] DassieParser.Raise_expressionContext context)
    {
        Type t = Visit(context.expression());

        if (t.IsValueType)
        {
            EmitErrorMessage(
               context.expression().Start.Line,
               context.expression().Start.Column,
               context.expression().GetText().Length,
               DS0060_InvalidThrowExpression,
               "The expression to throw must be a reference type.");

            return typeof(void);
        }

        CurrentMethod.IL.Emit(OpCodes.Throw);
        return typeof(void);
    }

    public override Type VisitRethrow_exception([NotNull] DassieParser.Rethrow_exceptionContext context)
    {
        if (!_isInsideCatch)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                context.GetText().Length,
                DS0059_RethrowOutsideCatchBlock,
                "Rethrowing exceptions is not possible outside of a catch block.");

            return typeof(void);
        }

        CurrentMethod.IL.Emit(OpCodes.Rethrow);
        return typeof(void);
    }

    bool _isInsideCatch = false;
    Label _exceptionBlockEnd;

    public override Type VisitTry_expression([NotNull] DassieParser.Try_expressionContext context)
    {
        Type t = Visit(context.try_branch());

        if (context.catch_branch() == null || context.catch_branch().Length < 1)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                context.GetText().Length,
                DS0061_MissingCatchBranch,
                "A try expression must contain at least one catch block.");

            return t;
        }

        if (context.fault_branch() != null)
            Visit(context.fault_branch());

        foreach (IParseTree tree in context.catch_branch())
        {
            Visit(tree);

            foreach (LocalInfo loc in invalidationList)
                loc.IsAvailable = false;

            invalidationList.Clear();
        }

        if (context.finally_branch() != null)
            Visit(context.finally_branch());

        CurrentMethod.IL.EndExceptionBlock();

        return t;
    }

    public override Type VisitTry_branch([NotNull] DassieParser.Try_branchContext context)
    {
        _exceptionBlockEnd = CurrentMethod.IL.BeginExceptionBlock();
        Type t = Visit(context.expression());
        return t;
    }

    List<LocalInfo> invalidationList = new();

    public override Type VisitCatch_branch([NotNull] DassieParser.Catch_branchContext context)
    {
        Type t = context.type_name() != null ? CliHelpers.ResolveTypeName(context.type_name()) : typeof(object);

        CurrentMethod.IL.BeginCatchBlock(t);

        if (context.Identifier() == null)
            CurrentMethod.IL.Emit(OpCodes.Pop);

        else
        {
            LocalBuilder lb = CurrentMethod.IL.DeclareLocal(t);
            lb.SetLocalSymInfo(context.Identifier().GetText());

            LocalInfo loc = new(context.Identifier().GetText(), lb, true, ++CurrentMethod.LocalIndex, default);
            invalidationList.Add(loc);
            CurrentMethod.Locals.Add(loc);

            EmitStloc(CurrentMethod.LocalIndex);
        }

        _isInsideCatch = true;
        Visit(context.expression());
        _isInsideCatch = false;

        return typeof(void);
    }

    public override Type VisitFinally_branch([NotNull] DassieParser.Finally_branchContext context)
    {
        CurrentMethod.IL.BeginFinallyBlock();
        Visit(context.expression());
        return typeof(void);
    }

    public override Type VisitFault_branch([NotNull] DassieParser.Fault_branchContext context)
    {
        EmitErrorMessage(
            context.Start.Line,
            context.Start.Column,
            context.GetText().Length,
            DS0063_UnsupportedFeature,
            "Dassie does not support 'fault' blocks yet.");

        return typeof(void);

        //CurrentMethod.IL.BeginFaultBlock();
        //Visit(context.expression());
        //return typeof(void);
    }

    public override Type VisitSeparated_expression([NotNull] DassieParser.Separated_expressionContext context)
    {
        return Visit(context.expression());
    }
}