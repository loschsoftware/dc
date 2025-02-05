using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dassie.CodeGeneration.Helpers;
using Dassie.CodeGeneration.Structure;
using Dassie.Core;
using Dassie.Helpers;
using Dassie.Meta;
using Dassie.Parser;
using Dassie.Runtime;
using Dassie.Symbols;
using Dassie.Text;
using Dassie.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using static Dassie.Helpers.TypeHelpers;

namespace Dassie.CodeGeneration;

internal static class MemberDeclarationGeneration
{
    private static void DefineCustomOperator(DassieParser.Type_memberContext context)
    {
        //if (!(TypeContext.Current.Builder.IsAbstract && TypeContext.Current.Builder.IsSealed))
        //{
        //    EmitErrorMessage(
        //        context.Custom_Operator().Symbol.Line,
        //        context.Custom_Operator().Symbol.Column,
        //        context.Custom_Operator().GetText().Length,
        //        DS0160_CustomOperatorDefinedOutsideModule,
        //        "Custom operators can only be defined inside of modules.");

        //    return;
        //}

        if (context.member_access_modifier() != null && context.member_access_modifier().Global() == null)
        {
            EmitErrorMessage(
                context.member_access_modifier().Start.Line,
                context.member_access_modifier().Start.Column,
                context.member_access_modifier().GetText().Length,
                DS0161_CustomOperatorNotGlobal,
                "The only valid access modifier for a custom operator is 'global'.");

            return;
        }

        if (context.parameter_list().parameter().Length > 2)
        {
            EmitErrorMessage(
                context.parameter_list().Start.Line,
                context.parameter_list().Start.Column,
                context.parameter_list().GetText().Length,
                DS0162_CustomOperatorTooManyParameters,
                "A custom operator cannot have more than two operands.");

            return;
        }

        if (context.expression() == null)
        {
            EmitErrorMessage(
                context.Custom_Operator().Symbol.Line,
                context.Custom_Operator().Symbol.Column,
                context.Custom_Operator().GetText().Length,
                DS0163_CustomOperatorNoMethodBody,
                "Custom operators are required to have method bodies.");

            return;
        }

        (string methodName, string operatorName) = Visitor.GetMethodNameForCustomOperator(context.Custom_Operator());

        MethodAttributes attrib = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName;
        attrib |= MethodAttributes.HideBySig;

        MethodBuilder mb = TypeContext.Current.Builder.DefineMethod(methodName, attrib, CallingConventions.Standard);
        CurrentMethod = new()
        {
            Builder = mb,
            IL = mb.GetILGenerator(),
            IsCustomOperator = true,
            ParserRule = context
        };
        CurrentMethod.FilesWhereDefined.Add(CurrentFile.Path);

        if (!TypeContext.Current.ContainsCustomOperators)
        {
            TypeContext.Current.Builder.SetCustomAttribute(new(typeof(ContainsCustomOperatorsAttribute).GetConstructor([]), []));
            TypeContext.Current.ContainsCustomOperators = true;
        }

        mb.SetCustomAttribute(new(typeof(OperatorAttribute).GetConstructor([]), []));

        var paramTypes = Visitor.ResolveParameterList(context.parameter_list());
        mb.SetParameters(paramTypes.Select(p => p.Type).ToArray());

        foreach (var param in paramTypes)
        {
            ParameterBuilder pb = mb.DefineParameter(
                CurrentMethod.ParameterIndex++,
                AttributeHelpers.GetParameterAttributes(param.Context.parameter_modifier(), param.Context.Equals() != null),
                param.Context.Identifier().GetText());

            CurrentMethod.Parameters.Add(new(param.Context.Identifier().GetText(), param.Type, pb, pb.Position, param.Context.Var() != null));
        }

        Type _tReturn = typeof(object);

        InjectClosureParameterInitializers();

        Type tReturn = _tReturn;
        if (context.type_name() != null)
        {
            tReturn = SymbolResolver.ResolveTypeName(context.type_name());

            if (tReturn == null)
                CurrentMethod.ReturnTypeName = context.type_name();
            else
            {
                mb.SetReturnType(tReturn);
                CurrentMethod.UnresolvedReturnType = false;
            }
        }

        CurrentFile.FunctionParameterConstraints.TryGetValue(operatorName, out Dictionary<string, string> constraintsForCurrentFunction);
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

        CurrentMethod.ClosureContainerType?.CreateType();
    }

