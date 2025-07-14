using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Dassie.CodeGeneration.Helpers;
using Dassie.CodeGeneration.Structure;
using Dassie.Core;
using Dassie.Errors;
using Dassie.Helpers;
using Dassie.Intrinsics;
using Dassie.Meta;
using Dassie.Parser;
using Dassie.Runtime;
using Dassie.Symbols;
using Dassie.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using static Dassie.Helpers.TypeHelpers;
using Color = Dassie.Text.Color;

namespace Dassie.CodeGeneration;

internal class Visitor : DassieParserBaseVisitor<Type>
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

        if (context.export_directive() != null && context.export_directive().Length > 0)
            Visit(context.export_directive()[0]);

        if (context.export_directive().Length > 1)
        {
            foreach (ParserRuleContext pt in context.export_directive().Skip(1))
            {
                EmitErrorMessage(
                    pt.Start.Line,
                    pt.Start.Column,
                    pt.GetText().Length,
                    DS0199_MultipleExports,
                    "A source file can export at most one namespace.");
            }
        }

        Visit(context.file_body());

        if (Context.ModuleInitializerParts.Count > 0)
        {
            MethodBuilder cctor = Context.Module.DefineGlobalMethod(".cctor", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, typeof(void), []);
            ILGenerator il = cctor.GetILGenerator();

            foreach (MethodInfo part in Context.ModuleInitializerParts)
                il.Emit(OpCodes.Call, part);

            il.Emit(OpCodes.Ret);
        }

        foreach (TypeContext type in Context.Types.Where(c => c.Builder.IsCreated()))
        {
            if (type.FullName == null)
                continue;

            CurrentFile.Fragments.Add(new()
            {
                Color = TooltipGenerator.ColorForType(type.Builder.CreateTypeInfo()),
                Line = type.ParserRule.Identifier().Symbol.Line,
                Column = type.ParserRule.Identifier().Symbol.Column,
                Length = type.ParserRule.Identifier().GetIdentifier().Length,
                ToolTip = TooltipGenerator.Type(type.Builder.CreateTypeInfo(), true, true),
                IsNavigationTarget = true
            });
        }

        if (!string.IsNullOrEmpty(Context.Configuration.EntryPoint))
        {
            if (Context.EntryPoint != null)
            {
                EmitWarningMessage(
                    0, 0, 0,
                    DS0195_EntryPointManuallySetWhenUsingDSConfigEntryPointProperty,
                    $"The manually specified entry point '{Context.Configuration.EntryPoint}' was overwritten by an '<EntryPoint>' attribute or top-level code.",
                    ProjectConfigurationFileName);

                return typeof(void);
            }

            bool IsValidEntryPoint(out MethodContext entryPoint)
            {
                entryPoint = null;

                if (!Context.Configuration.EntryPoint.Contains('.'))
                    return false;

                string[] parts = Context.Configuration.EntryPoint.Split('.');
                string typeId = string.Join('.', parts[..^1]);
                string methodName = parts[^1];

                TypeContext typeContext = Context.Types.FirstOrDefault(t => t.Builder.FullName == typeId);
                if (typeContext == null)
                    return false;

                entryPoint = typeContext.Methods.FirstOrDefault(m => m.UniqueMethodName == methodName);
                if (entryPoint == null)
                    return false;

                return true;
            }

            if (IsValidEntryPoint(out MethodContext entryPoint))
            {
                entryPoint.Builder.SetCustomAttribute(new(typeof(EntryPointAttribute).GetConstructor([]), []));
                Context.EntryPointIsSet = true;
                Context.EntryPoint = entryPoint.Builder;
            }
            else
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0196_InvalidMethodIdentifier,
                    $"The manually specified entry point '{Context.Configuration.EntryPoint}' is not a valid method or function identifier.",
                    ProjectConfigurationFileName);
            }
        }

        return typeof(void);
    }

    //public override Type VisitFile_body([NotNull] DassieParser.File_bodyContext context)
    //{
    //    if (context.top_level_statements() != null)
    //    {
    //        Visit(context.top_level_statements());
    //        return typeof(void);
    //    }

    //    Visit(context.full_program());

    //    return typeof(void);
    //}

    public override Type VisitFull_program([NotNull] DassieParser.Full_programContext context)
    {
        foreach (IParseTree type in context.type() ?? [])
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
        TypeContext tc = Context.Types.First(t => t.FullName == GetTypeName(context));
        TypeContext.Current = tc;
        TypeContext.Current.ParserRule = context;

        if (tc.PrimaryConstructorParameterList != null)
        {
            List<FieldInfo> fields = [];

            foreach (DassieParser.ParameterContext param in tc.PrimaryConstructorParameterList.parameter())
            {
                string paramName = param.Identifier().GetIdentifier();
                string fieldName = SymbolNameGenerator.GetPropertyBackingFieldName(paramName);
                Type paramType = SymbolResolver.ResolveTypeName(param.type_name());

                FieldBuilder backingField = tc.Builder.DefineField(fieldName, paramType, FieldAttributes.Private);
                PropertyBuilder prop = tc.Builder.DefineProperty(paramName, PropertyAttributes.None, paramType, []);
                tc.Properties.Add(prop);

                MethodBuilder getter = tc.Builder.DefineMethod($"get_{paramName}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, paramType, []);
                ILGenerator ilGet = getter.GetILGenerator();
                ilGet.Emit(OpCodes.Ldarg_0);
                ilGet.Emit(OpCodes.Ldfld, backingField);
                ilGet.Emit(OpCodes.Ret);
                prop.SetGetMethod(getter);

                if (param.Var() != null)
                {
                    if (tc.IsImmutable)
                    {
                        EmitErrorMessage(
                            param.Var().Symbol.Line,
                            param.Var().Symbol.Column,
                            param.Var().GetText().Length,
                            DS0151_VarFieldInImmutableType,
                            $"The 'var' modifier is invalid on members of immutable value types. Properties of immutable types are not allowed to be mutable.");
                    }

                    MethodBuilder setter = tc.Builder.DefineMethod($"set_{paramName}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, typeof(void), [paramType]);
                    ILGenerator ilSet = setter.GetILGenerator();
                    ilSet.Emit(OpCodes.Ldarg_0);
                    ilSet.Emit(OpCodes.Ldarg_1);
                    ilSet.Emit(OpCodes.Stfld, backingField);
                    ilSet.Emit(OpCodes.Ret);
                    prop.SetSetMethod(setter);
                }

                fields.Add(backingField);
            }

            ConstructorBuilder cb = tc.Builder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, fields.Select(f => f.FieldType).ToArray());
            ILGenerator il = cb.GetILGenerator();

            MethodContext current = CurrentMethod;
            tc.ConstructorContexts.Add(new()
            {
                IL = il,
                ConstructorBuilder = cb
            });

            for (int i = 0; i < fields.Count; i++)
            {
                EmitLdarg(0);
                EmitLdarg(i + 1);
                il.Emit(OpCodes.Stfld, fields[i]); // Going through the property is unnecessary since the setters never have side effects
            }

            if (!tc.Builder.IsValueType)
            {
                EmitLdarg(0);
                il.Emit(OpCodes.Call, tc.Builder.BaseType.GetConstructor([]));
            }

            il.Emit(OpCodes.Ret);
            CurrentMethod = current;
        }

        if (!tc.Builder.IsInterface)
        {
            tc.RequiredInterfaceImplementations = tc.ImplementedInterfaces
                .SelectMany(t =>
                {
                    if (!t.IsConstructedGenericType)
                        return t.GetInterfaces().Append(t);

                    return t.GetGenericTypeDefinition().GetInterfaces().Append(t);
                })
                .SelectMany(t =>
                {
                    if (!t.IsConstructedGenericType || !t.GetGenericTypeDefinition().GetGenericArguments().Any(t => t is TypeBuilder))
                    {
                        return t.GetMethods().Select(m => new MockMethodInfo()
                        {
                            Name = m.Name,
                            ReturnType = m.ReturnType,
                            Parameters = m.GetParameters().Select(p => p.ParameterType).ToList(),
                            IsAbstract = m.IsAbstract,
                            DeclaringType = t,
                            IsGenericMethod = m.IsGenericMethod,
                            GenericTypeArguments = m.GetGenericArguments().ToList(),
                            Builder = m
                        });
                    }

                    Type[] typeArgs = t.GenericTypeArguments;

                    return t.GetGenericTypeDefinition().GetMethods().Select(m =>
                    {
                        MockMethodInfo method = new()
                        {
                            Name = m.Name,
                            IsAbstract = m.IsAbstract,
                            Parameters = [],
                            DeclaringType = t,
                            IsGenericMethod = m.IsGenericMethod,
                            GenericTypeArguments = m.GetGenericArguments().ToList(),
                            Builder = TypeBuilder.GetMethod(t, m)
                        };

                        if (!m.ReturnType.IsGenericTypeParameter)
                            method.ReturnType = m.ReturnType;
                        else
                            method.ReturnType = typeArgs[m.ReturnType.GenericParameterPosition];

                        foreach (Type param in m.GetParameters().Select(p => p.ParameterType))
                        {
                            if (!param.IsGenericTypeParameter)
                                method.Parameters.Add(param);
                            else
                                method.Parameters.Add(typeArgs[param.GenericParameterPosition]);
                        }

                        return method;
                    });
                })
                .Where(m => m.IsAbstract)
                .Distinct()
                .ToList();
        }

        if (context.type_block() != null)
        {
            foreach (DassieParser.TypeContext nestedType in context.type_block().type())
                VisitType(nestedType, tc.Builder);

            foreach (DassieParser.Type_memberContext member in context.type_block()?.type_member())
                Visit(member);
        }

        foreach (var ctor in TypeContext.Current.Constructors)
            HandleConstructor(ctor);

        if (TypeContext.Current.Constructors.Count == 0 && TypeContext.Current.FieldInitializers.Count > 0 && !TypeContext.Current.IsEnumeration)
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

        // TODO: Find a better way

        Type currentType = tc.Builder.BaseType;
        bool hasCycle = false;

        while (currentType != null)
        {
            if (currentType == tc.Builder)
            {
                hasCycle = true;
                break;
            }

            currentType = currentType.BaseType;
        }

        if (hasCycle)
        {
            if (!Context.DS0192Emissions.Any(types =>
                (types.Type1 == tc.Builder && types.Type2 == tc.Builder.BaseType) ||
                (types.Type1 == tc.Builder.BaseType && types.Type2 == tc.Builder)))
            {
                EmitErrorMessage(
                    context.Identifier().Symbol.Line,
                    context.Identifier().Symbol.Column,
                    context.Identifier().GetIdentifier().Length,
                    DS0192_CircularReference,
                    $"Circular base type dependency involving '{tc.Builder.FullName}' and '{tc.Builder.BaseType.FullName}'.");
            }

            Context.DS0192Emissions.Add((tc.Builder, tc.Builder.BaseType));
        }
        else
        {
            Type t = tc.Builder.CreateType();
            TypeContext.Current.FinishedType = t;
        }

        if (TypeContext.Current.RequiredInterfaceImplementations.Count > 0)
        {
            foreach (MockMethodInfo method in TypeContext.Current.RequiredInterfaceImplementations)
            {
                EmitErrorMessage(
                    context.Identifier().Symbol.Line,
                    context.Identifier().Symbol.Column,
                    context.Identifier().GetIdentifier().Length,
                    DS0156_RequiredInterfaceMembersNotImplemented,
                    $"The type '{tc.FullName}' does not provide an implementation for the abstract template member '{method.FormatMethod()}'.");
            }
        }
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
                if (CanBeConverted(t, field.FieldType))
                    EmitConversionOperator(t, field.FieldType);

                else
                {
                    EmitErrorMessage(
                        value.Start.Line,
                        value.Start.Column,
                        value.GetText().Length,
                        DS0054_WrongFieldType,
                        $"Expected expression of type '{TypeName(field.FieldType)}', but got type '{TypeName(t)}'.");
                }
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
        CurrentMethod = TypeContext.Current.GetMethod(context);

        if (CurrentMethod.IL != null)
            HandleFieldInitializersAndDefaultConstructor();

        Type t = typeof(void);

        if (context.expression() != null)
            t = Visit(context.expression());

        if (t != typeof(void))
        {
            EmitErrorMessage(
                context.Equals().Symbol.Line,
                context.Equals().Symbol.Column,
                context.expression().Start.Column - context.Equals().Symbol.Column,
                DS0093_ConstructorReturnsValue,
                $"Expected expression of type 'null' but found type '{TypeName(t)}'. A constructor can never return a value.");
        }

        CurrentMethod.IL?.Emit(OpCodes.Ret);

        //CurrentFile.Fragments.Add(new()
        //{
        //    Color = TooltipGenerator.ColorForType(TypeContext.Current.Builder),
        //    Line = context.Identifier().Symbol.Line,
        //    Column = context.Identifier().Symbol.Column,
        //    Length = context.Identifier().Identifier().Length,
        //    ToolTip = TooltipGenerator.Constructor(TypeContext.Current.Builder, _params),
        //    NavigationTargetKind = Fragment.NavigationKind.Constructor
        //});
    }

    //public override Type VisitAnonymous_function_expression([NotNull] DassieParser.Anonymous_function_expressionContext context)
    //{
    //    TypeBuilder closureType = TypeContext.Current.Builder.DefineNestedType(
    //        GetClosureTypeName(CurrentMethod.ClosureIndex),
    //        TypeAttributes.NestedPrivate | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass);

    //    TypeContext.Current.Children.Add(new()
    //    {
    //        Builder = closureType
    //    });

    //    foreach (var local in CurrentMethod.Locals)
    //        closureType.DefineField(local.Name, local.Builder.LocalType, FieldAttributes.Public);

    //    Type ret = typeof(object); // TODO: Add type inference

    //    if (context.type_name() != null)
    //        ret = CliHelpers.ResolveTypeName(context.type_name());

    //    var paramList = ResolveParameterList(context.parameter_list());

    //    MethodBuilder invokeMethod = closureType.DefineMethod(
    //        "Invoke",
    //        MethodAttributes.Assembly | MethodAttributes.HideBySig,
    //        CallingConventions.HasThis,
    //        ret,
    //        paramList.Select(p => p.Type).ToArray());

    //    MethodContext current = CurrentMethod;

    //    CurrentMethod = new()
    //    {
    //        Builder = invokeMethod,
    //        IL = invokeMethod.GetILGenerator()
    //    };

    //    Visit(context.expression());
    //    CurrentMethod.IL.Emit(OpCodes.Ret);

    //    closureType.CreateType();

    //    CurrentMethod = current;
    //    CurrentMethod.ClosureIndex++;
    //    return ret;
    //}

    public override Type VisitType_member([NotNull] DassieParser.Type_memberContext context)
    {
        EmitBuildLogMessage($"    Generating code for '{TypeContext.Current.Builder.FullName}::{context.Identifier().GetIdentifier()}'...", 2);

        if (context.Custom_Operator() != null)
        {
            DefineCustomOperator(context);
            return typeof(void);
        }

        if (context.Identifier().GetIdentifier() == TypeContext.Current.Builder.Name)
        {
            // Defer constructors for field initializers
            //TypeContext.Current.Constructors.Add(context);
            HandleConstructor(context);
            return typeof(void);
        }

        Type _tReturn = typeof(object);

        if (context.parameter_list() != null || _tReturn == typeof(void))
        {
            MethodContext mc = TypeContext.Current.GetMethod(context);
            MethodBuilder mb = mc.Builder;
            CurrentMethod = mc;

            _tReturn = mb.ReturnType;

            InjectClosureParameterInitializers();

            Type tReturn = null;

            if (context.expression() != null)
            {
                tReturn = _tReturn;
                _tReturn = Visit(context.expression());
            }

            if (context.type_name() == null)
                tReturn = _tReturn;

            if (tReturn != null && mc.UnresolvedReturnType)
                mb.SetReturnType(_tReturn);

            if (context.expression() == null)
                tReturn = _tReturn;

            if (TypeContext.Current.GenericParameters.Select(t => t.Builder).Contains(tReturn))
            {
                if (tReturn.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant))
                {
                    EmitErrorMessage(
                        context.type_name().Start.Line,
                        context.type_name().Start.Column,
                        context.type_name().GetText().Length,
                        DS0118_InvalidVariance,
                        $"Invalid variance: The type parameter '{TypeName(tReturn)}' must be covariantly valid on '{mb.Name}'. '{TypeName(tReturn)}' is contravariant.");
                }
            }

            if (TypeContext.Current.RequiredInterfaceImplementations.Any(m => m.Name == CurrentMethod.Builder.Name && m.ReturnType == tReturn && m.Parameters.SequenceEqual(CurrentMethod.Parameters.Select(p => p.Type))))
                TypeContext.Current.RequiredInterfaceImplementations.Remove(TypeContext.Current.RequiredInterfaceImplementations.First(m => m.Name == CurrentMethod.Builder.Name && m.ReturnType == tReturn && m.Parameters.SequenceEqual(CurrentMethod.Parameters.Select(p => p.Type))));

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
                        $"Expected expression of type '{TypeName(tReturn)}', but got type '{TypeName(_tReturn)}'.");
                }
            }

            if (context.expression() != null)
                CurrentMethod.IL.Emit(OpCodes.Ret);

            CurrentFile.FunctionParameterConstraints.TryGetValue(context.Identifier().GetIdentifier(), out Dictionary<string, string> constraintsForCurrentFunction);
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

            // Remove default implementation of static interface method

            if (TypeContext.Current.Methods.Count(m =>
                m.Builder != null
                && m.Builder.IsStatic
                && m.Builder.Name == mb.Name
                && m.Builder.ReturnType == mb.ReturnType
                && m.Builder.GetParameters().SequenceEqual(mb.GetParameters()))
                > 1)
            {
                MethodContext[] overloads = TypeContext.Current.Methods.Where(m =>
                    m.Builder != null
                    && m.Builder.IsStatic
                    && m.Builder.Name == mb.Name
                    && m.Builder.ReturnType == mb.ReturnType
                    && m.Builder.GetParameters().SequenceEqual(mb.GetParameters())).ToArray()[..^1];

                TypeContext.Current.Methods = TypeContext.Current.Methods.Except(overloads).ToList();
            }

            CurrentFile.Fragments.Add(new()
            {
                Color = Color.Function,
                Line = context.Identifier().Symbol.Line,
                Column = context.Identifier().Symbol.Column,
                Length = context.Identifier().GetIdentifier().Length,
                ToolTip = TooltipGenerator.Function(context.Identifier().GetIdentifier(), tReturn, _params.ToArray()),
                IsNavigationTarget = true
            });

            if (context.attribute() != null)
            {
                foreach ((int i, (Type attribType, CustomAttributeBuilder data, ConstructorInfo ctor, object[] attribData, _)) in AttributeHelpers.GetAttributeList(context.attribute(), ExpressionEvaluator.Instance).Index())
                {
                    if (attribType == typeof(EntryPointAttribute))
                    {
                        if (Context.EntryPointIsSet && !messages.Any(m => m.ErrorCode == ErrorKind.DS0191_AmbiguousEntryPoint))
                        {
                            EmitErrorMessage(
                                context.attribute()[i].Start.Line,
                                context.attribute()[i].Start.Column,
                                context.attribute()[i].GetText().Length,
                                DS0055_MultipleEntryPoints,
                                "Only one function can be declared as an entry point.");
                        }

                        if (!mb.IsStatic)
                        {
                            EmitErrorMessage(
                                context.Identifier().Symbol.Line,
                                context.Identifier().Symbol.Column,
                                context.Identifier().GetIdentifier().Length,
                                DS0035_EntryPointNotStatic,
                                "The application entry point must be static.");
                        }

                        if ((tReturn != typeof(void) && tReturn != typeof(int) && tReturn != typeof(uint)) || CurrentMethod.Parameters.Count > 1 || (CurrentMethod.Parameters.Count == 1 && CurrentMethod.Parameters[0].Type != typeof(string[])))
                        {
                            EmitErrorMessage(
                                context.Identifier().Symbol.Line,
                                context.Identifier().Symbol.Column,
                                context.Identifier().GetIdentifier().Length,
                                DS0201_EntryPointInvalidSignature,
                                $"""
                                The application entry point has an invalid signature ({ErrorMessageHelpers.GenerateParamList(CurrentMethod.Parameters.Select(p => p.Type).ToArray())} -> {TypeName(tReturn)}). The only allowed signatures are:
                                    * [] -> null
                                    * [] -> int
                                    * [] -> uint
                                    * [Vector[string]] -> null
                                    * [Vector[string]] -> int
                                    * [Vector[string]] -> uint
                                """);
                        }

                        Context.EntryPointIsSet = true;

                        Context.EntryPoint = mb;

                        AttributeHelpers.AddAttributeToCurrentMethod(typeof(EntryPointAttribute).GetConstructor(Type.EmptyTypes), Array.Empty<object>());
                    }
                    else if (attribType != null)
                        AttributeHelpers.EvaluateSpecialAttributeSemantics(context, ctor, attribData, true);
                }
            }

            CurrentMethod.ClosureContainerType?.CreateType();
            return typeof(void);
        }

        CreateFakeMethod();

        Type _type = typeof(object);

        if (context.expression() != null)
            _type = Visit(context.expression());

        Type type = _type;

        if (context.type_name() != null)
            type = SymbolResolver.ResolveTypeName(context.type_name());

        bool isAutoEvent = false;
        bool isAutoProperty = false;
        List<CustomAttributeBuilder> customAttribs = [];
        List<Type> modreq = [];
        List<Type> modopt = [];

        if (context.attribute() != null)
        {
            foreach ((Type attribType, CustomAttributeBuilder data, _, _, _) in AttributeHelpers.GetAttributeList(context.attribute(), ExpressionEvaluator.Instance))
            {
                if (attribType != null)
                {
                    customAttribs.Add(data);

                    if (attribType == typeof(VolatileAttribute))
                    {
                        modreq.Add(typeof(IsVolatile));
                        customAttribs.Remove(data);
                    }

                    if (attribType == typeof(EventAttribute))
                        isAutoEvent = true;

                    if (attribType == typeof(AutoAttribute))
                        isAutoProperty = true;
                }
            }
        }

        if (isAutoEvent && isAutoProperty)
        {
            EmitErrorMessage(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetIdentifier().Length,
                DS0172_EventAndProperty,
                $"The attributes '<Auto>' and '<Event>' cannot be combined.");
        }

        string memberKind = !isAutoProperty ? "field" : "property";
        string memberKindPlural = !isAutoProperty ? "fields" : "properties";

        if (TypeContext.Current.IsImmutable && context.Var() != null)
        {
            EmitErrorMessage(
                context.Var().Symbol.Line,
                context.Var().Symbol.Column,
                context.Var().GetText().Length,
                DS0151_VarFieldInImmutableType,
                $"The 'var' modifier is invalid on members of immutable value types. {memberKindPlural.ToUpper()} of immutable types are not allowed to be mutable.");
        }

        bool isInitOnly = TypeContext.Current.IsImmutable || context.Val() != null;
        FieldAttributes fieldAttribs = AttributeHelpers.GetFieldAttributes(context.member_access_modifier(), context.member_oop_modifier(), context.member_special_modifier(), isInitOnly);

        if (TypeContext.Current.Builder.IsInterface && !fieldAttribs.HasFlag(FieldAttributes.Static))
        {
            EmitErrorMessage(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetIdentifier().Length,
                DS0158_InstanceFieldInTemplate,
                $"Template types cannot contain instance {memberKindPlural}.");
        }

        if ((type.IsByRef /*|| type.IsByRefLike*/) && !TypeContext.Current.IsByRefLike)
        {
            EmitErrorMessage(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetIdentifier().Length,
                DS0150_ByRefFieldInNonByRefLikeType,
                $"Invalid {memberKind} type '{TypeName(type)}'. References are only valid as part of ByRef-like value types (val& type).");
        }

        // Auto-implemented property
        if (isAutoProperty)
        {
            if (context.member_special_modifier() != null && context.member_special_modifier().Any(l => l.Literal() != null))
            {
                ITerminalNode node = context.member_special_modifier().First(l => l.Literal() != null).Literal();

                EmitErrorMessage(
                    node.Symbol.Line,
                    node.Symbol.Column,
                    node.GetText().Length,
                    DS0167_PropertyLiteral,
                    "The modifier 'literal' is not valid on properties.");
            }

            string propName = context.Identifier().GetIdentifier();
            FieldBuilder backingField = TypeContext.Current.Builder.DefineField(
                SymbolNameGenerator.GetPropertyBackingFieldName(propName),
                type,
                FieldAttributes.Private);

            PropertyBuilder pb = TypeContext.Current.Builder.DefineProperty(
                propName,
                PropertyAttributes.None,
                type, []);

            TypeContext.Current.Properties.Add(pb);

            (MethodAttributes attribs, _, _, CallingConventions callingConventions) = AttributeHelpers.GetMethodAttributes(context.member_access_modifier(), context.member_oop_modifier(), context.member_special_modifier(), []);
            attribs |= MethodAttributes.SpecialName;

            if (!attribs.HasFlag(MethodAttributes.HideBySig))
                attribs |= MethodAttributes.HideBySig;

            MethodBuilder getter = TypeContext.Current.Builder.DefineMethod($"get_{propName}", attribs, callingConventions, type, []);
            ILGenerator ilGet = getter.GetILGenerator();
            ilGet.Emit(OpCodes.Ldarg_0);
            ilGet.Emit(OpCodes.Ldfld, backingField);
            ilGet.Emit(OpCodes.Ret);
            pb.SetGetMethod(getter);

            if (context.Var() != null)
            {
                MethodBuilder setter = TypeContext.Current.Builder.DefineMethod($"set_{propName}", attribs, typeof(void), [type]);
                ILGenerator ilSet = setter.GetILGenerator();
                ilSet.Emit(OpCodes.Ldarg_0);
                ilSet.Emit(OpCodes.Ldarg_1);
                ilSet.Emit(OpCodes.Stfld, backingField);
                ilSet.Emit(OpCodes.Ret);
                pb.SetSetMethod(setter);
            }

            CurrentFile.Fragments.Add(new()
            {
                Color = Color.Property,
                Column = context.Identifier().Symbol.Column,
                Line = context.Identifier().Symbol.Line,
                Length = context.Identifier().GetIdentifier().Length,
                ToolTip = TooltipGenerator.Property(pb),
                IsNavigationTarget = true
            });

            return typeof(void);
        }

        if (isAutoEvent)
        {
            if (context.member_special_modifier() != null && context.member_special_modifier().Any(l => l.Literal() != null))
            {
                ITerminalNode node = context.member_special_modifier().First(l => l.Literal() != null).Literal();

                EmitErrorMessage(
                    node.Symbol.Line,
                    node.Symbol.Column,
                    node.GetText().Length,
                    DS0167_PropertyLiteral,
                    "The modifier 'literal' is not valid on events.");
            }

            if (!(type.BaseType == typeof(Delegate) || type.BaseType == typeof(MulticastDelegate)))
            {
                EmitErrorMessage(
                    context.type_name().Start.Line,
                    context.type_name().Start.Column,
                    context.type_name().GetText().Length,
                    DS0174_EventFieldTypeNotDelegate,
                    "Event must be of a delegate type.");

                return typeof(void);
            }

            string eventName = context.Identifier().GetIdentifier();

            FieldBuilder eventField = TypeContext.Current.Builder.DefineField(
                eventName,
                type,
                fieldAttribs);

            TypeContext.Current.Fields.Add(new()
            {
                Builder = eventField,
                Name = eventName
            });

            EventBuilder eb = TypeContext.Current.Builder.DefineEvent(
                eventName,
                EventAttributes.None,
                type);

            MethodAttributes handlerMethodAttribs = MethodAttributes.Public | MethodAttributes.SpecialName;

            if (eventField.IsStatic)
                handlerMethodAttribs |= MethodAttributes.Static;

            MethodBuilder addMethod = TypeContext.Current.Builder.DefineMethod(
                $"add_{eventName}",
                handlerMethodAttribs);

            addMethod.SetReturnType(typeof(void));
            addMethod.SetParameters(type);

            ILGenerator addMethodIL = addMethod.GetILGenerator();

            MethodContext current = CurrentMethod;

            MethodContext addContext = new()
            {
                Builder = addMethod,
                IL = addMethodIL
            };

            if (context.property_or_event_block() != null && context.property_or_event_block().add_handler().Length > 0)
            {
                if (context.property_or_event_block().add_handler().Length > 1)
                {
                    EmitErrorMessage(
                        context.property_or_event_block().add_handler()[1].Start.Line,
                        context.property_or_event_block().add_handler()[1].Start.Column,
                        context.property_or_event_block().add_handler()[1..].SelectMany(a => a.GetText()).Count(),
                        DS0173_EventHasMultipleHandlers,
                        "An event can only contain one 'add' handler.");
                }

                Visit(context.property_or_event_block().add_handler()[0].expression());
                addMethodIL.Emit(OpCodes.Ret);
            }
            else
                EventDefaultHandlerCodeGeneration.GenerateDefaultAddHandlerImplementation(eventField);

            MethodBuilder removeMethod = TypeContext.Current.Builder.DefineMethod(
                $"remove_{eventName}",
                handlerMethodAttribs);

            removeMethod.SetReturnType(typeof(void));
            removeMethod.SetParameters(type);

            ILGenerator removeMethodIL = removeMethod.GetILGenerator();

            MethodContext removeContext = new()
            {
                Builder = removeMethod,
                IL = removeMethodIL
            };

            if (context.property_or_event_block() != null && context.property_or_event_block().remove_handler().Length > 0)
            {
                if (context.property_or_event_block().remove_handler().Length > 1)
                {
                    EmitErrorMessage(
                        context.property_or_event_block().remove_handler()[1].Start.Line,
                        context.property_or_event_block().remove_handler()[1].Start.Column,
                        context.property_or_event_block().remove_handler()[1..].SelectMany(a => a.GetText()).Count(),
                        DS0173_EventHasMultipleHandlers,
                        "An event can only contain one 'remove' handler.");
                }

                Visit(context.property_or_event_block().remove_handler()[0].expression());
                removeMethodIL.Emit(OpCodes.Ret);
            }
            else
                EventDefaultHandlerCodeGeneration.GenerateDefaultRemoveHandlerImplementation(eventField);

            if (context.property_or_event_block() != null && (context.property_or_event_block().add_handler().Length == 0 ^ context.property_or_event_block().remove_handler().Length == 0))
            {
                EmitErrorMessage(
                    context.Identifier().Symbol.Line,
                    context.Identifier().Symbol.Column,
                    context.Identifier().GetIdentifier().Length,
                    DS0175_EventMissingHandlers,
                    $"Event '{eventName}' is missing a{(context.property_or_event_block().add_handler().Length == 0 ? "n" : "")} '{(context.property_or_event_block().add_handler().Length == 0 ? "add" : "remove")}' handler.");
            }

            eb.SetAddOnMethod(addMethod);
            eb.SetRemoveOnMethod(removeMethod);

            CurrentMethod = current;
            return typeof(void);
        }

        MetaFieldInfo mfi = TypeContext.Current.GetField(context);
        FieldBuilder fb = (FieldBuilder)mfi.Builder;

        return typeof(void);
    }

    public static (Type Type, DassieParser.ParameterContext Context)[] ResolveParameterList(DassieParser.Parameter_listContext paramList, bool noErrors = false)
    {
        if (paramList == null)
            return Array.Empty<(Type, DassieParser.ParameterContext)>();

        List<(Type, DassieParser.ParameterContext)> types = new();

        foreach (var param in paramList.parameter())
            types.Add((ResolveParameter(param, noErrors), param));

        return types.ToArray();
    }

    public static Type ResolveParameter(DassieParser.ParameterContext param, bool noErrors = false)
    {
        string name = param.Identifier().GetIdentifier();
        Type t = typeof(object);

        if (param.type_name() != null)
        {
            t = SymbolResolver.ResolveTypeName(param.type_name(), noErrors: noErrors);

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

        if (!noErrors && t.IsGenericTypeParameter && t.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Covariant))
        {
            EmitErrorMessage(
                param.type_name().Start.Line,
                param.type_name().Start.Column,
                param.type_name().GetText().Length,
                DS0118_InvalidVariance,
                $"Invalid variance: The type parameter '{t.Name}' must be contravariantly valid on '{CurrentMethod.Builder.Name}'. '{t.Name}' is covariant.");
        }

        CurrentFile.Fragments.Add(new()
        {
            Color = Color.LocalValue,
            Line = param.Identifier().Symbol.Line,
            Column = param.Identifier().Symbol.Column,
            Length = param.Identifier().GetIdentifier().Length,
        });

        return t;
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

    public override Type VisitFile_body([NotNull] DassieParser.File_bodyContext context)
    {
        if (context.full_program() != null && context.full_program().Length > 0)
        {
            foreach (IParseTree prog in context.full_program())
                Visit(prog);
        }

        List<IParseTree> ignoredTrees = [];

        if (context.type_member() != null && context.type_member().Length != 0)
        {
            if (CurrentFile.LocalTopLevelFunctionContainerType != null)
            {
                TypeContext.Current = TypeContext.GetForType(CurrentFile.LocalTopLevelFunctionContainerType);
                foreach (DassieParser.Type_memberContext localFunc in CurrentFile.LocalTopLevelFunctions)
                {
                    ignoredTrees.Add(localFunc);
                    Visit(localFunc);
                }
            }

            if (Context.GlobalTopLevelFunctionContainerType != null)
            {
                TypeContext.Current = TypeContext.GetForType(Context.GlobalTopLevelFunctionContainerType);
                foreach (DassieParser.Type_memberContext globalFunc in Context.GlobalTopLevelFunctions.Where(f => f.DeclaringFile == CurrentFile).Select(f => f.Function))
                {
                    ignoredTrees.Add(globalFunc);
                    Visit(globalFunc);
                }
            }
        }

        if (context.expression() == null || context.expression().Length == 0)
            return typeof(void);

        if (Context.Files.Count > 0)
        {
            if ((context.expression().Length == 0 && Context.FilePaths.Count < 2) || (context.expression().Length == 0 && Context.FilePaths.Last() == CurrentFile.Path && Context.ShouldThrowDS0027))
                EmitWarningMessage(0, 0, context.GetText().Length, DS0027_EmptyProgram, "Program contains no executable code.");

            Context.ShouldThrowDS0027 = context.expression().Length == 0;
        }

        if (context.expression().Length > 0)
        {
            if (Context.EntryPointIsSet)
            {
                EmitErrorMessage(
                    context.expression()[0].Start.Line,
                    context.expression()[0].Start.Column,
                    context.expression()[0].GetText().Length,
                    DS0191_AmbiguousEntryPoint,
                    "The program contains multiple implicit or explicit entry points.");
            }

            Context.EntryPointIsSet = true;
        }

        TypeBuilder tb = Context.Module.DefineType($"{(string.IsNullOrEmpty(CurrentFile.ExportedNamespace) ? "" : $"{CurrentFile.ExportedNamespace}.")}Program");

        TypeContext tc = new()
        {
            Builder = tb
        };

        EmitBuildLogMessage($"    Generating code for '{tb.FullName}::Main'...", 2);

        tc.FilesWhereDefined.Add(CurrentFile.Path);

        Context.EntryPointIsSet = true;
        ConstructorInfo entryPointCon = typeof(EntryPointAttribute).GetConstructor(Type.EmptyTypes);
        CustomAttributeBuilder entryPointAttribute = new(entryPointCon, []);

        MethodBuilder mb = tb.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(int), new Type[] { typeof(string[]) });

        ILGenerator il = mb.GetILGenerator();
        MethodContext mc = new()
        {
            Builder = mb,
            IL = il
        };

        AttributeHelpers.AddAttributeToCurrentMethod(entryPointCon, []);

        mc.Parameters.Add(new("args", typeof(string[]), mb.DefineParameter(1, ParameterAttributes.None, "args"), 0, false));
        mc.FilesWhereDefined.Add(CurrentFile.Path);
        tc.Methods.Add(mc);
        Context.Types.Add(tc);

        InjectClosureParameterInitializers();

        Type ret = typeof(void);

        if (context.children != null && context.children.Count > 0)
        {
            foreach (IParseTree child in context.children.Take(context.children.Count - 1))
            {
                if (child is DassieParser.Full_programContext)
                    continue;

                if (ignoredTrees.Contains(child))
                    continue;

                Type _t = Visit(child);

                if (_t != typeof(void) && _t != null && !CurrentMethod.SkipPop)
                {
#if ENABLE_DS0125
                ParserRuleContext rule = (ParserRuleContext)tree;
                string text = CurrentFile.CharStream.GetText(new(rule.Start.StartIndex, rule.Stop.StopIndex)).Trim();

                if (rule is not DassieParser.AssignmentContext and not DassieParser.Local_declaration_or_assignmentContext)
                {
                    EmitWarningMessage(
                        rule.Start.Line,
                        rule.Start.Column,
                        text.Length,
                        DS0125_UnusedValue,
                        $"Result of expression '{text}' is not used. Use 'ignore' to explicitly discard a value.");
                }
#endif // ENABLE_DS0125

                    CurrentMethod.IL.Emit(OpCodes.Pop);
                }

                if (CurrentMethod.SkipPop)
                    CurrentMethod.SkipPop = false;
            }

            // Last expression is like return statement
            ret = Visit(context.children.Last());
        }

        if (ret != typeof(void) && ret != typeof(int) && ret != typeof(int) && ret != null)
        {
            if (CanBeConverted(ret, typeof(int)))
            {
                EmitConversionOperator(ret, typeof(int));
                ret = typeof(int);
            }
            else
            {
                EmitErrorMessage(context.expression().Last().Start.Line,
                    context.expression().Last().Start.Column,
                    context.expression().Last().GetText().Length,
                    DS0050_ExpectedIntegerReturnValue,
                    $"Expected expression of type 'int32', 'uint32' or 'null', but got type '{TypeName(ret)}'.",
                    tip: "You may use the function 'ignore' to discard a value and return 'null'.");

                return ret;
            }
        }

        if (ret != typeof(int) && ret != null)
            CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)0);

        CurrentMethod.IL.Emit(OpCodes.Ret);

        Context.EntryPoint = mb;

        CurrentMethod.ClosureContainerType?.CreateType();
        TypeContext.Current.FinishedType = tb.CreateType();
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
        Expression constExpr = ExpressionEvaluator.Instance.Visit(context);

        if (constExpr != null)
        {
            EmitConst(constExpr.Value);
            return constExpr.Type;
        }

        Type t = Visit(context.expression()[0]);
        EnsureBoolean(t, throwError: false);

        Type t2 = Visit(context.expression()[1]);
        EnsureBoolean(t2, throwError: false);

        MethodInfo op_eq = t.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static, null, [t, t2], null);
        MethodInfo op_ineq = t.GetMethod("op_Inequality", BindingFlags.Public | BindingFlags.Static, null, [t, t2], null);

        if (IsValueTuple(t) && IsValueTuple(t2))
        {
            if (t.GetGenericArguments().Length != t2.GetGenericArguments().Length)
            {
                CurrentMethod.IL.Emit(OpCodes.Pop);
                CurrentMethod.IL.Emit(OpCodes.Pop);
                EmitLdcI4(0);
                return typeof(bool);
            }

            EmitTupleEquality(t, t2, context.op.Text == "==");
            return typeof(bool);
        }

        if ((op_eq == null && op_ineq == null) || (IsNumericType(t) && IsNumericType(t2)))
        {
            if ((IsNumericType(t) && IsNumericType(t2)) || (IsBoolean(t) && IsBoolean(t2)))
            {
                if (IsFloatingPointType(t) && !IsFloatingPointType(t2))
                {
                    CurrentMethod.IL.Emit(OpCodes.Conv_R8);
                }
                else if (IsFloatingPointType(t2) && !IsFloatingPointType(t))
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

        MethodInfo equalsMethod = t.GetMethods().Where(m => m.Name == "Equals").First();

        if (context.op.Text == "==")
        {
            if (op_eq == null)
            {
                //EmitErrorMessage(
                //        context.op.Line,
                //        context.op.Column,
                //        context.op.Text.Length,
                //        DS0036_ArithmeticError,
                //        $"The type '{TypeName(t)}' does not implement an equality operation with operand type '{TypeName(t2)}'.",
                //        Path.GetFileName(CurrentFile.Path));

                CurrentMethod.IL.Emit(OpCodes.Callvirt, equalsMethod);
                return equalsMethod.ReturnType;
            }

            CurrentMethod.IL.EmitCall(OpCodes.Call, op_eq, null);
            return op_eq.ReturnType;
        }
        else
        {
            if (op_ineq == null)
            {
                //EmitErrorMessage(
                //        context.op.Line,
                //        context.op.Column,
                //        context.op.Text.Length,
                //        DS0036_ArithmeticError,
                //        $"The type '{TypeName(t)}' does not implement an inequality operation with operand type '{TypeName(t2)}'.",
                //        Path.GetFileName(CurrentFile.Path));

                CurrentMethod.IL.Emit(OpCodes.Callvirt, equalsMethod);
                EmitLdcI4(0);
                CurrentMethod.IL.Emit(OpCodes.Ceq);
                return typeof(bool);
            }

            CurrentMethod.IL.EmitCall(OpCodes.Call, op_ineq, null);
            return op_ineq.ReturnType;
        }
    }

    public override Type VisitComparison_expression([NotNull] DassieParser.Comparison_expressionContext context)
    {
        Expression constExpr = ExpressionEvaluator.Instance.Visit(context);

        if (constExpr != null)
        {
            EmitConst(constExpr.Value);
            return constExpr.Type;
        }

        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (IsNumericType(t) && IsNumericType(t2))
        {
            if (IsFloatingPointType(t) && !IsFloatingPointType(t2))
            {
                CurrentMethod.IL.Emit(OpCodes.Conv_R8);
            }
            else if (IsFloatingPointType(t2) && !IsFloatingPointType(t))
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
                    $"The type '{TypeName(t)}' does not implement a comparison operation with operand type '{TypeName(t2)}'.",
                    Path.GetFileName(CurrentFile.Path));

            return typeof(bool);
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
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
    //                $"The type '{TypeName(t)}' does not implement the unary negation operation.",
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
    //                $"The type '{TypeName(t)}' does not implement the unary plus operation.",
    //                Path.GetFileName(CurrentFile.Path));

    //        return t;
    //    }

    //    CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);

    //    return t;
    //}

    public override Type VisitLogical_negation_expression([NotNull] DassieParser.Logical_negation_expressionContext context)
    {
        Expression constExpr = ExpressionEvaluator.Instance.Visit(context);

        if (constExpr != null)
        {
            EmitConst(constExpr.Value);
            return constExpr.Type;
        }

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
                $"The type '{TypeName(t)}' does not implement a logical negation operation.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
    }

    public override Type VisitLogical_and_expression([NotNull] DassieParser.Logical_and_expressionContext context)
    {
        Expression constExpr = ExpressionEvaluator.Instance.Visit(context);

        if (constExpr != null)
        {
            EmitConst(constExpr.Value);
            return constExpr.Type;
        }

        List<Type> types = [];

        Label falseLabel = CurrentMethod.IL.DefineLabel();
        Label trueLabel = CurrentMethod.IL.DefineLabel();

        foreach (DassieParser.ExpressionContext expr in context.expression()[..^1])
        {
            Type t = Visit(context.expression()[0]);
            EnsureBoolean(t, throwError: false);

            if (IsBoolean(t))
                CurrentMethod.IL.Emit(OpCodes.Brfalse, falseLabel);

            types.Add(t);
        }

        Type t2 = Visit(context.expression()[^1]);
        EnsureBoolean(t2, throwError: false);
        CurrentMethod.IL.Emit(OpCodes.Br, trueLabel);

        if (types.All(IsBoolean))
        {
            CurrentMethod.IL.MarkLabel(falseLabel);
            EmitLdcI4(0);
            CurrentMethod.IL.MarkLabel(trueLabel);
            return typeof(bool);
        }

        EmitErrorMessage(
            context.Double_Ampersand()[0].Symbol.Line,
            context.Double_Ampersand()[0].Symbol.Column,
            context.Double_Ampersand()[0].GetText().Length,
            DS0002_MethodNotFound,
            $"The 'logical and' operation is only supported for operands of type '{typeof(bool).FullName}'.",
            Path.GetFileName(CurrentFile.Path));

        return types[^1];
    }

    public override Type VisitLogical_or_expression([NotNull] DassieParser.Logical_or_expressionContext context)
    {
        Expression constExpr = ExpressionEvaluator.Instance.Visit(context);

        if (constExpr != null)
        {
            EmitConst(constExpr.Value);
            return constExpr.Type;
        }

        List<Type> types = [];

        Label trueLabel = CurrentMethod.IL.DefineLabel();
        Label falseLabel = CurrentMethod.IL.DefineLabel();

        foreach (DassieParser.ExpressionContext expr in context.expression()[..^1])
        {
            Type t = Visit(context.expression()[0]);
            EnsureBoolean(t, throwError: false);

            if (IsBoolean(t))
                CurrentMethod.IL.Emit(OpCodes.Brtrue, trueLabel);

            types.Add(t);
        }

        Type t2 = Visit(context.expression()[^1]);
        EnsureBoolean(t2, throwError: false);
        CurrentMethod.IL.Emit(OpCodes.Br, falseLabel);

        if (types.All(IsBoolean))
        {
            CurrentMethod.IL.MarkLabel(trueLabel);
            EmitLdcI4(1);
            CurrentMethod.IL.MarkLabel(falseLabel);
            return typeof(bool);
        }

        EmitErrorMessage(
            context.Double_Bar()[0].Symbol.Line,
            context.Double_Bar()[0].Symbol.Column,
            context.Double_Bar()[0].GetText().Length,
            DS0002_MethodNotFound,
            $"The 'logical or' operation is only supported for operands of type '{typeof(bool).FullName}'.",
            Path.GetFileName(CurrentFile.Path));

        return types[^1];
    }

    public override Type VisitOr_expression([NotNull] DassieParser.Or_expressionContext context)
    {
        Expression constExpr = ExpressionEvaluator.Instance.Visit(context);

        if (constExpr != null)
        {
            EmitConst(constExpr.Value);
            return constExpr.Type;
        }

        Type t = Visit(context.expression()[0]);
        EnsureBoolean(t, throwError: false);

        Type t2 = Visit(context.expression()[1]);
        EnsureBoolean(t2, throwError: false);

        if (IsBoolean(t) && IsBoolean(t2))
        {
            CurrentMethod.IL.Emit(OpCodes.Or);
            return typeof(bool);
        }

        if (IsNumericType(t) || (t == t2 && t.IsEnum))
        {
            CurrentMethod.IL.Emit(OpCodes.Or);
            return t;
        }

        // List union
        // Same as concatenation (+), except duplicates get filtered out

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) && t == t2)
        {
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("Concat").MakeGenericMethod(t.GetGenericArguments()[0]));
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethods().First(m => m.Name == "Distinct").MakeGenericMethod(t.GetGenericArguments()[0]));
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(t.GetGenericArguments()[0]));
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
                $"The type '{TypeName(t)}' does not implement a bitwise or operation with operand type '{TypeName(t2)}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
    }

    public override Type VisitAnd_expression([NotNull] DassieParser.And_expressionContext context)
    {
        Expression expr = ExpressionEvaluator.Instance.Visit(context);

        if (expr != null)
        {
            EmitConst(expr.Value);
            return expr.Type;
        }

        Type t = Visit(context.expression()[0]);
        EnsureBoolean(t, throwError: false);

        Type t2 = Visit(context.expression()[1]);
        EnsureBoolean(t2, throwError: false);

        if (IsBoolean(t) && IsBoolean(t2))
        {
            CurrentMethod.IL.Emit(OpCodes.And);
            return typeof(bool);
        }

        if (IsNumericType(t) || (t == t2 && t.IsEnum))
        {
            CurrentMethod.IL.Emit(OpCodes.And);
            return t;
        }

        // List intersection

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) && t == t2)
        {
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethods().First(m => m.Name == "Intersect").MakeGenericMethod(t.GetGenericArguments()[0]));
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(t.GetGenericArguments()[0]));
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
                $"The type '{TypeName(t)}' does not implement a bitwise and operation with operand type '{TypeName(t2)}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
    }

    public override Type VisitXor_expression([NotNull] DassieParser.Xor_expressionContext context)
    {
        Expression expr = ExpressionEvaluator.Instance.Visit(context);

        if (expr != null)
        {
            EmitConst(expr.Value);
            return expr.Type;
        }

        Type t = Visit(context.expression()[0]);
        EnsureBoolean(t, throwError: false);

        Type t2 = Visit(context.expression()[1]);
        EnsureBoolean(t, throwError: false);

        if (IsNumericType(t) || (t == t2 && t.IsEnum) || (IsBoolean(t) && IsBoolean(t2)))
        {
            CurrentMethod.IL.Emit(OpCodes.Xor);
            return t;
        }

        // Symmetric difference of lists
        // ^(a, b) = Union(Except(a, b), Except(b, a))

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) && t == t2)
        {
            // LocalIndex - 1: List 2
            CurrentMethod.LocalIndex++;
            CurrentMethod.IL.DeclareLocal(t2);
            EmitStloc(CurrentMethod.LocalIndex);

            // LocalIndex: List 1
            CurrentMethod.LocalIndex++;
            CurrentMethod.IL.DeclareLocal(t);
            EmitStloc(CurrentMethod.LocalIndex);

            EmitLdloc(CurrentMethod.LocalIndex);
            EmitLdloc(CurrentMethod.LocalIndex - 1);
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethods().First(m => m.Name == "Except").MakeGenericMethod(t.GetGenericArguments()[0]));

            EmitLdloc(CurrentMethod.LocalIndex - 1);
            EmitLdloc(CurrentMethod.LocalIndex);
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethods().First(m => m.Name == "Except").MakeGenericMethod(t.GetGenericArguments()[0]));

            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethods().First(m => m.Name == "Union").MakeGenericMethod(t.GetGenericArguments()[0]));
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(t.GetGenericArguments()[0]));
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
                $"The type '{TypeName(t)}' does not implement an exclusive or operation with operand type '{TypeName(t2)}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
    }

    public override Type VisitBitwise_complement_expression([NotNull] DassieParser.Bitwise_complement_expressionContext context)
    {
        Expression expr = ExpressionEvaluator.Instance.Visit(context);

        if (expr != null)
        {
            EmitConst(expr.Value);
            return expr.Type;
        }

        Type t = Visit(context.expression());

        if (IsNumericType(t) || t.IsEnum)
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
                $"The type '{TypeName(t)}' does not implement a complement operation.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
    }

    public override Type VisitMultiply_expression([NotNull] DassieParser.Multiply_expressionContext context)
    {
        Expression expr = ExpressionEvaluator.Instance.Visit(context);

        if (expr != null)
        {
            EmitConst(expr.Value);
            return expr.Type;
        }

        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (IsNumericType(t))
        {
            if (t != t2)
                EmitConversionOperator(t2, t);

            EmitMul(t);
            return t;
        }

        // String repetition operator
        // "ABC" * 3 = "ABCABCABC"

        if (t == typeof(string) && IsIntegerType(t2))
        {
            if (t2 != typeof(int))
                EmitConversionOperator(t2, typeof(int));

            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("Repeat").MakeGenericMethod(typeof(string)));
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, [typeof(IEnumerable<string>)]));
            return t;
        }

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) && IsIntegerType(t2))
        {
            if (t2 != typeof(int))
                EmitConversionOperator(t2, typeof(int));

            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("Repeat").MakeGenericMethod(t));
            CurrentMethod.IL.Emit(OpCodes.Ldnull);
            CurrentMethod.IL.Emit(OpCodes.Ldftn, typeof(Value).GetMethods().Where(m => m.Name == "id").ToArray()[1].MakeGenericMethod(t));
            CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(Func<,>).MakeGenericType(t.GetGenericArguments()[0], t).GetConstructor([typeof(object), typeof(nint)]));
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethods().Where(m => m.Name == "SelectMany").ToArray()[1].MakeGenericMethod(t, t.GetGenericArguments()[0]));
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(t.GetGenericArguments()[0]));
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
                $"The type '{TypeName(t)}' does not implement a multiplication operation with operand type '{TypeName(t2)}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
    }

    public override Type VisitDivide_expression([NotNull] DassieParser.Divide_expressionContext context)
    {
        Expression expr = ExpressionEvaluator.Instance.Visit(context);

        if (expr != null)
        {
            EmitConst(expr.Value);
            return expr.Type;
        }

        Type t = Visit(context.expression()[0]);

        ExpressionEvaluator.Instance.RequireNonZeroValue = true;
        Type t2 = Visit(context.expression()[1]);

        if (IsNumericType(t))
        {
            // Emit conversion operators to support dividing numeric primitives of different types.
            // t1 / t2 always returns expression of type t1.

            if (t != t2)
                EmitConversionOperator(t2, t);

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
                $"The type '{TypeName(t)}' does not implement a division operation with operand type '{TypeName(t2)}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
    }

    public override Type VisitAddition_expression([NotNull] DassieParser.Addition_expressionContext context)
    {
        Expression expr = ExpressionEvaluator.Instance.Visit(context);

        if (expr != null)
        {
            EmitConst(expr.Value);
            return expr.Type;
        }

        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (IsNumericType(t) && IsNumericType(t2))
        {
            if (t != t2)
                EmitConversionOperator(t2, t);

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
                CurrentMethod.IL.EmitCall(GetCallOpCode(t2), toString, null);
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
                CurrentMethod.IL.EmitCall(GetCallOpCode(t), toString, null);

                Visit(context.expression()[1]);
            }

            MethodInfo concat = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
            CurrentMethod.IL.EmitCall(OpCodes.Call, concat, null);

            return typeof(string);
        }

        // List concatenation
        // [1, 2, 3] + [4, 5, 6] = [1, 2, 3, 4, 5, 6]

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) && t == t2)
        {
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("Concat").MakeGenericMethod(t.GetGenericArguments()[0]));
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(t.GetGenericArguments()[0]));
            return t;
        }

        MethodInfo op = t.GetMethod("op_Addition", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Plus().Symbol.Line,
                context.Plus().Symbol.Column,
                context.Plus().GetText().Length,
                DS0002_MethodNotFound,
                $"The type '{TypeName(t)}' does not implement an addition operation with operand type '{TypeName(t2)}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
    }

    public override Type VisitSubtraction_expression([NotNull] DassieParser.Subtraction_expressionContext context)
    {
        Expression constExpr = ExpressionEvaluator.Instance.Visit(context);

        if (constExpr != null)
        {
            EmitConst(constExpr.Value);
            return constExpr.Type;
        }

        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (IsNumericType(t))
        {
            if (t != t2)
                EmitConversionOperator(t2, t);

            EmitSub(t);
            return t;
        }

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) && t == t2)
        {
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethods().First(m => m.Name == "Except").MakeGenericMethod(t.GetGenericArguments()[0]));
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(t.GetGenericArguments()[0]));
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
                $"The type '{TypeName(t)}' does not implement a subtraction operation with operand type '{TypeName(t2)}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
    }

    public override Type VisitRemainder_expression([NotNull] DassieParser.Remainder_expressionContext context)
    {
        Expression constExpr = ExpressionEvaluator.Instance.Visit(context);

        if (constExpr != null)
        {
            EmitConst(constExpr.Value);
            return constExpr.Type;
        }

        Type t = Visit(context.expression()[0]);

        ExpressionEvaluator.Instance.RequireNonZeroValue = true;
        Type t2 = Visit(context.expression()[1]);

        if (IsNumericType(t))
        {
            if (t != t2)
                EmitConversionOperator(t2, t);

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
                $"The type '{TypeName(t)}' does not implement a remainder operation with operand type '{TypeName(t2)}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
    }

    // a %% b = (a % b + b) % b
    public override Type VisitModulus_expression([NotNull] DassieParser.Modulus_expressionContext context)
    {
        Expression constExpr = ExpressionEvaluator.Instance.Visit(context);

        if (constExpr != null)
        {
            EmitConst(constExpr.Value);
            return constExpr.Type;
        }

        Type t = Visit(context.expression()[0]);

        ExpressionEvaluator.Instance.RequireNonZeroValue = true;
        Type t2 = Visit(context.expression()[1]);

        if (IsNumericType(t))
        {
            if (t != t2)
                EmitConversionOperator(t2, t);

            CurrentMethod.IL.DeclareLocal(t);
            CurrentMethod.LocalIndex++;

            EmitStloc(CurrentMethod.LocalIndex);

            EmitLdloc(CurrentMethod.LocalIndex);
            EmitRem(t);

            EmitLdloc(CurrentMethod.LocalIndex);
            EmitAdd(t);

            EmitLdloc(CurrentMethod.LocalIndex);
            EmitRem(t);
            return t;
        }

        MethodInfo op = t.GetMethod("op_Modulus", BindingFlags.Public | BindingFlags.Static, null, new Type[] { t, t2 }, null);

        if (op == null)
        {
            EmitErrorMessage(
                context.Double_Percent().Symbol.Line,
                context.Double_Percent().Symbol.Column,
                context.Double_Percent().GetText().Length,
                DS0002_MethodNotFound,
                $"The type '{TypeName(t)}' does not implement a remainder operation with operand type '{TypeName(t2)}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
    }

    public override Type VisitPower_expression([NotNull] DassieParser.Power_expressionContext context)
    {
        Expression constExpr = ExpressionEvaluator.Instance.Visit(context);

        if (constExpr != null)
        {
            EmitConst(constExpr.Value);
            return constExpr.Type;
        }

        Type t = Visit(context.expression()[0]);

        if (t != typeof(double))
            EmitConversionOperator(t, typeof(double));

        Type t2 = Visit(context.expression()[1]);

        if (t2 != typeof(double))
            EmitConversionOperator(t2, typeof(double));

        MethodInfo m = typeof(Math)
            .GetMethod("Pow",
            [
                typeof(double),
                typeof(double)
            ]);

        if (m == null)
        {
            EmitErrorMessage(
                context.Double_Asterisk().Symbol.Line,
                context.Double_Asterisk().Symbol.Column,
                context.Double_Asterisk().GetText().Length,
                DS0036_ArithmeticError,
                $"The type '{TypeName(t)}' does not implement a exponentiation operation with operand type '{TypeName(t2)}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, m, null);
        return m.ReturnType;
    }

    public override Type VisitLeft_shift_expression([NotNull] DassieParser.Left_shift_expressionContext context)
    {
        Expression constExpr = ExpressionEvaluator.Instance.Visit(context);

        if (constExpr != null)
        {
            EmitConst(constExpr.Value);
            return constExpr.Type;
        }

        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (IsIntegerType(t))
        {
            CurrentMethod.IL.Emit(OpCodes.Shl);
            return t;
        }

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
        {
            Type listType = t.GetGenericArguments()[0];

            if (t2 != listType)
            {
                if (CanBeConverted(t2, listType))
                    EmitConversionOperator(t2, listType);
                else
                {
                    EmitErrorMessage(
                        context.expression()[1].Start.Line,
                        context.expression()[1].Start.Column,
                        context.expression()[1].GetText().Length,
                        DS0154_ListAppendIncompatibleElement,
                        $"An expression of type '{TypeName(t2)}' cannot be appended to a list of type '{TypeName(t)}'.");
                }
            }

            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("Append").MakeGenericMethod(t.GetGenericArguments()[0]));
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(t.GetGenericArguments()[0]));
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
                $"The type '{TypeName(t)}' does not implement a left shift operation with operand type '{TypeName(t2)}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
    }

    public override Type VisitRight_shift_expression([NotNull] DassieParser.Right_shift_expressionContext context)
    {
        Expression constExpr = ExpressionEvaluator.Instance.Visit(context);

        if (constExpr != null)
        {
            EmitConst(constExpr.Value);
            return constExpr.Type;
        }

        Type t = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);

        if (t2.IsGenericType && t2.GetGenericTypeDefinition() == typeof(List<>))
        {
            Type listType = t2.GetGenericArguments()[0];

            // LocalIndex - 1: List
            CurrentMethod.LocalIndex++;
            CurrentMethod.IL.DeclareLocal(t2);
            EmitStloc(CurrentMethod.LocalIndex);

            // LocalIndex: Item
            CurrentMethod.LocalIndex++;
            CurrentMethod.IL.DeclareLocal(t);
            EmitStloc(CurrentMethod.LocalIndex);

            EmitLdloc(CurrentMethod.LocalIndex - 1);
            EmitLdloc(CurrentMethod.LocalIndex);

            if (t != listType)
            {
                if (CanBeConverted(t, listType))
                    EmitConversionOperator(t, listType);
                else
                {
                    EmitErrorMessage(
                        context.expression()[1].Start.Line,
                        context.expression()[1].Start.Column,
                        context.expression()[1].GetText().Length,
                        DS0154_ListAppendIncompatibleElement,
                        $"An expression of type '{TypeName(t)}' cannot be prepended to a list of type '{TypeName(t2)}'.");
                }
            }

            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("Prepend").MakeGenericMethod(t2.GetGenericArguments()[0]));
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(t2.GetGenericArguments()[0]));
            return t2;
        }

        if (IsIntegerType(t))
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
                $"The type '{TypeName(t)}' does not implement a right shift operation with operand type '{TypeName(t2)}'.",
                Path.GetFileName(CurrentFile.Path));

            return t;
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, op, null);
        return op.ReturnType;
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

        Type t = SymbolResolver.ResolveTypeName(context.type_name());
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
        CurrentMethod.LoadReference = true;
        CurrentMethod.LoadIndirectIfByRef = false;

        Type t = Visit(context.expression());

        CurrentMethod.LoadReference = false;
        CurrentMethod.LoadIndirectIfByRef = true;

        if (t.IsByRef)
            return t;

        return t.MakeByRefType();
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

    private static void MakeOrGetClosureContainerType(List<Type> fieldTypes, out TypeBuilder tb, out List<FieldBuilder> fields)
    {
        if (CurrentMethod.ClosureContainerType != null)
        {
            tb = CurrentMethod.ClosureContainerType;
            fields = CurrentMethod.ClosureCapturedFields;
            return;
        }

        MakeClosureContainerType(fieldTypes, out tb, out fields);
    }

    private static void MakeClosureContainerType(List<Type> fieldTypes, out TypeBuilder tb, out List<FieldBuilder> fields)
    {
        tb = TypeContext.Current.Builder.DefineNestedType(
            SymbolNameGenerator.GetClosureTypeName(CurrentMethod.UniqueMethodName));

        tb.SetCustomAttribute(new(typeof(CompilerGeneratedAttribute).GetConstructor(Type.EmptyTypes), []));

        List<FieldBuilder> flds = [];

        for (int i = 0; i < fieldTypes.Count; i++)
            flds.Add(tb.DefineField(SymbolNameGenerator.GetClosureFieldName(i), fieldTypes[i], FieldAttributes.Public));

        fields = flds;

        CurrentMethod.ClosureContainerType = tb;
        CurrentMethod.ClosureCapturedFields = fields;
    }

    int _anonFunctionIndex = 0;

    public MethodInfo HandleAnonymousFunction([NotNull] DassieParser.Anonymous_function_expressionContext context, out TypeBuilder closureType)
    {
        Type ret = null;
        string name = CurrentMethod.GetAnonymousFunctionName(++CurrentMethod.AnonymousFunctionIndex);

        List<(Type Type, string Name, bool IsMutable)> parameters = [];

        foreach (var param in context.parameter_list().parameter())
        {
            Type paramType = typeof(object);

            if (param.type_name() != null)
                paramType = SymbolResolver.ResolveTypeName(param.type_name());

            parameters.Add((paramType, param.Identifier().GetIdentifier(), param.Var() != null));
        }

        var locals = CurrentMethod.Locals;
        var _params = CurrentMethod.Parameters;

        Disabled = true;
        CreateFakeMethod();
        CurrentMethod.CaptureSymbols = true;

        // Lambda parameters
        for (int i = 0; i < parameters.Count; i++)
        {
            (Type Type, string Name, bool IsMutable) param = parameters[i];

            LocalBuilder lb = CurrentMethod.IL.DeclareLocal(param.Type);
            CurrentMethod.Locals.Add(new()
            {
                Builder = lb,
                Index = i,
                IsConstant = !param.IsMutable,
                Name = param.Name,
                Scope = 0
            });
        }

        // Parameters of enclosing function
        for (int i = 0; i < _params.Count; i++)
        {
            ParamInfo param = _params[i];
            LocalBuilder lb = CurrentMethod.IL.DeclareLocal(param.Type);
            CurrentMethod.Locals.Add(new()
            {
                Builder = lb,
                Index = i,
                IsConstant = !param.IsMutable,
                Name = param.Name,
                Scope = 0
            });
        }

        CurrentMethod.Locals.AddRange(locals);

        Visit(context.expression());

        List<Type> fieldTypes = [];
        SymbolInfo[] capturedSymbols = CurrentMethod.CapturedSymbols.DistinctBy(s => s.Name()).Where(s => !parameters.Select(p => p.Name).Contains(s.Name())).ToArray();

        foreach (var sym in capturedSymbols)
            fieldTypes.Add(sym.Type());

        ResetFakeMethod();
        Disabled = false;

        MakeOrGetClosureContainerType(fieldTypes, out closureType, out List<FieldBuilder> fields);
        Dictionary<SymbolInfo, (FieldInfo Type, string LocalName)> alternativeLocations = [];

        for (int i = 0; i < fields.Count; i++)
            alternativeLocations.Add(capturedSymbols[i], (fields[i], SymbolNameGenerator.GetClosureLocalName(closureType.FullName)));

        foreach (var loc in alternativeLocations)
        {
            if (!CurrentMethod.AdditionalStorageLocations.ContainsKey(loc.Key))
                CurrentMethod.AdditionalStorageLocations.Add(loc.Key, loc.Value);
        }

        if (context.type_name() != null)
            ret = SymbolResolver.ResolveTypeName(context.type_name());

        MethodContext parentMethod = CurrentMethod;

        MethodBuilder invokeFunction = closureType.DefineMethod(SymbolNameGenerator.GetAnonymousFunctionName(_anonFunctionIndex++), MethodAttributes.Public, CallingConventions.HasThis);
        CurrentMethod = new()
        {
            Builder = invokeFunction,
            IL = invokeFunction.GetILGenerator(),
            IsClosureInvocationFunction = true
        };

        invokeFunction.SetParameters(parameters.Select(p => p.Type).ToArray());
        for (int i = 0; i < parameters.Count; i++)
        {
            ParameterBuilder pb = invokeFunction.DefineParameter(i + 1, ParameterAttributes.None, parameters[i].Name);
            CurrentMethod.Parameters.Add(new()
            {
                Builder = pb,
                Index = i + 1,
                IsMutable = parameters[i].IsMutable,
                Name = parameters[i].Name,
                Type = parameters[i].Type
            });
        }

        foreach (var sym in capturedSymbols)
        {
            LocalBuilder lb = CurrentMethod.IL.DeclareLocal(sym.Type());
            CurrentMethod.Locals.Add(new()
            {
                Builder = lb,
                Index = sym.Index(),
                IsConstant = !sym.IsMutable(),
                Name = sym.Name(),
                Scope = 0
            });
        }

        foreach (var loc in alternativeLocations)
            CurrentMethod.AdditionalStorageLocations.Add(loc.Key, loc.Value);

        // Locals not available
        Type tRet = Visit(context.expression());
        ret ??= tRet;
        CurrentMethod.IL.Emit(OpCodes.Ret);
        invokeFunction.SetReturnType(ret);

        SpecialStep1CurrentMethod = CurrentMethod;
        CurrentMethod = parentMethod;

        return invokeFunction;
    }

    public override Type VisitFull_identifier_member_access_expression([NotNull] DassieParser.Full_identifier_member_access_expressionContext context)
    {
        return VisitFull_identifier_member_access_expression(context, false).Type;
    }

    public MethodInfo GetFunctionPointerTarget(DassieParser.Full_identifier_member_access_expressionContext context, out TypeBuilder containerType, out FieldInfo instance, out MethodInfo target)
    {
        List<Type> argumentTypes = [];
        List<int> wildcardIndices = [];
        containerType = null;
        List<FieldBuilder> fields = null;
        instance = null;

        if (context.arglist() != null)
        {
            string funcName = context.full_identifier().GetText();

            List<Type> argTypes = [];

            RedirectEmitterToNullStream();

            foreach (DassieParser.ExpressionContext argument in context.arglist().expression())
            {
                if (!TreeHelpers.IsType<DassieParser.Wildcard_atomContext>(argument))
                    argTypes.Add(Visit(argument));
            }

            ResetNullStream();

            MakeOrGetClosureContainerType(argTypes, out containerType, out fields);

            DassieParser.ExpressionContext[] args = context.arglist().expression();

            CurrentMethod.CaptureSymbols = true;

            for (int i = 0; i < args.Length; i++)
            {
                DassieParser.ExpressionContext argument = args[i];
                if (TreeHelpers.IsType<DassieParser.Wildcard_atomContext>(argument))
                {
                    argumentTypes.Add(typeof(Wildcard));
                    wildcardIndices.Add(i);
                }
                else
                {
                    FieldInfo fld = fields.First(f => f.Name == SymbolNameGenerator.GetClosureFieldName(i));
                    Type t = Visit(argument);
                    CurrentMethod.IL.Emit(OpCodes.Stsfld, fld);
                    argumentTypes.Add(t);

                    if (CurrentMethod.CapturedSymbols.Count > 0)
                        CurrentMethod.AdditionalStorageLocations.Add(CurrentMethod.CapturedSymbols.Last(), (fld, null));
                }
            }

            CurrentMethod.CaptureSymbols = false;
            CurrentMethod.CapturedSymbols.Clear();
        }

        Type[] _args = null;
        if (context.arglist() != null)
            _args = argumentTypes.ToArray();

        CurrentMethod.CaptureSymbols = true;
        // TODO: Store argument types for each method call individually
        List<Type> paramTypes = new(CurrentMethod.ArgumentTypesForNextMethodCall);
        MethodInfo method = VisitFull_identifier_member_access_expression(context, true, _args).Result;
        CurrentMethod.ArgumentTypesForNextMethodCall = paramTypes;
        CurrentMethod.CaptureSymbols = false;

        target = method;

        if (context.arglist() == null)
        {
            CurrentMethod.CapturedSymbols.Clear();
            return method;
        }

        if (!method.IsStatic)
        {
            instance = containerType.DefineField(SymbolNameGenerator.GetBaseInstanceName(method.Name), method.DeclaringType, FieldAttributes.Public | FieldAttributes.Static);

            var symbols = CurrentMethod.CapturedSymbols.Where(s => s.Type() == method.DeclaringType);
            if (symbols.Any())
                CurrentMethod.AdditionalStorageLocations.Add(symbols.Last(), (instance, null));
        }

        //CurrentMethod.CapturedSymbols.Clear();

        int wildcardIndex = 0;
        ParameterInfo[] originalParams = method.GetParameters();
        Type[] wildcardParams = originalParams.Where(p => wildcardIndices.Contains(p.Position)).Select(p => p.ParameterType).ToArray();
        MethodBuilder result = containerType.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, method.ReturnType, wildcardParams);

        ILGenerator il = result.GetILGenerator();

        if (!method.IsStatic)
            il.Emit(OpCodes.Ldsfld, instance);

        for (int i = 0; i < argumentTypes.Count; i++)
        {
            if (argumentTypes[i] == typeof(Wildcard))
                il.Emit(OpCodes.Ldarg_S, (byte)wildcardIndex++);
            else
                il.Emit(OpCodes.Ldsfld, fields.First(f => f.Name == SymbolNameGenerator.GetClosureFieldName(i)));
        }

        il.Emit(OpCodes.Call, method);

        il.Emit(OpCodes.Ret);

        return result;
    }

    public (Type Type, MethodInfo Result) VisitFull_identifier_member_access_expression([NotNull] DassieParser.Full_identifier_member_access_expressionContext context, bool getFunctionPointerTarget, Type[] functionPointerParams = null, Type functionPointerRet = null)
    {
        memberIndex++;

        Generics.GenericArgumentContext[] genericArgList = Generics.ResolveGenericArgList(context.generic_arg_list());
        Type[] typeArgs = genericArgList
            .Select(t => t.Type)
            .ToArray();

        CurrentMethod.GenericArgumentsForNextMethodCall = genericArgList;

        if (context.full_identifier().Identifier().Length > 1)
            CurrentMethod.ShouldLoadAddressIfValueType = true;

        if (IntrinsicFunctionHandler.HandleSpecialFunction(
            context.full_identifier().Identifier().Last().GetText(),
            context.arglist(),
            context.full_identifier().Identifier().Last().Symbol.Line,
            context.full_identifier().Identifier().Last().Symbol.Column,
            context.full_identifier().Identifier().Last().GetText().Length,
            out Type ret,
            out MethodInfo method))
        {
            CurrentMethod.ShouldLoadAddressIfValueType = false;
            return (ret, method);
        }

        object o = SymbolResolver.GetSmallestTypeFromLeft(
            context.full_identifier(),
            genericArgList,
            context.full_identifier().Start.Line,
            context.full_identifier().Start.Column,
            context.full_identifier().GetText().Length,
            out int firstIndex);

        MethodInfo result = null;
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

                CurrentMethod.ShouldLoadAddressIfValueType = false;
                return (null, null);
            }

            if (o is ParamInfo p)
            {
                SymbolInfo s = new()
                {
                    Parameter = p,
                    SymbolType = SymbolInfo.SymType.Parameter
                };

                if (s.Type().IsFunctionPointer && context.arglist() != null && firstIndex == 0)
                    Visit(context.arglist());

                if (CurrentMethod.ShouldLoadAddressIfValueType)
                    s.LoadAddressIfValueType();
                else
                    s.Load();

                t = s.Type();

                if (t.IsByRef)
                    t = t.GetElementType();

                if (s.Type().IsFunctionPointer && (context.arglist() != null || (s.Type().GetFunctionPointerParameterTypes().Length == 0 && context.arglist() == null)) && firstIndex == 0)
                {
                    CurrentMethod.IL.EmitCalli(OpCodes.Calli, CallingConvention.Winapi, s.Type().GetFunctionPointerReturnType(), s.Type().GetFunctionPointerParameterTypes());
                    CurrentMethod.ShouldLoadAddressIfValueType = false;
                    return (s.Type().GetFunctionPointerReturnType(), null);
                }
            }

            else if (o is LocalInfo l)
            {
                SymbolInfo s = new()
                {
                    Local = l,
                    SymbolType = SymbolInfo.SymType.Local
                };

                if (s.Type().IsFunctionPointer && context.arglist() != null && firstIndex == 0)
                    Visit(context.arglist());

                FieldInfo closureInstanceField = null;
                string closureContainerLocalName = "";

                if (VisitorStep1CurrentMethod != null && VisitorStep1CurrentMethod.AdditionalStorageLocations.Any(sm => sm.Key.Name() == s.Name()))
                    (closureInstanceField, closureContainerLocalName) = VisitorStep1CurrentMethod.AdditionalStorageLocations.First(sm => sm.Key.Name() == s.Name()).Value;

                if (CurrentMethod.AdditionalStorageLocations.Any(sm => sm.Key.Name() == s.Name()))
                    (closureInstanceField, closureContainerLocalName) = CurrentMethod.AdditionalStorageLocations.First(sm => sm.Key.Name() == s.Name()).Value;

                if (CurrentMethod.Locals.Any(l => l.Name == closureContainerLocalName))
                    EmitLdloc(CurrentMethod.Locals.First(l => l.Name == closureContainerLocalName).Index);
                else if (closureInstanceField != null)
                    EmitLdarg(0);

                if ((CurrentMethod.ShouldLoadAddressIfValueType && !notLoadAddress) && (l.Builder.LocalType.IsValueType && context.full_identifier().Identifier().Length > 1))
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

                if (s.Type().IsFunctionPointer && (context.arglist() != null || (s.Type().GetFunctionPointerParameterTypes().Length == 0 && context.arglist() == null)) && firstIndex == 0)
                {
                    CurrentMethod.IL.EmitCalli(OpCodes.Calli, CallingConvention.Winapi, s.Type().GetFunctionPointerReturnType(), s.Type().GetFunctionPointerParameterTypes());
                    CurrentMethod.ShouldLoadAddressIfValueType = false;
                    return (s.Type().GetFunctionPointerReturnType(), null);
                }
            }

            else if (o is FieldInfo f)
            {
                if (TryGetConstantValue(f, out object v))
                {
                    EmitConst(v);

                    CurrentMethod.ShouldLoadAddressIfValueType = false;
                    return (f.FieldType, null);
                }

                if (f.FieldType.IsFunctionPointer && context.arglist() != null && firstIndex == 0)
                    Visit(context.arglist());

                if (f.IsStatic)
                    CurrentMethod.IL.Emit(OpCodes.Ldsfld, f);

                else if (TypeContext.Current.Fields.Any(_f => _f.Builder == f))
                {
                    CurrentMethod.IL.Emit(OpCodes.Ldarg_0);
                    CurrentMethod.IL.Emit(OpCodes.Ldfld, f);
                }

                //return f.FieldType;
                t = f.FieldType;

                if (t.IsGenericTypeParameter)
                    t = f.DeclaringType.GetGenericArguments()[t.GenericParameterPosition];

                if (f.FieldType.IsFunctionPointer && (context.arglist() != null || (f.FieldType.GetFunctionPointerParameterTypes().Length == 0 && context.arglist() == null)) && firstIndex == 0)
                {
                    CurrentMethod.IL.EmitCalli(OpCodes.Calli, CallingConvention.Winapi, f.FieldType.GetFunctionPointerReturnType(), f.FieldType.GetFunctionPointerParameterTypes());
                    CurrentMethod.ShouldLoadAddressIfValueType = false;
                    return (f.FieldType.GetFunctionPointerReturnType(), null);
                }
            }

            else if (o is MetaFieldInfo mfi)
            {
                if (mfi.ConstantValue != null)
                {
                    EmitConst(mfi.ConstantValue);
                    CurrentMethod.ShouldLoadAddressIfValueType = false;
                    return (mfi.ConstantValue.GetType(), null);
                }

                if (mfi.Builder.FieldType.IsFunctionPointer && context.arglist() != null && firstIndex == 0)
                    Visit(context.arglist());

                FieldInfo fld = mfi.Builder;

                if (fld.IsStatic)
                    CurrentMethod.IL.Emit(OpCodes.Ldsfld, fld);

                else if (TypeContext.Current.Fields.Any(_f => _f.Builder == fld))
                {
                    CurrentMethod.IL.Emit(OpCodes.Ldarg_0);
                    CurrentMethod.IL.Emit(OpCodes.Ldfld, fld);
                }

                t = fld.FieldType;

                if (t.IsGenericTypeParameter)
                    t = fld.FieldType.DeclaringType.GetGenericArguments()[t.GenericParameterPosition];

                if (mfi.Builder.FieldType.IsFunctionPointer && (context.arglist() != null || (mfi.Builder.FieldType.GetFunctionPointerParameterTypes().Length == 0 && context.arglist() == null)) && firstIndex == 0)
                {
                    CurrentMethod.IL.EmitCalli(OpCodes.Calli, CallingConvention.Winapi, mfi.Builder.FieldType.GetFunctionPointerReturnType(), mfi.Builder.FieldType.GetFunctionPointerParameterTypes());
                    CurrentMethod.ShouldLoadAddressIfValueType = false;
                    return (mfi.Builder.FieldType.GetFunctionPointerReturnType(), null);
                }
            }

            else if (o is PropertyInfo prop)
            {
                MethodInfo getter = prop.GetGetMethod();

                if (getter.DeclaringType.IsGenericTypeDefinition && getter.DeclaringType.FullName == TypeContext.Current.FullName)
                    getter = TypeBuilder.GetMethod(getter.DeclaringType.MakeGenericType(TypeContext.Current.GenericParameters.Select(t => t.Builder).ToArray()), getter);

                if (!prop.GetGetMethod().IsStatic)
                    EmitLdarg(0);

                EmitCall(prop.DeclaringType, getter);
                t = prop.PropertyType;

                if (firstIndex < (context.full_identifier().Identifier().Length - 1) && t.IsValueType)
                {
                    CurrentMethod.IL.DeclareLocal(t);
                    CurrentMethod.LocalIndex++;
                    EmitStloc(CurrentMethod.LocalIndex);
                    EmitLdloca(CurrentMethod.LocalIndex);
                }
            }

            else if (o is SymbolResolver.EnumValueInfo e)
            {
                EmitLdcI4((int)e.Value);
                t = e.EnumType;
            }

            else if (o is MethodBuilder m)
            {
                MethodInfo meth = m;
                List<Type> typeParams = [];

                if (m.IsGenericMethod)
                {
                    foreach (Type typeParam in m.GetGenericArguments())
                        typeParams.Add(typeParam);

                    meth = m.MakeGenericMethod(typeArgs);
                }

                for (int i = 0; i < meth.GetParameters().Length; i++)
                {
                    ParameterInfo param = meth.GetParameters()[i];

                    if (param.ParameterType.IsByRef /*|| param.ParameterType.IsByRefLike*/)
                        CurrentMethod.ByRefArguments.Add(i);
                }

                bool allowTailCall = CurrentMethod.AllowTailCallEmission;
                CurrentMethod.AllowTailCallEmission = false;

                if (context.arglist() != null && !getFunctionPointerTarget)
                    Visit(context.arglist());

                Type[] argTypes = CurrentMethod.ArgumentTypesForNextMethodCall.ToArray();

                if (getFunctionPointerTarget)
                {
                    if (functionPointerParams != null)
                        argTypes = functionPointerParams;
                    else
                        argTypes = null;
                }

                ErrorMessageHelpers.EmitDS0002ErrorIfInvalid(
                    context.full_identifier().Identifier()[0].Symbol.Line,
                    context.full_identifier().Identifier()[0].Symbol.Column,
                    context.full_identifier().Identifier()[0].GetText().Length,
                    meth.Name,
                    meth.DeclaringType,
                    meth,
                    argTypes);

                CurrentMethod.AllowTailCallEmission = allowTailCall;

                if (!getFunctionPointerTarget)
                {
                    Type[] optionalParamTypes = null;

                    if (meth.CallingConvention.HasFlag(CallingConventions.VarArgs))
                        optionalParamTypes = argTypes[meth.GetParameters().Length..];

                    EmitTailcall();
                    EmitCall(meth.DeclaringType, meth, optionalParamTypes);
                }

                if (meth.ReturnType.IsValueType && CurrentMethod.ShouldLoadAddressIfValueType && !notLoadAddress && !getFunctionPointerTarget)
                {
                    CurrentMethod.IL.DeclareLocal(meth.ReturnType);
                    CurrentMethod.LocalIndex++;
                    EmitStloc(CurrentMethod.LocalIndex);
                    EmitLdloca(CurrentMethod.LocalIndex);
                }

                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();

                result = meth;
                t = meth.ReturnType;

                if (t.IsGenericTypeParameter)
                    t = m.DeclaringType.GetGenericArguments()[t.GenericParameterPosition];

                if (typeParams.Count != typeArgs.Length)
                {
                    ThrowErrorForInvalidTypeArgumentCount(
                        context.full_identifier().Identifier()[0].Symbol.Line,
                        context.full_identifier().Identifier()[0].Symbol.Column,
                        context.full_identifier().Identifier()[0].GetText().Length,
                        false,
                        context.full_identifier().Identifier()[0].GetText(),
                        typeParams.Count,
                        typeArgs.Length);
                }
                else
                {
                    if (typeParams.Contains(t))
                        t = typeArgs[typeParams.IndexOf(t)];
                }
            }

            // Global method
            else if (o is List<MethodInfo> or List<MethodBuilder>)
            {
                List<MethodInfo> methods;
                if (o is List<MethodInfo> methodInfos)
                    methods = methodInfos;
                else
                    methods = ((List<MethodBuilder>)o).Cast<MethodInfo>().ToList();

                if (getFunctionPointerTarget && functionPointerParams == null && functionPointerRet == null && methods.Count > 0)
                {
                    MethodInfo fptr = methods.First();

                    CurrentMethod.ShouldLoadAddressIfValueType = false;
                    return (fptr.ReturnType, fptr);
                }

                bool allowTailCall = CurrentMethod.AllowTailCallEmission;
                CurrentMethod.AllowTailCallEmission = false;

                if (context.arglist() != null && !getFunctionPointerTarget)
                    Visit(context.arglist());

                CurrentMethod.AllowTailCallEmission = allowTailCall;

                Type[] argumentTypes = (CurrentMethod.ArgumentTypesForNextMethodCall ?? Type.EmptyTypes.ToList()).ToArray();
                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                MethodInfo final = null;

                if (getFunctionPointerTarget && functionPointerParams != null)
                    argumentTypes = functionPointerParams;

                if (methods.Count > 0)
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

                        if (possibleMethod.GetParameters().Length != argumentTypes.Length)
                            continue;

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

                if (!getFunctionPointerTarget)
                    EmitCall(final.DeclaringType, final);

                CurrentMethod.ShouldLoadAddressIfValueType = false;
                return (final.ReturnType, final);
            }
        }

        if (context.full_identifier().Identifier().Length == 1 && exitEarly)
        {
            CurrentMethod.ShouldLoadAddressIfValueType = false;
            return (t, result);
        }

        BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

        string typeName = context.full_identifier().Identifier().Last().GetText();
        string backtickedTypeName = typeName;
        if (typeArgs != null && typeArgs.Length > 0)
            backtickedTypeName = $"{typeName}`{typeArgs.Length}";

        bool handleCtor = false;

        ITerminalNode[] nextNodes = [];
        if (backtickedTypeName == t.Name || typeName == t.Name)
        {
            nextNodes = [(context.full_identifier().Identifier().Last())];
            handleCtor = true;
        }
        else
            nextNodes = context.full_identifier().Identifier()[firstIndex..];

        foreach (ITerminalNode identifier in nextNodes)
        {
            string memberName = identifier.GetText();
            if (handleCtor)
                memberName = t.Name;

            Type[] _params = null;

            if (identifier == context.full_identifier().Identifier().Last() && context.arglist() != null)
            {
                notLoadAddress = true;

                RedirectEmitterToNullStream();
                Visit(context.arglist());
                ResetNullStream();

                _params = CurrentMethod.ArgumentTypesForNextMethodCall.ToArray();
                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
            }

            if (functionPointerParams != null)
                _params = functionPointerParams;

            bool ignoreParams = getFunctionPointerTarget && functionPointerParams == null;

            object member = SymbolResolver.ResolveMember(
                t,
                memberName,
                identifier.Symbol.Line,
                identifier.Symbol.Column,
                identifier.GetText().Length,
                false,
                _params,
                flags,
                getDefaultOverload: ignoreParams);

            bool arglistVisited = false;

            if (identifier == context.full_identifier().Identifier().Last() && context.arglist() != null && !getFunctionPointerTarget)
            {
                Visit(context.arglist());
                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                arglistVisited = true;
            }

            if (member == null)
            {
                CurrentMethod.ShouldLoadAddressIfValueType = false;
                return (null, null);
            }

            if (member is Type nestedType)
            {
                t = nestedType;
                continue;
            }

            if (member is FieldInfo f)
            {
                if (!arglistVisited && f.FieldType.IsFunctionPointer && context.arglist() != null && identifier == nextNodes.Last())
                    Visit(context.arglist());

                if (TryGetConstantValue(f, out object v))
                    EmitConst(v);
                else
                    LoadField(f);

                t = f.FieldType;

                if (t.IsGenericTypeParameter)
                    t = f.DeclaringType.GetGenericArguments()[t.GenericParameterPosition];

                if (f.FieldType.IsFunctionPointer && (context.arglist() != null || (f.FieldType.GetFunctionPointerParameterTypes().Length == 0 && context.arglist() == null)))
                {
                    CurrentMethod.IL.EmitCalli(OpCodes.Calli, CallingConvention.Winapi, f.FieldType.GetFunctionPointerReturnType(), f.FieldType.GetFunctionPointerParameterTypes());
                    CurrentMethod.ShouldLoadAddressIfValueType = false;
                    return (f.FieldType.GetFunctionPointerReturnType(), null);
                }
            }

            else if (member is MetaFieldInfo mfi)
            {
                if (!arglistVisited && mfi.Builder.FieldType.IsFunctionPointer && context.arglist() != null && identifier == nextNodes.Last())
                    Visit(context.arglist());

                if (mfi.ConstantValue != null)
                    EmitConst(mfi.ConstantValue);
                else
                    LoadField(mfi.Builder);

                t = mfi.Builder.FieldType;

                if (t.IsGenericTypeParameter)
                    t = mfi.Builder.DeclaringType.GetGenericArguments()[t.GenericParameterPosition];

                if (mfi.Builder.FieldType.IsFunctionPointer && (context.arglist() != null || (mfi.Builder.FieldType.GetFunctionPointerParameterTypes().Length == 0 && context.arglist() == null)))
                {
                    CurrentMethod.IL.EmitCalli(OpCodes.Calli, CallingConvention.Winapi, mfi.Builder.FieldType.GetFunctionPointerReturnType(), mfi.Builder.FieldType.GetFunctionPointerParameterTypes());
                    CurrentMethod.ShouldLoadAddressIfValueType = false;
                    return (mfi.Builder.FieldType.GetFunctionPointerReturnType(), null);
                }
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

                try
                {
                    CurrentMethod.IL.Emit(OpCodes.Newobj, c);
                }
                catch { }

                if (genericArgList.Any(g => g.Value != null))
                {
                    Generics.GenericArgumentContext[] vals = genericArgList.Where(g => g.Value != null).ToArray();
                    MetaFieldInfo[] fields = SymbolResolver.GetFields(c.DeclaringType, BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(f => f.Attributes.Any(f => f is DependentValueAttribute))
                        .ToArray();

                    for (int i = 0; i < fields.Length; i++)
                    {
                        CurrentMethod.IL.Emit(OpCodes.Dup);
                        EmitConst(vals[i].Value);
                        CurrentMethod.IL.Emit(OpCodes.Stfld, fields[i].Builder);
                    }
                }

                t = c.DeclaringType;

                if (VisitorStep1CurrentMethod != null)
                    CurrentMethod.ParameterBoxIndices.Clear();
            }

            else if (member is SymbolResolver.DirectlyInitializedValueType d)
            {
                if (CurrentMethod.LoadAddressForDirectObjectInit)
                    EmitLdloca(CurrentMethod.DirectObjectInitIndex);

                CurrentMethod.IL.Emit(OpCodes.Initobj, d.Type);
                t = d.Type;

                CurrentMethod.LocalSetExternally = true;
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

                if (!getFunctionPointerTarget || identifier != nextNodes.Last())
                {
                    Type[] optionalParamTypes = null;
                    if (m.CallingConvention.HasFlag(CallingConventions.VarArgs))
                        optionalParamTypes = CurrentMethod.ArgumentTypesForNextMethodCall[m.GetParameters().Length..].ToArray();

                    //EmitTailcall();
                    EmitCall(t, m, optionalParamTypes);
                }

                if (identifier == nextNodes.Last())
                    result = m;

                t = m.ReturnType;

                if (t.IsGenericTypeParameter)
                    t = m.DeclaringType.GetGenericArguments()[t.GenericParameterPosition];

                if (VisitorStep1CurrentMethod != null)
                    CurrentMethod.ParameterBoxIndices.Clear();
            }
        }

        //CurrentMethod.ParameterBoxIndices.Clear();
        CurrentMethod.ShouldLoadAddressIfValueType = false;
        return (t, result);
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
                RedirectEmitterToNullStream();
                Visit(context.arglist());
                ResetNullStream();

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

            bool arglistVisited = false;

            if (identifier == context.Identifier().Last() && context.arglist() != null)
            {
                Visit(context.arglist());
                CurrentMethod.ArgumentTypesForNextMethodCall.Clear();
                arglistVisited = true;
            }

            if (member == null)
            {
                CurrentMethod.ShouldLoadAddressIfValueType = false;
                return null;
            }

            if (member is FieldInfo f)
            {
                if (!arglistVisited && f.FieldType.IsFunctionPointer && context.arglist() != null && identifier == context.Identifier().Last())
                    Visit(context.arglist());

                if (TryGetConstantValue(f, out object v))
                    EmitConst(v);
                else
                    LoadField(f);

                t = f.FieldType;

                if (t.IsGenericTypeParameter)
                    t = f.DeclaringType.GetGenericArguments()[t.GenericParameterPosition];

                if (f.FieldType.IsFunctionPointer && (context.arglist() != null || (f.FieldType.GetFunctionPointerParameterTypes().Length == 0 && context.arglist() == null)) && identifier == context.Identifier().Last())
                {
                    CurrentMethod.IL.EmitCalli(OpCodes.Calli, CallingConvention.Winapi, f.FieldType.GetFunctionPointerReturnType(), f.FieldType.GetFunctionPointerParameterTypes());
                    CurrentMethod.ShouldLoadAddressIfValueType = false;
                    return f.FieldType.GetFunctionPointerReturnType();
                }
            }

            else if (member is MetaFieldInfo mfi)
            {
                if (!arglistVisited && mfi.Builder.FieldType.IsFunctionPointer && context.arglist() != null && identifier == context.Identifier().Last())
                    Visit(context.arglist());

                if (mfi.ConstantValue != null)
                    EmitConst(mfi.ConstantValue);
                else
                    LoadField(mfi.Builder);

                t = mfi.Builder.FieldType;

                if (t.IsGenericTypeParameter)
                    t = mfi.Builder.DeclaringType.GetGenericArguments()[t.GenericParameterPosition];

                if (mfi.Builder.FieldType.IsFunctionPointer && (context.arglist() != null || (mfi.Builder.FieldType.GetFunctionPointerParameterTypes().Length == 0 && context.arglist() == null)) && identifier == context.Identifier().Last())
                {
                    CurrentMethod.IL.EmitCalli(OpCodes.Calli, CallingConvention.Winapi, mfi.Builder.FieldType.GetFunctionPointerReturnType(), mfi.Builder.FieldType.GetFunctionPointerParameterTypes());
                    CurrentMethod.ShouldLoadAddressIfValueType = false;
                    return mfi.Builder.FieldType.GetFunctionPointerReturnType();
                }
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

                Type[] optionalParamTypes = null;
                if (m.CallingConvention.HasFlag(CallingConventions.VarArgs))
                    optionalParamTypes = CurrentMethod.ArgumentTypesForNextMethodCall[m.GetParameters().Length..].ToArray();

                EmitTailcall();
                EmitCall(t, m, optionalParamTypes);
                t = m.ReturnType;

                if (t.IsGenericTypeParameter)
                    t = m.DeclaringType.GetGenericArguments()[t.GenericParameterPosition];

                if (VisitorStep1CurrentMethod != null)
                    CurrentMethod.ParameterBoxIndices.Clear();
            }
        }

        //CurrentMethod.ParameterBoxIndices.Clear();
        CurrentMethod.ShouldLoadAddressIfValueType = false;
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

                try
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, boxedType);
                    t = typeof(object);
                }
                catch (NullReferenceException) { }
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
        int prevScope = CurrentMethod.CurrentScope;
        CurrentMethod.CurrentScope = CurrentMethod.Scopes.Max() + 1;
        CurrentMethod.Scopes.Add(CurrentMethod.CurrentScope);

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
            {
#if ENABLE_DS0125
                ParserRuleContext rule = (ParserRuleContext)tree;
                string text = CurrentFile.CharStream.GetText(new(rule.Start.StartIndex, rule.Stop.StopIndex)).Trim();

                if (rule is not DassieParser.AssignmentContext and not DassieParser.Local_declaration_or_assignmentContext)
                {
                    EmitWarningMessage(
                        rule.Start.Line,
                        rule.Start.Column,
                        text.Length,
                        DS0125_UnusedValue,
                        $"Result of expression '{text}' is not used. Use 'ignore' to explicitly discard a value.");
                }
#endif // ENABLE_DS0125

                CurrentMethod.IL.Emit(OpCodes.Pop);
            }

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
        CurrentMethod.CurrentScope = prevScope;
        return ret;
    }

    public override Type VisitIdentifier_atom([NotNull] DassieParser.Identifier_atomContext context)
    {
        string text = context.Identifier() != null
            ? context.Identifier().GetIdentifier()
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

    private static void WarnIfConstantBoolean(DassieParser.ExpressionContext rule)
    {
        Expression expr = ExpressionEvaluator.Instance.Visit(rule);

        if (expr is not null && expr.Value is bool cond && !expr.IsBooleanLiteral)
        {
            EmitWarningMessage(
                rule.Start.Line,
                rule.Start.Column,
                rule.GetText().Length,
                DS0202_ConditionConstant,
                $"Condition is always {cond.ToString().ToLowerInvariant()}.");
        }
    }

    public override Type VisitPrefix_if_expression([NotNull] DassieParser.Prefix_if_expressionContext context)
    {
        Type t;
        List<Type> t2 = new();
        Type t3 = null;

        Label falseBranch = CurrentMethod.IL.DefineLabel();
        Label restBranch = CurrentMethod.IL.DefineLabel();

        // Comparative expression
        Type ct = Visit(context.if_branch().expression()[0]);
        WarnIfConstantBoolean(context.if_branch().expression()[0]);
        EnsureBoolean(ct,
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
                WarnIfConstantBoolean(tree.expression()[0]);
                EnsureBoolean(_ct,
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
        else
            EmitDefault(t);

        CurrentMethod.IL.MarkLabel(restBranch);

        bool allEqual = t2.Select(t => t.Name).Distinct().Count() == 1;

        if (t2.Count == 0)
        {
            allEqual = true;
            t2.Add(t);
        }

        if (allEqual && t3 == null)
            return t;

        // TODO: Support branches with different types -> return new style union

        //if (!allEqual || t != t3 || t != t2[0])
        //    return typeof(UnionValue);

        return t;
    }

    public override Type VisitPostfix_if_expression([NotNull] DassieParser.Postfix_if_expressionContext context)
    {
        Label fb = CurrentMethod.IL.DefineLabel();
        Label rest = CurrentMethod.IL.DefineLabel();

        // Comparative expression
        Type ct = Visit(context.postfix_if_branch().expression());
        WarnIfConstantBoolean(context.postfix_if_branch().expression());
        EnsureBoolean(ct,
            context.Start.Line,
            context.Start.Column,
            context.Start.Text.Length);

        CurrentMethod.IL.Emit(OpCodes.Brfalse, fb);

        Type t = Visit(context.expression());

        CurrentMethod.IL.MarkLabel(fb);
        EmitDefault(t);
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
        WarnIfConstantBoolean(context.unless_branch().expression()[0]);
        EnsureBoolean(ct,
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
                WarnIfConstantBoolean(tree.expression()[0]);
                EnsureBoolean(_ct,
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
        else
            EmitDefault(t);

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
        WarnIfConstantBoolean(context.postfix_unless_branch().expression());
        EnsureBoolean(ct,
            context.Start.Line,
            context.Start.Column,
            context.Start.Text.Length);

        CurrentMethod.IL.Emit(OpCodes.Brtrue, fb);

        Type t = Visit(context.expression());

        CurrentMethod.IL.MarkLabel(fb);
        EmitDefault(t);
        CurrentMethod.IL.Emit(OpCodes.Br, rest);

        CurrentMethod.IL.MarkLabel(rest);

        return t;
    }

    public override Type VisitReal_atom([NotNull] DassieParser.Real_atomContext context)
    {
        Expression expr = ExpressionEvaluator.Instance.VisitReal_atom(context);

        if (expr.Type == typeof(float))
            CurrentMethod.IL.Emit(OpCodes.Ldc_R4, expr.Value);
        else if (expr.Type == typeof(double))
            CurrentMethod.IL.Emit(OpCodes.Ldc_R8, expr.Value);
        else
            DecimalLiteralCodeGeneration.EmitDecimal(expr.Value);

        return expr.Type;
    }

    public override Type VisitInteger_atom([NotNull] DassieParser.Integer_atomContext context)
    {
        Expression expr = ExpressionEvaluator.Instance.VisitInteger_atom(context);

        if (expr.Type == typeof(ulong) || expr.Type == typeof(long))
            EmitLdcI8(expr.Value);
        else
        {
            EmitLdcI4((int)expr.Value);

            if (expr.Type != typeof(int) && expr.Type != typeof(uint))
                EmitConversionOperator(typeof(int), expr.Type);
        }

        return expr.Type;
    }

    public override Type VisitString_atom([NotNull] DassieParser.String_atomContext context)
    {
        string rawText = ExpressionEvaluator.Instance.VisitString_atom(context).Value;

        if (context.identifier_atom() == null)
        {
            CurrentMethod.IL.Emit(OpCodes.Ldstr, rawText);
            return typeof(string);
        }

        Type processorType = SymbolResolver.ResolveTypeName(
            context.identifier_atom().GetText(),
            context.identifier_atom().Start.Line,
            context.identifier_atom().Start.Column,
            context.identifier_atom().GetText().Length,
            noErrors: true);

        if (processorType == null)
        {
            EmitErrorMessage(
                context.identifier_atom().Start.Line,
                context.identifier_atom().Start.Column,
                context.identifier_atom().GetText().Length,
                DS0188_InvalidStringProcessor,
                $"The string processor '{context.identifier_atom().GetText()}' could not be resolved.");

            CurrentMethod.IL.Emit(OpCodes.Ldstr, rawText);
            return typeof(string);
        }

        MethodInfo processMethod = processorType.GetMethod("Process");

        if (processMethod.GetCustomAttribute<PureAttribute>() == null)
        {
            CurrentMethod.IL.Emit(OpCodes.Ldstr, rawText);
            CurrentMethod.IL.Emit(OpCodes.Call, processMethod);
            return processMethod.ReturnType;
        }

        object result;

        try
        {
            result = processMethod.Invoke(null, [rawText]);
        }
        catch (TargetInvocationException)
        {
            EmitErrorMessage(
                context.identifier_atom().Start.Line,
                context.identifier_atom().Start.Column,
                context.identifier_atom().GetText().Length,
                DS0190_StringProcessorThrewException,
                $"Expression value could not be determined because string processor '{context.identifier_atom().GetText()}' threw an exception at compile-time.");

            CurrentMethod.IL.Emit(OpCodes.Ldstr, rawText);
            return typeof(string);
        }

        if (EmitConst(result))
            return result.GetType();

        CurrentMethod.IL.Emit(OpCodes.Ldstr, rawText);
        CurrentMethod.IL.Emit(OpCodes.Call, processMethod);
        return processMethod.ReturnType;
    }

    public override Type VisitCharacter_atom([NotNull] DassieParser.Character_atomContext context)
    {
        char rawChar = ExpressionEvaluator.Instance.VisitCharacter_atom(context).Value;

        CurrentMethod.IL.Emit(OpCodes.Ldc_I4, rawChar);

        return typeof(char);
    }

    public override Type VisitBoolean_atom([NotNull] DassieParser.Boolean_atomContext context)
    {
        Expression val = ExpressionEvaluator.Instance.VisitBoolean_atom(context);

        CurrentMethod.IL.Emit(OpCodes.Ldc_I4_S, (byte)(val.Value ? 1 : 0));

        return typeof(bool);
    }

    public override Type VisitAssignment([NotNull] DassieParser.AssignmentContext context)
    {
        if (context.expression()[0].GetType() != typeof(DassieParser.Full_identifier_member_access_expressionContext)
            && context.expression()[0].GetType() != typeof(DassieParser.Member_access_expressionContext)
            && context.expression()[0].GetType() != typeof(DassieParser.Index_expressionContext))
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
        con ??= (context.expression()[0] as DassieParser.Index_expressionContext).expression()[0];

        bool indexAssignment = context.expression()[0] is DassieParser.Index_expressionContext;

        // TODO: Implement index assignment

        object o = null;
        int firstIndex = 0;
        bool exitEarly = true;

        if (context.expression()[0].GetType() == typeof(DassieParser.Full_identifier_member_access_expressionContext)
            || con.GetType() == typeof(DassieParser.Full_identifier_member_access_expressionContext))
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

                CurrentMethod.ShouldLoadAddressIfValueType = false;
                return null;
            }

            if (o is ParamInfo p)
            {
                SymbolInfo s = new()
                {
                    Parameter = p,
                    SymbolType = SymbolInfo.SymType.Parameter
                };

                //EmitConversionOperator(ret, s.Type());

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

                //EmitConversionOperator(ret, s.Type());

                if (CurrentMethod.ShouldLoadAddressIfValueType)
                    s.LoadAddressIfValueType();
                else
                    s.Load();

                t = s.Type();
            }

            else if (o is FieldInfo f)
            {
                MetaFieldInfo mfi = null;
                if (TypeContext.Current.Fields.Any(_f => _f.Builder == f))
                    mfi = TypeContext.Current.Fields.First(_f => _f.Builder == f);

                if (f.IsInitOnly)
                {
                    EmitErrorMessage(
                        context.Start.Line,
                        context.Start.Column,
                        context.GetText().Length,
                        DS0094_InitOnlyFieldAssignedOutsideOfConstructor,
                        $"The field '{f.Name}' is read-only and cannot be modified outside of a constructor.");
                }

                if (f.IsStatic)
                {
                    EmitLdloc(tempIndex);
                    EmitConversionOperator(ret, f.FieldType);
                    CurrentMethod.IL.Emit(OpCodes.Stsfld, f);
                }

                else if (mfi != null)
                {
                    CurrentMethod.IL.Emit(OpCodes.Ldarg_0);
                    EmitLdloc(tempIndex);
                    EmitConversionOperator(ret, f.FieldType);
                    CurrentMethod.IL.Emit(OpCodes.Stfld, f);
                }

                CurrentMethod.ShouldLoadAddressIfValueType = false;
                return ret;
            }

            else if (o is SymbolResolver.EnumValueInfo e)
            {
                EmitLdcI4((int)e.Value);

                CurrentMethod.ShouldLoadAddressIfValueType = false;
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
                CurrentMethod.ShouldLoadAddressIfValueType = false;
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
                CurrentMethod.ShouldLoadAddressIfValueType = false;
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
            {
                CurrentMethod.ShouldLoadAddressIfValueType = false;
                return null;
            }

            if (member is MetaFieldInfo mf)
            {
                if (identifier == ids.Last())
                {
                    if (mf.Builder.IsInitOnly)
                    {
                        EmitErrorMessage(
                            context.Start.Line,
                            context.Start.Column,
                            context.GetText().Length,
                            DS0094_InitOnlyFieldAssignedOutsideOfConstructor,
                            $"The field '{mf.Name}' is read-only and cannot be modified outside of a constructor.");
                    }

                    EmitLdloc(tempIndex);
                    EmitConversionOperator(ret, mf.Builder.FieldType);
                    EmitStfld(mf.Builder);
                    CurrentMethod.SkipPop = true;
                }
                else
                {
                    LoadField(mf.Builder);
                    t = mf.Builder.FieldType;
                }
            }

            if (member is FieldInfo f)
            {
                MetaFieldInfo mfi = null;
                if (TypeContext.Current.Fields.Any(_f => _f.Builder == f))
                    mfi = TypeContext.Current.Fields.First(_f => _f.Builder == f);

                if (identifier == ids.Last())
                {
                    if (f.IsInitOnly)
                    {
                        EmitErrorMessage(
                            context.Start.Line,
                            context.Start.Column,
                            context.GetText().Length,
                            DS0094_InitOnlyFieldAssignedOutsideOfConstructor,
                            $"The field '{f.Name}' is read-only and cannot be modified outside of a constructor.");
                    }

                    EmitLdloc(tempIndex);
                    EmitConversionOperator(ret, f.FieldType);
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
                        DassieParser.Full_identifierContext full_id = (DassieParser.Full_identifierContext)(con.full_identifier());

                        EmitErrorMessage(
                            full_id.Identifier().Last().Symbol.Line,
                            full_id.Identifier().Last().Symbol.Column,
                            full_id.Identifier().Last().GetText().Length,
                            DS0066_PropertyNoSuitableSetter,
                            $"The property '{p.Name}' is immutable and cannot be assigned to.");
                    }
                    else
                    {
                        EmitLdloc(tempIndex);
                        EmitConversionOperator(ret, p.PropertyType);
                        EmitCall(t, p.GetSetMethod());
                    }
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
        bool skipLocalSet;

        if (context.expression() is DassieParser.Try_expressionContext)
        {
            EmitErrorMessage(
                context.expression().Start.Line,
                context.expression().Start.Column,
                context.expression().GetText().Length,
                DS0064_InvalidExpression,
                "A try block cannot be used as an expression.");
        }

        FieldInfo closureInstanceField = null;
        string closureContainerLocalName = "";
        SymbolInfo sym = SymbolResolver.GetSymbol(context.Identifier().GetIdentifier());

        if (sym is not null)
        {
            if (sym.SymbolType == SymbolInfo.SymType.Local && sym.Local.Scope > CurrentMethod.CurrentScope)
            {
                EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    context.GetText().Length,
                    DS0109_LocalDefinedInDifferentScope,
                    $"The local '{sym.Name()}' cannot be defined here, because it is already defined in a different scope.");

                return sym.Type();
            }

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
            {
                //EmitLdarg(sym.Index());
                sym.Load(skipLdind: true);
            }

            if (VisitorStep1CurrentMethod != null && VisitorStep1CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == sym.Name()))
                (closureInstanceField, closureContainerLocalName) = VisitorStep1CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == sym.Name()).Value;

            if (CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == sym.Name()))
                (closureInstanceField, closureContainerLocalName) = CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == sym.Name()).Value;

            if (CurrentMethod.Locals.Any(l => l.Name == closureContainerLocalName))
                EmitLdloc(CurrentMethod.Locals.First(l => l.Name == closureContainerLocalName).Index);
            else if (closureInstanceField != null)
                EmitLdarg(0);

            CurrentMethod.LoadAddressForDirectObjectInit = true;
            CurrentMethod.DirectObjectInitIndex = sym.Index();

            Type type = Visit(context.expression());

            skipLocalSet = CurrentMethod.LocalSetExternally;
            CurrentMethod.LocalSetExternally = false;

            CurrentMethod.LoadAddressForDirectObjectInit = false;

            //if (CurrentMethod.NextAssignmentIsFunctionPointer && type == typeof(nint))
            //{
            //    sym.IsFunctionPointer = true;
            //    sym.FunctionPointerTarget = CurrentMethod.NextAssignmentFunctionPointerTarget;

            //    if (!CurrentMethod.NextAssignmentFunctionPointerTarget.IsStatic)
            //    {
            //        EmitErrorMessage(
            //            context.expression().Start.Line,
            //            context.expression().Start.Column,
            //            context.expression().GetText().Length,
            //            DS0152_FunctionPointerForInstanceMethod,
            //            $"Cannot create function pointer for instance method '{CurrentMethod.NextAssignmentFunctionPointerTarget.Name}'. Function pointers are only supported for static methods.");
            //    }
            //}

            //CurrentMethod.NextAssignmentIsFunctionPointer = false;

            bool checkTypes = true;
            if (type != sym.Type() && !((sym.Type().IsByRef /*|| sym.Type().IsByRefLike*/) && sym.Type().GetElementType() == type))
            {
                if (CanBeConverted(type, sym.Type()))
                {
                    EmitConversionOperator(type, sym.Type());
                    checkTypes = false;
                }
            }

            if (!skipLocalSet)
                sym.Set(setIndirectIfByRef: !type.IsByRef);

            if (checkTypes && type != sym.Type() && !((sym.Type().IsByRef /*|| sym.Type().IsByRefLike*/) && sym.Type().GetElementType() == type))
            {
                //if (sym.Type() == typeof(UnionValue))
                //{
                //    if (sym.Union().AllowedTypes.Contains(type))
                //    {
                //        sym.LoadAddress();

                //        EmitLdloc(CurrentMethod.LocalIndex);
                //        CurrentMethod.IL.Emit(OpCodes.Box, type);

                //        MethodInfo m = typeof(UnionValue).GetMethod("set_Value", new Type[] { typeof(object) });
                //        CurrentMethod.IL.Emit(OpCodes.Call, m);

                //        CurrentMethod.SkipPop = true;
                //        return sym.Union().GetType();
                //    }

                //    EmitErrorMessage(
                //        context.assignment_operator().Start.Line,
                //        context.assignment_operator().Start.Column,
                //        context.assignment_operator().GetText().Length,
                //        DS0019_GenericValueTypeInvalid,
                //        $"Values of type '{TypeName(type)}' are not supported by union type '{sym.Union().ToTypeString()}'.");

                //    return sym.Union().GetType();
                //}

                if (!EmitConversionOperator(type, sym.Type()))
                {
                    EmitErrorMessage(
                        context.assignment_operator().Start.Line,
                        context.assignment_operator().Start.Column,
                        context.assignment_operator().GetText().Length,
                        DS0006_VariableTypeChanged,
                        $"Expected expression of type '{TypeName(sym.Type())}', but got type '{TypeName(type)}'.");
                }

                return type;
            }

            if (sym.Field != null && !sym.Field.Builder.IsStatic)
                EmitLdarg(0);

            if (CurrentMethod.Locals.Any(l => l.Name == closureContainerLocalName))
                EmitLdloc(CurrentMethod.Locals.First(l => l.Name == closureContainerLocalName).Index);
            else if (closureInstanceField != null)
                EmitLdarg(0);

            sym.Load();

            CurrentFile.Fragments.Add(sym.GetFragment(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetIdentifier().Length,
                false));

            return sym.Type();
        }

        if (VisitorStep1CurrentMethod != null && VisitorStep1CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == context.Identifier().GetIdentifier()))
            (closureInstanceField, closureContainerLocalName) = VisitorStep1CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == context.Identifier().GetIdentifier()).Value;

        if (CurrentMethod.AdditionalStorageLocations.Any(s => s.Key.Name() == context.Identifier().GetIdentifier()))
            (closureInstanceField, closureContainerLocalName) = CurrentMethod.AdditionalStorageLocations.First(s => s.Key.Name() == context.Identifier().GetIdentifier()).Value;

        if (CurrentMethod.Locals.Any(l => l.Name == closureContainerLocalName))
            EmitLdloc(CurrentMethod.Locals.First(l => l.Name == closureContainerLocalName).Index);
        else if (closureInstanceField != null)
            EmitLdarg(0);

        CurrentMethod.LoadAddressForDirectObjectInit = true;
        CurrentMethod.DirectObjectInitIndex = CurrentMethod.LocalIndex + 1;

        Type t = Visit(context.expression());

        skipLocalSet = CurrentMethod.LocalSetExternally;
        CurrentMethod.LocalSetExternally = false;

        CurrentMethod.LoadAddressForDirectObjectInit = false;

        Type t1 = t;

        if (context.type_name() != null)
        {
            Type t2 = Visit(context.type_name());

            if (t2 != t)
            {
                if (CanBeConverted(t, t2))
                    EmitConversionOperator(t, t2);

                else if (t2 == typeof(object))
                {
                    CurrentMethod.IL.Emit(OpCodes.Box, t);
                }
                else if (!t.IsByRef && t.MakeByRefType() == t2) { }
                else if (!t.IsByRef && CanBeConverted(t.MakeByRefType(), t2))
                    EmitConversionOperator(t.MakeByRefType(), t2);
                else
                {
                    EmitErrorMessage(
                        context.expression().Start.Line,
                        context.expression().Start.Column,
                        context.expression().GetText().Length,
                        DS0057_IncompatibleType,
                        $"An expression of type '{TypeName(t)}' cannot be assigned to a variable of type '{TypeName(t2)}'.");
                }
            }

            t = t2;
        }

        LocalBuilder lb = CurrentMethod.IL.DeclareLocal(t);

        if (t.IsByRef && context.Var() == null)
        {
            EmitErrorMessage(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetIdentifier().Length,
                DS0148_ImmutableValueOfByRefType,
                $"The type '{TypeName(t)}' is invalid for '{context.Identifier().GetIdentifier()}' since it is an immutable value. Pointers and references are only supported by mutable variables.");
        }

        CurrentFile.Fragments.Add(new()
        {
            Line = context.Identifier().Symbol.Line,
            Column = context.Identifier().Symbol.Column,
            Length = context.Identifier().GetIdentifier().Length,
            Color = context.Var() == null ? Color.LocalValue : Color.LocalVariable,
            ToolTip = TooltipGenerator.Local(context.Identifier().GetIdentifier(), context.Var() != null, lb),
            IsNavigationTarget = true
        });

        SetLocalSymInfo(lb, context.Identifier().GetIdentifier());
        MarkSequencePoint(context.Identifier().Symbol.Line, context.Identifier().Symbol.Column, context.Identifier().GetIdentifier().Length);

        CurrentMethod.LocalIndex++;
        CurrentMethod.Locals.Add(new(context.Identifier().GetIdentifier(), lb, context.Var() == null, CurrentMethod.LocalIndex));

        SymbolInfo localSymbol = new()
        {
            Local = CurrentMethod.Locals.Last(),
            SymbolType = SymbolInfo.SymType.Local
        };

        string ds0074Message = $"'{CurrentMethod.Builder.Name}': Only {ushort.MaxValue} locals can be declared per function.";
        if (localSymbol.Index() > ushort.MaxValue && !messages.Any(m => m.ErrorMessage == ds0074Message))
        {
            EmitErrorMessage(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetText().Length,
                DS0074_TooManyLocals,
                ds0074Message);
        }

        //if (CurrentMethod.NextAssignmentIsFunctionPointer && localSymbol.Type() == typeof(nint))
        //{
        //    localSymbol.IsFunctionPointer = true;
        //    localSymbol.FunctionPointerTarget = CurrentMethod.NextAssignmentFunctionPointerTarget;

        //    if (!CurrentMethod.NextAssignmentFunctionPointerTarget.IsStatic)
        //    {
        //        EmitErrorMessage(
        //            context.expression().Start.Line,
        //            context.expression().Start.Column,
        //            context.expression().GetText().Length,
        //            DS0152_FunctionPointerForInstanceMethod,
        //            $"Cannot create function pointer for instance method '{CurrentMethod.NextAssignmentFunctionPointerTarget.Name}'. Function pointers are only supported for static methods.");
        //    }
        //}

        //if (t == typeof(UnionValue))
        //{
        //    CurrentMethod.IL.Emit(OpCodes.Box, t1);

        //    ConstructorInfo constructor = t.GetConstructor(new Type[] { typeof(object), typeof(Type[]) });

        //    UnionValue union = CurrentMethod.CurrentUnion;

        //    EmitLdcI4(union.AllowedTypes.Length);
        //    CurrentMethod.IL.Emit(OpCodes.Newarr, typeof(Type));
        //    CurrentMethod.IL.Emit(OpCodes.Dup);

        //    for (int i = 0; i < union.AllowedTypes.Length; i++)
        //    {
        //        EmitLdcI4(i);
        //        CurrentMethod.IL.Emit(OpCodes.Ldtoken, union.AllowedTypes[i]);

        //        MethodInfo getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });
        //        CurrentMethod.IL.Emit(OpCodes.Call, getTypeFromHandle);
        //        CurrentMethod.IL.Emit(OpCodes.Stelem_Ref);

        //        CurrentMethod.IL.Emit(OpCodes.Dup);
        //    }

        //    CurrentMethod.IL.Emit(OpCodes.Pop);

        //    CurrentMethod.IL.Emit(OpCodes.Newobj, constructor);
        //}

        if (!skipLocalSet)
            localSymbol.Set(setIndirectIfByRef: !t.IsByRef);

        localSymbol.Load(!string.IsNullOrEmpty(closureContainerLocalName), closureContainerLocalName);

        return t;
    }

    public override Type VisitType_name([NotNull] DassieParser.Type_nameContext context)
    {
        return ExpressionEvaluator.Instance.VisitType_name(context).Value;
    }

    public override Type VisitTuple_expression([NotNull] DassieParser.Tuple_expressionContext context)
    {
        static string TypeName(Type t)
        {
            try
            {
                return t.AssemblyQualifiedName;
            }
            catch (Exception)
            {
                return $"{t.FullName}, {Context.Assembly.FullName}";
            }
        }

        List<Type> types = [];

        for (int i = 0; i < context.expression().Length; i++)
            types.Add(Visit(context.expression()[i]));

        // If more than 8 tuple items are specified, split the tuples into multiple ones
        // This stupid algorithm took AGES to create...

        List<Type> _types = types.ToList();
        List<string> _intermediateTuples = [];

        string typeId = $"System.ValueTuple`{Math.Min(_types.Count, 8)}[";

        for (int k = 0; k < types.Count; k += 7)
        {
            if (_types.Count <= 7)
            {
                for (int i = 0; i < _types.Count - 1; i++)
                {
                    string _middlePart = $"[{TypeName(_types[i])}],";

                    if (_intermediateTuples.Any())
                        _intermediateTuples[^1] += _middlePart;

                    typeId += _middlePart;
                }

                string _endPart = $"[{TypeName(_types.Last())}]]";

                if (_intermediateTuples.Any())
                    _intermediateTuples[^1] += _endPart;

                typeId += _endPart;
                break;
            }

            Type[] proper = _types.ToArray()[(k * 8)..7];
            for (int j = 0; j < proper.Length; j++)
                typeId += $"[{TypeName(proper[j])}],";

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
            ConstructorInfo imConstructor;

            try
            {
                imConstructor = TypeBuilder.GetConstructor(t, t.GetGenericTypeDefinition().GetConstructors()[0]);
            }
            catch (ArgumentException)
            {
                imConstructor = t.GetConstructors()[0];
            }

            CurrentMethod.IL.Emit(OpCodes.Newobj, imConstructor);
        }

        Type _tupleType = Type.GetType(typeId);
        _tupleType ??= GetValueTupleType(_types.ToArray());

        ConstructorInfo _c;

        try
        {
            _c = TypeBuilder.GetConstructor(_tupleType, _tupleType.GetGenericTypeDefinition().GetConstructors()[0]);
        }
        catch (ArgumentException)
        {
            _c = _tupleType.GetConstructors()[0];
        }

        CurrentMethod.IL.Emit(OpCodes.Newobj, _c);
        return _tupleType;
    }

    public override Type VisitArray_expression([NotNull] DassieParser.Array_expressionContext context)
    {
        if (context.expression().Length == 1 && context.expression()[0] is DassieParser.Delimited_range_expressionContext range)
        {
            Expression start = ExpressionEvaluator.Instance.Visit(range.expression()[0]);
            Expression end = ExpressionEvaluator.Instance.Visit(range.expression()[1]);

            if (start == null || end == null || start.Type != typeof(int) || end.Type != typeof(int))
            {
                EmitErrorMessage(
                    range.Start.Line,
                    range.Start.Column,
                    range.GetText().Length,
                    DS0200_ListFromRangeNotCompileTimeConstant,
                    "Array constructed from range expression must use range of compile-time constant integral values.");

                return typeof(int[]);
            }

            EmitLdcI4(start.Value); // Start
            EmitLdcI4(end.Value - start.Value + 1); // Count
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("Range"));
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(typeof(int)));
            return typeof(int[]);
        }

        Expression cnst = ExpressionEvaluator.Instance.Visit(context);
        if (cnst != null)
        {
            EmitConst(cnst.Value);
            return cnst.Type;
        }

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

    public override Type VisitList_initializer_expression([NotNull] DassieParser.List_initializer_expressionContext context)
    {
        if (context.expression().Length == 0)
        {
            CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(List<object>).GetConstructor([]));
            return typeof(List<object>);
        }

        if (context.expression().Length == 1 && context.expression()[0] is DassieParser.Delimited_range_expressionContext range)
        {
            Expression start = ExpressionEvaluator.Instance.Visit(range.expression()[0]);
            Expression end = ExpressionEvaluator.Instance.Visit(range.expression()[1]);

            if (start == null || end == null || start.Type != typeof(int) || end.Type != typeof(int))
            {
                EmitErrorMessage(
                    range.Start.Line,
                    range.Start.Column,
                    range.GetText().Length,
                    DS0200_ListFromRangeNotCompileTimeConstant,
                    "List constructed from range expression must use range of compile-time constant integral values.");

                return typeof(List<int>);
            }

            EmitLdcI4(start.Value); // Start
            EmitLdcI4(end.Value - start.Value + 1); // Count
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("Range"));
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(typeof(int)));
            return typeof(List<int>);
        }

        Type itemType = Visit(context.expression()[0]);

        CurrentMethod.LocalIndex++;
        CurrentMethod.IL.DeclareLocal(itemType);
        EmitStloc(CurrentMethod.LocalIndex);

        Type listType = typeof(List<>).MakeGenericType(itemType);
        CurrentMethod.IL.Emit(OpCodes.Newobj, listType.GetConstructor([]));
        CurrentMethod.IL.Emit(OpCodes.Dup);

        EmitLdloc(CurrentMethod.LocalIndex);
        CurrentMethod.IL.Emit(OpCodes.Callvirt, listType.GetMethod("Add"));

        foreach (ParserRuleContext expr in context.expression()[1..])
        {
            CurrentMethod.IL.Emit(OpCodes.Dup);

            Type type = Visit(expr);

            if (type != itemType)
            {
                if (type.IsAssignableTo(itemType) || CanBeConverted(type, itemType))
                    EmitConversionOperator(type, itemType);

                else
                {
                    EmitErrorMessage(
                        expr.Start.Line,
                        expr.Start.Column,
                        expr.GetText().Length,
                        DS0153_ListLiteralDifferentTypes,
                        $"An item of type '{TypeName(type)}' cannot be added to a list of type '{listType}'.");
                }
            }

            CurrentMethod.IL.Emit(OpCodes.Callvirt, listType.GetMethod("Add"));
        }

        return listType;
    }

    public override Type VisitEmpty_atom([NotNull] DassieParser.Empty_atomContext context)
    {
        CurrentMethod.IL.Emit(OpCodes.Ldnull);
        return typeof(object);
    }

    public override Type VisitWildcard_atom([NotNull] DassieParser.Wildcard_atomContext context)
    {
        CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(Wildcard).GetConstructor([]));
        return typeof(Wildcard);
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

    public override Type VisitRange_index_expression([NotNull] DassieParser.Range_index_expressionContext context)
    {
        Visit(context.integer_atom());

        EmitLdcI4(context.Caret() == null ? 0 : 1);

        CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(Index).GetConstructor(new Type[] { typeof(int), typeof(bool) }));
        return typeof(Index);
    }

    // ..
    public override Type VisitFull_range_expression([NotNull] DassieParser.Full_range_expressionContext context)
    {
        CurrentMethod.IL.EmitCall(OpCodes.Call, typeof(Range).GetMethod("get_All", Type.EmptyTypes), null);
        return typeof(Range);
    }

    // ..a
    public override Type VisitOpen_ended_range_expression([NotNull] DassieParser.Open_ended_range_expressionContext context)
    {
        Type t = Visit(context.expression());

        if (t != typeof(Index))
        {
            if (CanBeConverted(t, typeof(Index)))
                EmitConversionOperator(t, typeof(Index));
            else
            {
                EmitErrorMessage(
                    context.expression().Start.Line,
                    context.expression().Start.Column,
                    context.expression().GetText().Length,
                    DS0155_RangeInvalidOperands,
                    $"Range expressions require operands of type 'System.Index'.");

                return typeof(Range);
            }
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, typeof(Range).GetMethod("EndAt", [typeof(Index)]), null);
        return typeof(Range);
    }

    // a..
    public override Type VisitClosed_ended_range_expression([NotNull] DassieParser.Closed_ended_range_expressionContext context)
    {
        Type t = Visit(context.expression());

        if (t != typeof(Index))
        {
            if (CanBeConverted(t, typeof(Index)))
                EmitConversionOperator(t, typeof(Index));
            else
            {
                EmitErrorMessage(
                    context.expression().Start.Line,
                    context.expression().Start.Column,
                    context.expression().GetText().Length,
                    DS0155_RangeInvalidOperands,
                    $"Range expressions require operands of type 'System.Index'.");

                return typeof(Range);
            }
        }

        CurrentMethod.IL.EmitCall(OpCodes.Call, typeof(Range).GetMethod("StartAt", [typeof(Index)]), null);
        return typeof(Range);
    }

    // a..b
    public override Type VisitDelimited_range_expression([NotNull] DassieParser.Delimited_range_expressionContext context)
    {
        Type t1 = Visit(context.expression()[0]);

        if (t1 != typeof(Index))
        {
            if (CanBeConverted(t1, typeof(Index)))
                EmitConversionOperator(t1, typeof(Index));
            else
            {
                EmitErrorMessage(
                    context.expression()[0].Start.Line,
                    context.expression()[0].Start.Column,
                    context.expression()[0].GetText().Length,
                    DS0155_RangeInvalidOperands,
                    $"Range expressions require operands of type 'System.Index'.");

                return typeof(Range);
            }
        }

        Type t2 = Visit(context.expression()[1]);

        if (t2 != typeof(Index))
        {
            if (CanBeConverted(t2, typeof(Index)))
                EmitConversionOperator(t2, typeof(Index));
            else
            {
                EmitErrorMessage(
                    context.expression()[1].Start.Line,
                    context.expression()[1].Start.Column,
                    context.expression()[1].GetText().Length,
                    DS0155_RangeInvalidOperands,
                    $"Range expressions require operands of type 'System.Index'.");

                return typeof(Range);
            }
        }

        CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(Range).GetConstructor([typeof(Index), typeof(Index)]));
        return typeof(Range);
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

    private int _loopIndex = 0;

    public override Type VisitWhile_loop([NotNull] DassieParser.While_loopContext context)
    {
        Type t = null;
        WarnIfConstantBoolean(context.expression().First());

        if (VisitorStep1CurrentMethod == null)
        {
            t = Visit(context.expression().First());
            CurrentMethod.LoopExpressionTypes.Add(_loopIndex++, t);
        }
        else
        {
            if (!VisitorStep1CurrentMethod.LoopExpressionTypes.TryGetValue(_loopIndex++, out t))
                t = typeof(object);
        }

        Type tReturn = null;

        CurrentMethod.LoopArrayTypeProbeIndex++;
        VisitorStep1CurrentMethod?.LoopArrayTypeProbes.TryGetValue(CurrentMethod.LoopArrayTypeProbeIndex, out tReturn);

        if (tReturn == null || tReturn == typeof(void))
            tReturn = typeof(object);

        if (IsIntegerType(t))
        {
            if (VisitorStep1CurrentMethod != null)
                Visit(context.expression().First());

            if (t != typeof(int))
                EmitConversionOperator(t, typeof(int));

            // Build the array of return values
            // (A for loop returns an array containing the return
            // values of every iteration of the loop)
            // The length of the array is already on the stack
            CurrentMethod.IL.Emit(OpCodes.Newarr, tReturn);

            // A local that saves the returning array
            LocalBuilder returnBuilder = CurrentMethod.IL.DeclareLocal(tReturn.MakeArrayType());

            CurrentMethod.Locals.Add(new(GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex++), returnBuilder, false, CurrentMethod.LocalIndex++));

            EmitStloc(CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);

            SetLocalSymInfo(returnBuilder,
                GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1));

            Label loop = CurrentMethod.IL.DefineLabel();
            Label start = CurrentMethod.IL.DefineLabel();

            LocalBuilder lb = CurrentMethod.IL.DeclareLocal(typeof(int));
            CurrentMethod.Locals.Add(new(GetThrowawayCounterVariableName(CurrentMethod.ThrowawayCounterVariableIndex++), lb, false, CurrentMethod.LocalIndex++));

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

            Type _tReturn = Visit(context.expression().Last());

            if (VisitorStep1 == null)
            {
                tReturn = _tReturn;

                if (tReturn == typeof(void))
                    tReturn = typeof(object);

                CurrentMethod.LoopArrayTypeProbes.Add(CurrentMethod.LoopArrayTypeProbeIndex, _tReturn);
            }

            if (_tReturn == typeof(void))
            {
                CurrentMethod.IL.Emit(OpCodes.Ldnull);
                CurrentMethod.IL.Emit(OpCodes.Stelem, typeof(object));
            }
            else
                CurrentMethod.IL.Emit(OpCodes.Stelem, tReturn);

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

            return tReturn.MakeArrayType();
        }

        if (IsBoolean(t))
        {
            Type listType = typeof(List<>).MakeGenericType(tReturn);

            EnsureBoolean(t, 0, 0, 0);

            if (VisitorStep1CurrentMethod == null)
                CurrentMethod.IL.Emit(OpCodes.Pop);

            CurrentMethod.IL.Emit(OpCodes.Newobj, listType.GetConstructor(Type.EmptyTypes));

            // A local that saves the returning list
            LocalBuilder returnBuilder = CurrentMethod.IL.DeclareLocal(listType);

            CurrentMethod.Locals.Add(new(GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex++), returnBuilder, false, CurrentMethod.LocalIndex++));

            EmitStloc(CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);

            SetLocalSymInfo(
                returnBuilder,
                GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1));

            Label loop = CurrentMethod.IL.DefineLabel();
            Label start = CurrentMethod.IL.DefineLabel();

            CurrentMethod.IL.Emit(OpCodes.Br, loop);

            CurrentMethod.IL.MarkLabel(start);

            EmitLdloc(CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);
            Type _tReturn = Visit(context.expression().Last());

            if (VisitorStep1 == null)
            {
                tReturn = _tReturn;

                if (tReturn == typeof(void))
                    tReturn = typeof(object);

                CurrentMethod.LoopArrayTypeProbes.Add(CurrentMethod.LoopArrayTypeProbeIndex, _tReturn);
            }

            if (_tReturn == typeof(void))
            {
                CurrentMethod.IL.Emit(OpCodes.Ldnull);
                _tReturn = typeof(object);
            }

            if (tReturn == typeof(object) && _tReturn != typeof(object))
                EmitConversionOperator(_tReturn, tReturn);

            CurrentMethod.IL.EmitCall(OpCodes.Callvirt, listType.GetMethod("Add", [tReturn]), null);

            CurrentMethod.IL.MarkLabel(loop);

            Visit(context.expression().First());
            CurrentMethod.IL.Emit(OpCodes.Brtrue, start);

            EmitLdloc(CurrentMethod.Locals.Where(l => l.Name == GetLoopArrayReturnValueVariableName(CurrentMethod.LoopArrayReturnValueIndex - 1)).First().Index + 1);
            CurrentMethod.SkipPop = false;

            return listType;
        }

        EmitWarningMessage(
            context.expression().First().Start.Line,
            context.expression().First().Start.Column,
            context.expression().First().Start.Text.Length,
            DS0043_PossiblyUnintentionalInfiniteLoop,
            "The loop condition is not boolean. The loop will run indefinetly.");

        if (VisitorStep1CurrentMethod != null)
            Visit(context.expression().First());

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
                loc.Scope = int.MaxValue;

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
        Type t = context.type_name() != null ? SymbolResolver.ResolveTypeName(context.type_name()) : typeof(object);

        CurrentMethod.IL.BeginCatchBlock(t);

        if (context.Identifier() == null)
            CurrentMethod.IL.Emit(OpCodes.Pop);

        else
        {
            LocalBuilder lb = CurrentMethod.IL.DeclareLocal(t);
            lb.SetLocalSymInfo(context.Identifier().GetIdentifier());

            LocalInfo loc = new(context.Identifier().GetIdentifier(), lb, true, ++CurrentMethod.LocalIndex, default);
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

    public override Type VisitFunction_pointer_expression([NotNull] DassieParser.Function_pointer_expressionContext context)
    {
        bool isLambdaExpression = false;
        MethodInfo pointerTarget = null;
        MethodInfo m = null;
        Type delegateType = null;
        FieldInfo instanceField = null;
        TypeBuilder closureType = null;

        if (!((List<Type>)[typeof(DassieParser.Member_access_expressionContext), typeof(DassieParser.Full_identifier_member_access_expressionContext), typeof(DassieParser.Anonymous_function_expressionContext)]).Contains(context.expression().GetType()))
        {
            EmitErrorMessage(
                context.expression().Start.Line,
                context.expression().Start.Column,
                context.expression().GetText().Length,
                DS0122_InvalidFunctionPointerTargetExpression,
                $"Invalid expression for function pointer.");

            return typeof(void);
        }

        if (context.expression().GetType() == typeof(DassieParser.Full_identifier_member_access_expressionContext))
            m = GetFunctionPointerTarget(context.expression() as DassieParser.Full_identifier_member_access_expressionContext, out closureType, out instanceField, out pointerTarget);

        if (context.expression().GetType() == typeof(DassieParser.Anonymous_function_expressionContext))
        {
            isLambdaExpression = true;
            m = HandleAnonymousFunction((DassieParser.Anonymous_function_expressionContext)context.expression(), out closureType);
            pointerTarget = m;
        }

        CurrentMethod.ClosureContainerType = closureType;

        if (context.op.Text == "func")
        {
            if (isLambdaExpression)
            {
                EmitLdloc(0);
                CurrentMethod.IL.Emit(OpCodes.Dup);
            }
            else
            {
                if (m.IsStatic)
                    CurrentMethod.IL.Emit(OpCodes.Ldnull);
                else
                    CurrentMethod.IL.Emit(OpCodes.Dup);
            }
        }

        EmitLdftn(m);

        if (context.op.Text == "func&") // Get raw pointer instead of Func[T] delegate
        {
            // TODO: Return proper function pointer type
            return typeof(nint);
        }

        ParameterInfo[] parameters = m.GetParameters();

        if (parameters.Length > 16)
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                context.GetText().Length,
                DS0121_FunctionPointerTooManyArguments,
                $"The function '{m.Name}' has {parameters.Length} parameters, which is more than the maximum supported amount of parameters for function pointers, which is 16.");
        }

        if (m.ReturnType == typeof(void))
        {
            if (parameters.Length == 0)
                delegateType = typeof(Action);
            else
            {
                string openActionName = $"System.Action`{parameters.Length}";
                Type openActionType = Type.GetType(openActionName);

                delegateType = openActionType.MakeGenericType(parameters.Select(p => p.ParameterType).ToArray());
            }
        }
        else
        {
            string openFuncName = $"System.Func`{parameters.Length + 1}";
            Type openFuncType = Type.GetType(openFuncName);

            List<Type> typeArgs = [];
            if (parameters.Length != 0)
                typeArgs.AddRange(parameters.Select(p => p.ParameterType));
            typeArgs.Add(m.ReturnType);

            delegateType = openFuncType.MakeGenericType(typeArgs.ToArray());
        }

        ConstructorInfo con = delegateType.GetConstructor([typeof(object), typeof(nint)]);
        if (con != null)
            CurrentMethod.IL.Emit(OpCodes.Newobj, con);

        return delegateType;
    }

    public override Type VisitConversion_expression([NotNull] DassieParser.Conversion_expressionContext context)
    {
        Type tSource = Visit(context.expression());
        Type tTarget = SymbolResolver.ResolveTypeName(context.type_name());

        if (!EmitConversionOperator(tSource, tTarget))
        {
            EmitErrorMessage(
                context.Start.Line,
                context.Start.Column,
                context.GetText().Length,
                DS0136_InvalidConversion,
                $"Unable to convert from type '{TypeName(tSource)}' to type '{TypeName(tTarget)}'.");
        }

        return tTarget;
    }

    private static readonly Dictionary<string, string> UnaryOperatorSpecialNames = new()
    {
        ["+"] = "UnaryPlus",
        ["-"] = "UnaryNegation",
        ["!"] = "LogicalNot",
        ["~"] = "OnesComplement",
        ["++"] = "Increment",
        ["--"] = "Decrement"
    };

    private static readonly Dictionary<string, string> BinaryOperatorSpecialNames = new()
    {
        ["+"] = "Addition",
        ["-"] = "Subtraction",
        ["*"] = "Multiply",
        ["/"] = "Division",
        ["%"] = "Modulus",
        ["&"] = "BitwiseAnd",
        ["|"] = "BitwiseOr",
        ["^"] = "ExclusiveOr",
        ["<<"] = "LeftShift",
        [">>"] = "RightShift",
        [">>>"] = "UnsignedRightShift",
        ["=="] = "Equality",
        ["!="] = "Inequality",
        ["<"] = "LessThan",
        [">"] = "GreaterThan",
        ["<="] = "LessThanOrEqual",
        [">="] = "GreaterThanOrEqual"
    };

    internal static (string MethodName, string OperatorName) GetMethodNameForCustomOperator(ITerminalNode customOperator, bool isUnary = false)
    {
        string fullName = customOperator.GetText();
        string operatorName = fullName;

        if ((fullName.StartsWith('/') && fullName.EndsWith('/'))
            || (fullName.StartsWith('(') && fullName.EndsWith(')')))
            operatorName = fullName[1..^1];

        // Operators with special names (for C# interop)

        string specialName = "";
        _ = isUnary && UnaryOperatorSpecialNames.TryGetValue(operatorName, out specialName);
        _ = !isUnary && BinaryOperatorSpecialNames.TryGetValue(operatorName, out specialName);

        if (!string.IsNullOrEmpty(specialName))
            return ($"op_{specialName}", operatorName);

        // Use same names for characters that F# uses to allow interop.
        // Even though some of the names are weird (twiddle?)

        string methodName = $"op_{string.Join("", operatorName.ToCharArray()
            .Select(c => $"{c switch
            {
                '!' => "Bang",
                '%' => "Percent",
                '&' => "Amp",
                '*' => "Multiply",
                '+' => "Plus",
                '-' => "Minus",
                '.' => "Dot",
                '/' => "Divide",
                '<' => "Less",
                '=' => "Equal",
                '>' => "Greater",
                '@' => "At",
                '^' => "Hat",
                '|' => "Bar",
                '~' => "Twiddle",
                _ => (int)c
            }}"))}";

        return (methodName, operatorName);
    }

    private void DefineCustomOperator(DassieParser.Type_memberContext context)
    {
        MethodContext mc = TypeContext.Current.GetMethod(context);
        MethodBuilder mb = mc.Builder;
        CurrentMethod = mc;

        var paramTypes = ResolveParameterList(context.parameter_list());
        mb.SetParameters(paramTypes.Select(p => p.Type).ToArray());

        Type _tReturn = typeof(object);

        InjectClosureParameterInitializers();

        Type tReturn = _tReturn;
        if (context.type_name() != null)
        {
            tReturn = SymbolResolver.ResolveTypeName(context.type_name());
            mb.SetReturnType(tReturn);
        }

        if (context.expression() != null)
        {
            tReturn = _tReturn;
            _tReturn = Visit(context.expression());
        }

        if (context.type_name() == null)
            tReturn = _tReturn;

        if (context.type_name() == null)
            mb.SetReturnType(_tReturn);

        if (context.expression() == null)
            _tReturn = tReturn;

        if (tReturn == typeof(void))
        {
            EmitErrorMessage(
                context.Custom_Operator().Symbol.Line,
                context.Custom_Operator().Symbol.Column,
                context.Custom_Operator().GetText().Length,
                DS0165_CustomOperatorNoReturnValue,
                "A custom operator must return a value. 'null' is an invalid return type.");
        }

        if (TypeContext.Current.GenericParameters.Select(t => t.Builder).Contains(tReturn))
        {
            if (tReturn.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant))
            {
                EmitErrorMessage(
                    context.type_name().Start.Line,
                    context.type_name().Start.Column,
                    context.type_name().GetText().Length,
                    DS0118_InvalidVariance,
                    $"Invalid variance: The type parameter '{tReturn.Name}' must be covariantly valid on '{mb.Name}'. '{tReturn.Name}' is contravariant.");
            }
        }

        if (_tReturn != tReturn)
        {
            if (CanBeConverted(_tReturn, tReturn))
                EmitConversionOperator(_tReturn, tReturn);
            else
            {
                EmitErrorMessage(
                    context.expression().Start.Line,
                    context.expression().Start.Column,
                    context.expression().GetText().Length,
                    DS0053_WrongReturnType,
                    $"Expected expression of type '{tReturn.FullName}', but got type '{TypeName(_tReturn)}'.");
            }
        }

        if (context.expression() != null)
            CurrentMethod.IL.Emit(OpCodes.Ret);

        CurrentFile.FunctionParameterConstraints.TryGetValue(mb.Name, out Dictionary<string, string> constraintsForCurrentFunction);
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

        if (TypeContext.Current.RequiredInterfaceImplementations.Any(m => m.Name == CurrentMethod.Builder.Name && m.ReturnType == tReturn && m.Parameters.SequenceEqual(CurrentMethod.Parameters.Select(p => p.Type))))
        {
            MockMethodInfo ovrMethod = TypeContext.Current.RequiredInterfaceImplementations.First(m => m.Name == CurrentMethod.Builder.Name && m.ReturnType == tReturn && m.Parameters.SequenceEqual(CurrentMethod.Parameters.Select(p => p.Type)));
            TypeContext.Current.RequiredInterfaceImplementations.Remove(ovrMethod);
            TypeContext.Current.Builder.DefineMethodOverride(mb, ovrMethod.Builder);
        }

        CurrentFile.Fragments.Add(new()
        {
            Color = Color.Function,
            Line = context.Custom_Operator().Symbol.Line,
            Column = context.Custom_Operator().Symbol.Column,
            Length = context.Custom_Operator().GetText().Length,
            ToolTip = TooltipGenerator.Function(mb.Name, tReturn, _params.ToArray()),
            IsNavigationTarget = true
        });

        CurrentMethod.ClosureContainerType?.CreateType();
    }

    private static MethodInfo[] GetOperatorMethods(ITerminalNode context, bool isUnary = false)
    {
        (string methodName, string operatorName) = GetMethodNameForCustomOperator(context, isUnary);
        MethodInfo[] operatorMethods = SymbolResolver.ResolveCustomOperatorOverloads(methodName);

        if (operatorMethods == null)
        {
            EmitErrorMessage(
                context.Symbol.Line,
                context.Symbol.Column,
                context.GetText().Length,
                DS0164_CustomOperatorNotFound,
                $"Could not resolve custom operator '{operatorName}'.");
        }

        return operatorMethods;
    }

    // TODO: Readd once parsing is figured out

    //public override Type VisitCustom_operator_unary_expression([NotNull] DassieParser.Custom_operator_unary_expressionContext context)
    //{
    //    MethodInfo[] methods = GetOperatorMethods(context.Custom_Operator(), true);
    //    if (methods == null)
    //        return typeof(void);

    //    Type tOperand = Visit(context.expression());
    //    MethodInfo final = null;

    //    foreach (MethodInfo candidate in methods)
    //    {
    //        if (candidate.GetParameters().Length != 1)
    //            continue;

    //        if (candidate.GetParameters()[0].ParameterType == tOperand)
    //        {
    //            final = candidate;
    //            break;
    //        }

    //        if (CanBeConverted(tOperand, candidate.GetParameters()[0].ParameterType))
    //        {
    //            EmitConversionOperator(tOperand, candidate.GetParameters()[0].ParameterType);
    //            final = candidate;
    //            break;
    //        }
    //    }

    //    if (final == null)
    //    {
    //        ErrorMessageHelpers.EmitDS0002Error(
    //            context.Custom_Operator().Symbol.Line,
    //            context.Custom_Operator().Symbol.Column,
    //            context.Custom_Operator().GetText().Length,
    //            context.Custom_Operator().GetText()[1..^1],
    //            methods.First().DeclaringType,
    //            methods,
    //            [tOperand]);

    //        return typeof(void);
    //    }

    //    CurrentMethod.IL.Emit(OpCodes.Call, final);
    //    return final.ReturnType;
    //}

    public override Type VisitCustom_operator_binary_expression([NotNull] DassieParser.Custom_operator_binary_expressionContext context)
    {
        MethodInfo[] methods = GetOperatorMethods(context.Custom_Operator());
        if (methods == null)
            return typeof(void);

        Type t1 = Visit(context.expression()[0]);
        Type t2 = Visit(context.expression()[1]);
        MethodInfo final = null;

        foreach (MethodInfo candidate in methods)
        {
            if (candidate.GetParameters().Length != 2)
                continue;

            if (candidate.GetParameters()[0].ParameterType == t1 && candidate.GetParameters()[1].ParameterType == t2)
            {
                final = candidate;
                break;
            }

            if (CanBeConverted(t1, candidate.GetParameters()[0].ParameterType) && CanBeConverted(t2, candidate.GetParameters()[1].ParameterType))
            {
                CurrentMethod.LocalIndex++;
                CurrentMethod.IL.DeclareLocal(t2);
                EmitStloc(CurrentMethod.LocalIndex);

                EmitConversionOperator(t1, candidate.GetParameters()[0].ParameterType);
                EmitLdloc(CurrentMethod.LocalIndex);
                EmitConversionOperator(t2, candidate.GetParameters()[1].ParameterType);

                final = candidate;
                break;
            }
        }

        if (final == null)
        {
            ErrorMessageHelpers.EmitDS0002Error(
                context.Custom_Operator().Symbol.Line,
                context.Custom_Operator().Symbol.Column,
                context.Custom_Operator().GetText().Length,
                context.Custom_Operator().GetText()[1..^1],
                methods.First().DeclaringType,
                methods,
                [t1, t2]);

            return typeof(void);
        }

        CurrentMethod.IL.Emit(OpCodes.Call, final);
        return final.ReturnType;
    }

    public override Type VisitIsinstance_expression([NotNull] DassieParser.Isinstance_expressionContext context)
    {
        Type t1 = Visit(context.expression());
        if (t1.IsValueType)
        {
            EmitErrorMessage(
                context.expression().Start.Line,
                context.expression().Start.Column,
                context.expression().GetText().Length,
                DS0168_InstanceCheckOperatorOnValueType,
                $"The ':?' operator is not valid on value types.");
        }

        Type comparedType = SymbolResolver.ResolveTypeName(context.type_name());
        CurrentMethod.IL.Emit(OpCodes.Isinst, comparedType);
        CurrentMethod.IL.Emit(OpCodes.Ldnull);
        CurrentMethod.IL.Emit(OpCodes.Cgt);
        return typeof(bool);
    }

    public override Type VisitSafe_conversion_expression([NotNull] DassieParser.Safe_conversion_expressionContext context)
    {
        Type t1 = Visit(context.expression());
        if (t1.IsValueType)
        {
            EmitErrorMessage(
                context.expression().Start.Line,
                context.expression().Start.Column,
                context.expression().GetText().Length,
                DS0168_InstanceCheckOperatorOnValueType,
                $"The '<?:' operator is not valid on value types.");
        }

        Type comparedType = SymbolResolver.ResolveTypeName(context.type_name());
        CurrentMethod.IL.Emit(OpCodes.Isinst, comparedType);
        return comparedType;
    }

    public override Type VisitLock_statement([NotNull] DassieParser.Lock_statementContext context)
    {
        Type t = Visit(context.expression()[0]);

        if (t == typeof(Lock))
        {
            int index = ++CurrentMethod.LocalIndex;
            CurrentMethod.IL.DeclareLocal(typeof(Lock.Scope));

            CurrentMethod.IL.Emit(OpCodes.Callvirt, t.GetMethod("EnterScope"));
            EmitStloc(index);

            CurrentMethod.IL.BeginExceptionBlock();

            Type t2 = Visit(context.expression()[1]);

            if (t2 != null && t2 != typeof(void))
                CurrentMethod.IL.Emit(OpCodes.Pop);

            CurrentMethod.IL.BeginFinallyBlock();
            EmitLdloca(index);
            CurrentMethod.IL.Emit(OpCodes.Call, typeof(Lock.Scope).GetMethod("Dispose"));

            CurrentMethod.IL.EndExceptionBlock();
            return typeof(void);
        }

        if (t.IsValueType || t.IsEnum)
        {
            EmitErrorMessage(
                context.expression()[0].Start.Line,
                context.expression()[0].Start.Column,
                context.expression()[0].GetText().Length,
                DS0176_LockOnValueType,
                $"The '$lock' statement is only valid on reference types. The provided expression is of type '{TypeName(t)}', which is a value type.");
        }

        int lockObjIndex = ++CurrentMethod.LocalIndex;
        int isTakenIndex = ++CurrentMethod.LocalIndex;
        Label end = CurrentMethod.IL.DefineLabel();
        CurrentMethod.IL.DeclareLocal(t);
        CurrentMethod.IL.DeclareLocal(typeof(bool));

        EmitStloc(lockObjIndex);
        EmitLdcI4(0);
        EmitStloc(isTakenIndex);

        CurrentMethod.IL.BeginExceptionBlock();

        EmitLdloc(lockObjIndex);
        EmitLdloca(isTakenIndex);
        CurrentMethod.IL.Emit(OpCodes.Call, typeof(Monitor).GetMethod("Enter", BindingFlags.Public | BindingFlags.Static, [typeof(object), typeof(bool).MakeByRefType()]));

        Type _t2 = Visit(context.expression()[1]);

        if (_t2 != null && _t2 != typeof(void))
            CurrentMethod.IL.Emit(OpCodes.Pop);

        CurrentMethod.IL.BeginFinallyBlock();

        EmitLdloc(isTakenIndex);
        CurrentMethod.IL.Emit(OpCodes.Brfalse, end);
        EmitLdloc(lockObjIndex);
        CurrentMethod.IL.Emit(OpCodes.Call, typeof(Monitor).GetMethod("Exit", BindingFlags.Public | BindingFlags.Static, [typeof(object)]));

        CurrentMethod.IL.MarkLabel(end);
        CurrentMethod.IL.EndExceptionBlock();
        return typeof(void);
    }
}