    private static void HandleConstructor(DassieParser.Type_memberContext context, TypeContext parent)
    {
        if (parent.IsEnumeration)
        {
            EmitErrorMessage(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetText().Length,
                DS0141_MethodInEnumeration,
                "Enumeration types cannot contain constructors.");
        }

        CallingConventions callingConventions = CallingConventions.HasThis;

        if (context.member_special_modifier().Any(m => m.Static() != null))
            callingConventions = CallingConventions.Standard;

        var paramTypes = Visitor.ResolveParameterList(context.parameter_list());

        (MethodAttributes attribs, MethodImplAttributes implementationFlags) = AttributeHelpers.GetMethodAttributes(context.member_access_modifier(), context.member_oop_modifier(), context.member_special_modifier(), context.attribute());
        if (attribs.HasFlag(MethodAttributes.Virtual))
            attribs &= ~MethodAttributes.Virtual;

        ConstructorBuilder cb = parent.Builder.DefineConstructor(attribs, callingConventions, paramTypes.Select(p => p.Type).ToArray());
        cb.SetImplementationFlags(implementationFlags);

        ILGenerator il = null;

        if (!implementationFlags.HasFlag(MethodImplAttributes.Runtime))
            il = cb.GetILGenerator();

        CurrentMethod = new()
        {
            ConstructorBuilder = cb,
            IL = il,
            ParserRule = context
        };

        CurrentMethod.FilesWhereDefined.Add(CurrentFile.Path);
        TypeContext.Current.ConstructorContexts.Add(CurrentMethod);

        foreach (var param in paramTypes)
        {
            ParameterBuilder pb = cb.DefineParameter(
                CurrentMethod.ParameterIndex++,
                AttributeHelpers.GetParameterAttributes(param.Context.parameter_modifier(), param.Context.Equals() != null),
                param.Context.Identifier().GetText());

            CurrentMethod.Parameters.Add(new(param.Context.Identifier().GetText(), param.Type, pb, CurrentMethod.ParameterIndex, param.Context.Var() != null));
        }

        if (CurrentMethod.ConstructorBuilder.IsStatic)
        {
            foreach (var param in CurrentMethod.Parameters)
                param.Index--;
        }

        List<(Type, string)> _params = new();
        foreach (var param in paramTypes)
            _params.Add((param.Type, param.Context.Identifier().GetText()));
    }

    public static void GenerateMember(DassieParser.Type_memberContext context, TypeContext declaringType, bool ignoreDS0058 = false, bool alwaysGlobal = false)
    {
        if (context.Custom_Operator() != null)
        {
            DefineCustomOperator(context);
            return;
        }

        if (context.Identifier().GetText() == TypeContext.Current.Builder.Name)
        {
            // Defer constructors for field initializers
            //TypeContext.Current.Constructors.Add(context);
            HandleConstructor(context, declaringType);
            return;
        }

        if (context.Var() != null && context.parameter_list() != null)
        {
            EmitErrorMessage(
                context.Var().Symbol.Line,
                context.Var().Symbol.Column,
                context.Var().GetText().Length,
                DS0083_InvalidVarModifier,
                "The modifier 'var' cannot be used on methods.");
        }

        if (context.parameter_list() != null && context.member_special_modifier() != null && context.member_special_modifier().Any(s => s.Literal() != null))
        {
            DassieParser.Member_special_modifierContext rule = context.member_special_modifier().First(s => s.Literal != null);

            EmitErrorMessage(
                rule.Start.Line,
                rule.Start.Column,
                rule.GetText().Length,
                DS0137_LiteralModifierOnMethod,
                "The modifier 'literal' cannot be used on methods.");
        }

        Type _tReturn = typeof(object);

        if (context.parameter_list() != null || _tReturn == typeof(void))
        {
            if (TypeContext.Current.IsEnumeration)
            {
                EmitErrorMessage(
                    context.Identifier().Symbol.Line,
                    context.Identifier().Symbol.Column,
                    context.Identifier().GetText().Length,
                    DS0141_MethodInEnumeration,
                    "Enumeration types cannot contain methods.");
            }

            CallingConventions callingConventions = CallingConventions.HasThis;

            if (context.member_special_modifier().Any(m => m.Static() != null) || (TypeContext.Current.Builder.IsSealed && TypeContext.Current.Builder.IsAbstract))
                callingConventions = CallingConventions.Standard;

            (MethodAttributes attrib, MethodImplAttributes implementationFlags) = AttributeHelpers.GetMethodAttributes(
                    context.member_access_modifier(),
                    context.member_oop_modifier(),
                    context.member_special_modifier(),
                    context.attribute(),
                    ignoreDS0058);

            if (alwaysGlobal && attrib.HasFlag(MethodAttributes.Private))
            {
                attrib &= ~MethodAttributes.Private;
                attrib |= MethodAttributes.Public;
            }

            if (attrib.HasFlag(MethodAttributes.PinvokeImpl))
            {
                // TODO: Implement P/Invoke methods
                //MethodBuilder pInvokeMethod = TypeContext.Current.Builder.DefinePInvokeMethod();

                return;
            }

            MethodInfo ovr = null;

            if (VisitorStep1 != null)
            {
                List<MockMethodInfo> interfaceMethods = TypeContext.Current.RequiredInterfaceImplementations;
                foreach (MockMethodInfo method in interfaceMethods)
                {
                    if (method.Name == context.Identifier().GetText() && method.ReturnType == (VisitorStep1CurrentMethod == null ? typeof(DassieParser) : VisitorStep1CurrentMethod.Builder.ReturnType) && method.Parameters.SequenceEqual(VisitorStep1CurrentMethod == null ? [] : VisitorStep1CurrentMethod.Builder.GetParameters().Select(p => p.ParameterType)))
                    {
                        if (method.IsAbstract)
                        {
                            attrib |= MethodAttributes.HideBySig;
                            attrib |= MethodAttributes.NewSlot;
                            attrib |= MethodAttributes.Virtual;
                        }
                        else if (method.Builder.IsStatic)
                            ovr = method.Builder;
                    }
                }
            }

            if (attrib.HasFlag(MethodAttributes.Static))
                attrib &= ~MethodAttributes.Virtual;

            MethodBuilder mb = declaringType.Builder.DefineMethod(
                context.Identifier().GetText(),
                attrib,
                callingConventions);

            mb.SetImplementationFlags(implementationFlags);

            if (ovr != null)
                TypeContext.Current.Builder.DefineMethodOverride(mb, ovr);

            CurrentMethod = new()
            {
                Builder = mb,
                UnresolvedReturnType = true,
                ParserRule = context
            };

            if (!attrib.HasFlag(MethodAttributes.Abstract) && !implementationFlags.HasFlag(MethodImplAttributes.Runtime))
                CurrentMethod.IL = mb.GetILGenerator();

            if (context.type_parameter_list() != null)
            {
                List<TypeParameterContext> typeParamContexts = [];

                foreach (DassieParser.Type_parameterContext typeParam in context.type_parameter_list().type_parameter())
                {
                    if (typeParamContexts.Any(p => p.Name == typeParam.Identifier().GetText()))
                    {
                        EmitErrorMessage(
                            typeParam.Start.Line,
                            typeParam.Start.Column,
                            typeParam.GetText().Length,
                            DS0112_DuplicateTypeParameter,
                            $"Duplicate type parameter '{typeParam.GetText()}'.");

                        continue;
                    }

                    if (TypeContext.Current.TypeParameters.Any(t => t.Name == typeParam.Identifier().GetText()))
                    {
                        EmitErrorMessage(
                            typeParam.Start.Line,
                            typeParam.Start.Column,
                            typeParam.GetText().Length,
                            DS0114_TypeParameterIsDefinedInContainingScope,
                            $"The type parameter '{typeParam.Identifier().GetText()}' is already declared by the containing type '{Format(TypeContext.Current.Builder)}'.");
                    }

                    typeParamContexts.Add(BuildTypeParameter(typeParam));
                }

                GenericTypeParameterBuilder[] typeParams = mb.DefineGenericParameters(typeParamContexts.Select(t => t.Name).ToArray());
                foreach (GenericTypeParameterBuilder typeParam in typeParams)
                {
                    TypeParameterContext tpc = typeParamContexts.First(c => c.Name == typeParam.Name);
                    typeParam.SetGenericParameterAttributes(tpc.Attributes);
                    typeParam.SetBaseTypeConstraint(tpc.BaseTypeConstraint);
                    typeParam.SetInterfaceConstraints(tpc.InterfaceConstraints.ToArray());

                    tpc.Builder = typeParam;
                }

                CurrentMethod.TypeParameters = typeParamContexts;
            }

            CurrentMethod.FilesWhereDefined.Add(CurrentFile.Path);

            var paramTypes = Visitor.ResolveParameterList(context.parameter_list(), true);

            if (paramTypes.Any(p => p.Type == null))
                CurrentMethod.ParameterTypeNames = context.parameter_list().parameter().Select(p => p.type_name()).ToList();
            else
            {
                mb.SetParameters(paramTypes.Select(p => p.Type).ToArray());

                foreach (var param in paramTypes)
                {
                    ParameterBuilder pb = mb.DefineParameter(
                        CurrentMethod.ParameterIndex++ + 1, // Add 1 so parameter indices start at 1 -> 0 is always the current instance of the containing type
                        AttributeHelpers.GetParameterAttributes(param.Context.parameter_modifier(), param.Context.Equals() != null),
                        param.Context.Identifier().GetText());

                    CurrentMethod.Parameters.Add(new(param.Context.Identifier().GetText(), param.Type, pb, pb.Position, param.Context.Var() != null));

                    if (CurrentMethod.Builder.IsStatic)
                    {
                        foreach (var _param in CurrentMethod.Parameters)
                        {
                            if (_param.Index > 0)
                                _param.Index--;
                        }
                    }
                }
            }

            if (context.expression() == null)
            {
                if (!attrib.HasFlag(MethodAttributes.Abstract) && !implementationFlags.HasFlag(MethodImplAttributes.Runtime))
                {
                    EmitErrorMessage(
                        context.Start.Line,
                        context.Start.Column,
                        context.GetText().Length,
                        DS0115_NonAbstractMethodHasNoBody,
                        $"The non-abstract member '{mb.Name}' needs to define a body.");

                    CurrentMethod.IL.Emit(OpCodes.Ret);
                }
            }

            if (attrib.HasFlag(MethodAttributes.Abstract) && context.expression() != null)
            {
                EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    context.GetText().Length,
                    DS0116_AbstractMethodHasBody,
                    $"The abstract member '{mb.Name}' cannot define a body.");

                return;
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

            //InjectClosureParameterInitializers();

            Type tReturn = _tReturn;
            if (context.type_name() != null)
            {
                tReturn = SymbolResolver.ResolveTypeName(context.type_name());

                if (tReturn == null)
                    CurrentMethod.ReturnTypeName = context.type_name();
                else
                {
                    mb.SetReturnType(tReturn);
                    CurrentMethod.UnresolvedReturnType = false;
                }
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

            return;
        }

        CreateFakeMethod();

        Type type = typeof(object);

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
                context.Identifier().GetText().Length,
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

        bool isInitOnly = declaringType.IsImmutable || context.Val() != null;
        FieldAttributes fieldAttribs = AttributeHelpers.GetFieldAttributes(context.member_access_modifier(), context.member_oop_modifier(), context.member_special_modifier(), isInitOnly);

        if (declaringType.Builder.IsInterface && !fieldAttribs.HasFlag(FieldAttributes.Static))
        {
            EmitErrorMessage(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetText().Length,
                DS0158_InstanceFieldInTemplate,
                $"Template types cannot contain instance {memberKindPlural}.");
        }

        if ((type.IsByRef /*|| type.IsByRefLike*/) && !TypeContext.Current.IsByRefLike)
        {
            EmitErrorMessage(
                context.Identifier().Symbol.Line,
                context.Identifier().Symbol.Column,
                context.Identifier().GetText().Length,
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

            string propName = context.Identifier().GetText();
            FieldBuilder backingField = declaringType.Builder.DefineField(
                SymbolNameGenerator.GetPropertyBackingFieldName(propName),
                type,
                FieldAttributes.Private);

            PropertyBuilder pb = declaringType.Builder.DefineProperty(
                propName,
                PropertyAttributes.None,
                type, []);

            TypeContext.Current.Properties.Add(pb);

            (MethodAttributes attribs, _) = AttributeHelpers.GetMethodAttributes(context.member_access_modifier(), context.member_oop_modifier(), context.member_special_modifier(), []);
            attribs |= MethodAttributes.SpecialName;

            if (!attribs.HasFlag(MethodAttributes.HideBySig))
                attribs |= MethodAttributes.HideBySig;

            MethodBuilder getter = TypeContext.Current.Builder.DefineMethod($"get_{propName}", attribs, type, []);
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
                Length = context.Identifier().GetText().Length,
                ToolTip = TooltipGenerator.Property(pb),
                IsNavigationTarget = true
            });

            return;
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

                return;
            }

            string eventName = context.Identifier().GetText();

            FieldBuilder eventField = declaringType.Builder.DefineField(
                eventName,
                type,
                fieldAttribs);

            TypeContext.Current.Fields.Add(new()
            {
                Builder = eventField,
                Name = eventName
            });

            EventBuilder eb = declaringType.Builder.DefineEvent(
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

                //Visit(context.property_or_event_block().add_handler()[0].expression());
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

                //Visit(context.property_or_event_block().remove_handler()[0].expression());
                removeMethodIL.Emit(OpCodes.Ret);
            }
            else
                EventDefaultHandlerCodeGeneration.GenerateDefaultRemoveHandlerImplementation(eventField);

            if (context.property_or_event_block() != null && (context.property_or_event_block().add_handler().Length == 0 ^ context.property_or_event_block().remove_handler().Length == 0))
            {
                EmitErrorMessage(
                    context.Identifier().Symbol.Line,
                    context.Identifier().Symbol.Column,
                    context.Identifier().GetText().Length,
                    DS0175_EventMissingHandlers,
                    $"Event '{eventName}' is missing a{(context.property_or_event_block().add_handler().Length == 0 ? "n" : "")} '{(context.property_or_event_block().add_handler().Length == 0 ? "add" : "remove")}' handler.");
            }

            eb.SetAddOnMethod(addMethod);
            eb.SetRemoveOnMethod(removeMethod);

            CurrentMethod = current;
            return;
        }

        FieldBuilder fb = TypeContext.Current.Builder.DefineField(
            context.Identifier().GetText(),
            type,
            modreq.ToArray(),
            modopt.ToArray(),
            fieldAttribs);

        foreach (CustomAttributeBuilder cab in customAttribs)
            fb.SetCustomAttribute(cab);

        MetaFieldInfo mfi = new(context.Identifier().GetText(), fb);
        mfi.ParserRule = context;

        if (context.member_special_modifier() != null && context.member_special_modifier().Any(l => l.Literal() != null))
        {
            Expression result = ExpressionEvaluator.Instance.Visit(context.expression());

            if (result == null)
            {
                EmitErrorMessage(
                    context.expression().Start.Line,
                    context.expression().Start.Column,
                    context.expression().GetText().Length,
                    DS0138_CompileTimeConstantRequired,
                    "Compile-time constant expected.");

                return;
            }

            mfi.ConstantValue = result.Value;
            fb.SetConstant(result.Value);
        }

        else if (context.expression() != null)
            TypeContext.Current.FieldInitializers.Add((fb, context.expression()));

        TypeContext.Current.Fields.Add(mfi);

        CurrentFile.Fragments.Add(new()
        {
            Color = Color.Field,
            Column = context.Identifier().Symbol.Column,
            Line = context.Identifier().Symbol.Line,
            Length = context.Identifier().GetText().Length,
            ToolTip = TooltipGenerator.Field(fb),
            IsNavigationTarget = true
        });
    }
}