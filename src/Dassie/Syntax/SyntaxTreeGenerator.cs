using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Dassie.Parser;

namespace Dassie.Syntax;

internal class SyntaxTreeGenerator : DassieParserBaseVisitor<SyntaxNode>
{
    private static SyntaxToken ToSyntaxToken(ITerminalNode terminal)
        => ToSyntaxToken(terminal.Symbol);

    private static SyntaxToken ToSyntaxToken(IToken token)
    {
        return new();
    }

    public override SyntaxNode VisitAccess_modifier_member_group([NotNull] DassieParser.Access_modifier_member_groupContext context)
    {
        return base.VisitAccess_modifier_member_group(context);
    }

    public override SyntaxNode VisitAddition_expression([NotNull] DassieParser.Addition_expressionContext context)
    {
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = new(), // TODO
            FullSpan = new(),
            OperatorToken = ToSyntaxToken(context.Plus()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitAdd_handler([NotNull] DassieParser.Add_handlerContext context)
    {
        return base.VisitAdd_handler(context);
    }

    public override SyntaxNode VisitAnd_expression([NotNull] DassieParser.And_expressionContext context)
    {
        return base.VisitAnd_expression(context);
    }

    public override SyntaxNode VisitAnonymous_function_expression([NotNull] DassieParser.Anonymous_function_expressionContext context)
    {
        return base.VisitAnonymous_function_expression(context);
    }

    public override SyntaxNode VisitArglist([NotNull] DassieParser.ArglistContext context)
    {
        return base.VisitArglist(context);
    }

    public override SyntaxNode VisitArray_element_assignment([NotNull] DassieParser.Array_element_assignmentContext context)
    {
        return base.VisitArray_element_assignment(context);
    }

    public override SyntaxNode VisitArray_expression([NotNull] DassieParser.Array_expressionContext context)
    {
        return base.VisitArray_expression(context);
    }

    public override SyntaxNode VisitAssignment([NotNull] DassieParser.AssignmentContext context)
    {
        return base.VisitAssignment(context);
    }

    public override SyntaxNode VisitAssignment_operator([NotNull] DassieParser.Assignment_operatorContext context)
    {
        return base.VisitAssignment_operator(context);
    }

    public override SyntaxNode VisitAtom([NotNull] DassieParser.AtomContext context)
    {
        return base.VisitAtom(context);
    }

    public override SyntaxNode VisitAtom_expression([NotNull] DassieParser.Atom_expressionContext context)
    {
        return base.VisitAtom_expression(context);
    }

    public override SyntaxNode VisitAttribute([NotNull] DassieParser.AttributeContext context)
    {
        return base.VisitAttribute(context);
    }

    public override SyntaxNode VisitAttributed_expression([NotNull] DassieParser.Attributed_expressionContext context)
    {
        return base.VisitAttributed_expression(context);
    }

    public override SyntaxNode VisitBasic_import([NotNull] DassieParser.Basic_importContext context)
    {
        return base.VisitBasic_import(context);
    }

    public override SyntaxNode VisitBitwise_complement_expression([NotNull] DassieParser.Bitwise_complement_expressionContext context)
    {
        return base.VisitBitwise_complement_expression(context);
    }

    public override SyntaxNode VisitBlock_expression([NotNull] DassieParser.Block_expressionContext context)
    {
        return base.VisitBlock_expression(context);
    }

    public override SyntaxNode VisitBoolean_atom([NotNull] DassieParser.Boolean_atomContext context)
    {
        return base.VisitBoolean_atom(context);
    }

    public override SyntaxNode VisitByref_expression([NotNull] DassieParser.Byref_expressionContext context)
    {
        return base.VisitByref_expression(context);
    }

    public override SyntaxNode VisitCatch_branch([NotNull] DassieParser.Catch_branchContext context)
    {
        return base.VisitCatch_branch(context);
    }

    public override SyntaxNode VisitCharacter_atom([NotNull] DassieParser.Character_atomContext context)
    {
        return base.VisitCharacter_atom(context);
    }

    public override SyntaxNode VisitClosed_ended_range_expression([NotNull] DassieParser.Closed_ended_range_expressionContext context)
    {
        return base.VisitClosed_ended_range_expression(context);
    }

    public override SyntaxNode VisitCode_block([NotNull] DassieParser.Code_blockContext context)
    {
        return base.VisitCode_block(context);
    }

    public override SyntaxNode VisitComparison_expression([NotNull] DassieParser.Comparison_expressionContext context)
    {
        return base.VisitComparison_expression(context);
    }

    public override SyntaxNode VisitCompilation_unit([NotNull] DassieParser.Compilation_unitContext context)
    {
        return base.VisitCompilation_unit(context);
    }

    public override SyntaxNode VisitConversion_expression([NotNull] DassieParser.Conversion_expressionContext context)
    {
        return base.VisitConversion_expression(context);
    }

    public override SyntaxNode VisitCustom_operator_binary_expression([NotNull] DassieParser.Custom_operator_binary_expressionContext context)
    {
        return base.VisitCustom_operator_binary_expression(context);
    }

    public override SyntaxNode VisitDelimited_range_expression([NotNull] DassieParser.Delimited_range_expressionContext context)
    {
        return base.VisitDelimited_range_expression(context);
    }

    public override SyntaxNode VisitDereference_expression([NotNull] DassieParser.Dereference_expressionContext context)
    {
        return base.VisitDereference_expression(context);
    }

    public override SyntaxNode VisitDictionary_expression([NotNull] DassieParser.Dictionary_expressionContext context)
    {
        return base.VisitDictionary_expression(context);
    }

    public override SyntaxNode VisitDivide_expression([NotNull] DassieParser.Divide_expressionContext context)
    {
        return base.VisitDivide_expression(context);
    }

    public override SyntaxNode VisitElif_branch([NotNull] DassieParser.Elif_branchContext context)
    {
        return base.VisitElif_branch(context);
    }

    public override SyntaxNode VisitElse_branch([NotNull] DassieParser.Else_branchContext context)
    {
        return base.VisitElse_branch(context);
    }

    public override SyntaxNode VisitElse_unless_branch([NotNull] DassieParser.Else_unless_branchContext context)
    {
        return base.VisitElse_unless_branch(context);
    }

    public override SyntaxNode VisitEmpty_atom([NotNull] DassieParser.Empty_atomContext context)
    {
        return base.VisitEmpty_atom(context);
    }

    public override SyntaxNode VisitEquality_expression([NotNull] DassieParser.Equality_expressionContext context)
    {
        return base.VisitEquality_expression(context);
    }

    public override SyntaxNode VisitExport_directive([NotNull] DassieParser.Export_directiveContext context)
    {
        return base.VisitExport_directive(context);
    }

    public override SyntaxNode VisitExpression_atom([NotNull] DassieParser.Expression_atomContext context)
    {
        return base.VisitExpression_atom(context);
    }

    public override SyntaxNode VisitExternal_block([NotNull] DassieParser.External_blockContext context)
    {
        return base.VisitExternal_block(context);
    }

    public override SyntaxNode VisitFault_branch([NotNull] DassieParser.Fault_branchContext context)
    {
        return base.VisitFault_branch(context);
    }

    public override SyntaxNode VisitField_access_modifier([NotNull] DassieParser.Field_access_modifierContext context)
    {
        return base.VisitField_access_modifier(context);
    }

    public override SyntaxNode VisitField_declaration([NotNull] DassieParser.Field_declarationContext context)
    {
        return base.VisitField_declaration(context);
    }

    public override SyntaxNode VisitFile_body([NotNull] DassieParser.File_bodyContext context)
    {
        return base.VisitFile_body(context);
    }

    public override SyntaxNode VisitFinally_branch([NotNull] DassieParser.Finally_branchContext context)
    {
        return base.VisitFinally_branch(context);
    }

    public override SyntaxNode VisitForeach_loop([NotNull] DassieParser.Foreach_loopContext context)
    {
        return base.VisitForeach_loop(context);
    }

    public override SyntaxNode VisitFull_identifier([NotNull] DassieParser.Full_identifierContext context)
    {
        return base.VisitFull_identifier(context);
    }

    public override SyntaxNode VisitFull_identifier_member_access_expression([NotNull] DassieParser.Full_identifier_member_access_expressionContext context)
    {
        return base.VisitFull_identifier_member_access_expression(context);
    }

    public override SyntaxNode VisitFull_program([NotNull] DassieParser.Full_programContext context)
    {
        return base.VisitFull_program(context);
    }

    public override SyntaxNode VisitFull_range_expression([NotNull] DassieParser.Full_range_expressionContext context)
    {
        return base.VisitFull_range_expression(context);
    }

    public override SyntaxNode VisitFunction_pointer_expression([NotNull] DassieParser.Function_pointer_expressionContext context)
    {
        return base.VisitFunction_pointer_expression(context);
    }

    public override SyntaxNode VisitFunction_pointer_parameter_list([NotNull] DassieParser.Function_pointer_parameter_listContext context)
    {
        return base.VisitFunction_pointer_parameter_list(context);
    }

    public override SyntaxNode VisitGeneric_argument([NotNull] DassieParser.Generic_argumentContext context)
    {
        return base.VisitGeneric_argument(context);
    }

    public override SyntaxNode VisitGeneric_arg_list([NotNull] DassieParser.Generic_arg_listContext context)
    {
        return base.VisitGeneric_arg_list(context);
    }

    public override SyntaxNode VisitGeneric_identifier([NotNull] DassieParser.Generic_identifierContext context)
    {
        return base.VisitGeneric_identifier(context);
    }

    public override SyntaxNode VisitGeneric_parameter([NotNull] DassieParser.Generic_parameterContext context)
    {
        return base.VisitGeneric_parameter(context);
    }

    public override SyntaxNode VisitGeneric_parameter_attribute([NotNull] DassieParser.Generic_parameter_attributeContext context)
    {
        return base.VisitGeneric_parameter_attribute(context);
    }

    public override SyntaxNode VisitGeneric_parameter_list([NotNull] DassieParser.Generic_parameter_listContext context)
    {
        return base.VisitGeneric_parameter_list(context);
    }

    public override SyntaxNode VisitGeneric_parameter_variance([NotNull] DassieParser.Generic_parameter_varianceContext context)
    {
        return base.VisitGeneric_parameter_variance(context);
    }

    public override SyntaxNode VisitIdentifier_atom([NotNull] DassieParser.Identifier_atomContext context)
    {
        return base.VisitIdentifier_atom(context);
    }

    public override SyntaxNode VisitIf_branch([NotNull] DassieParser.If_branchContext context)
    {
        return base.VisitIf_branch(context);
    }

    public override SyntaxNode VisitImplementation_query_expression([NotNull] DassieParser.Implementation_query_expressionContext context)
    {
        return base.VisitImplementation_query_expression(context);
    }

    public override SyntaxNode VisitIndex_expression([NotNull] DassieParser.Index_expressionContext context)
    {
        return base.VisitIndex_expression(context);
    }

    public override SyntaxNode VisitInheritance_list([NotNull] DassieParser.Inheritance_listContext context)
    {
        return base.VisitInheritance_list(context);
    }

    public override SyntaxNode VisitInline_predicate([NotNull] DassieParser.Inline_predicateContext context)
    {
        return base.VisitInline_predicate(context);
    }

    public override SyntaxNode VisitInline_predicate_atom([NotNull] DassieParser.Inline_predicate_atomContext context)
    {
        return base.VisitInline_predicate_atom(context);
    }

    public override SyntaxNode VisitInteger_atom([NotNull] DassieParser.Integer_atomContext context)
    {
        return base.VisitInteger_atom(context);
    }

    public override SyntaxNode VisitIsinstance_expression([NotNull] DassieParser.Isinstance_expressionContext context)
    {
        return base.VisitIsinstance_expression(context);
    }

    public override SyntaxNode VisitLeft_pipe_expression([NotNull] DassieParser.Left_pipe_expressionContext context)
    {
        return base.VisitLeft_pipe_expression(context);
    }

    public override SyntaxNode VisitLeft_shift_expression([NotNull] DassieParser.Left_shift_expressionContext context)
    {
        return base.VisitLeft_shift_expression(context);
    }

    public override SyntaxNode VisitList_initializer_expression([NotNull] DassieParser.List_initializer_expressionContext context)
    {
        return base.VisitList_initializer_expression(context);
    }

    public override SyntaxNode VisitLocal_declaration_or_assignment([NotNull] DassieParser.Local_declaration_or_assignmentContext context)
    {
        return base.VisitLocal_declaration_or_assignment(context);
    }

    public override SyntaxNode VisitLocal_function([NotNull] DassieParser.Local_functionContext context)
    {
        return base.VisitLocal_function(context);
    }

    public override SyntaxNode VisitLock_statement([NotNull] DassieParser.Lock_statementContext context)
    {
        return base.VisitLock_statement(context);
    }

    public override SyntaxNode VisitLogical_and_expression([NotNull] DassieParser.Logical_and_expressionContext context)
    {
        return base.VisitLogical_and_expression(context);
    }

    public override SyntaxNode VisitLogical_negation_expression([NotNull] DassieParser.Logical_negation_expressionContext context)
    {
        return base.VisitLogical_negation_expression(context);
    }

    public override SyntaxNode VisitLogical_or_expression([NotNull] DassieParser.Logical_or_expressionContext context)
    {
        return base.VisitLogical_or_expression(context);
    }

    public override SyntaxNode VisitMatch_alternative_case([NotNull] DassieParser.Match_alternative_caseContext context)
    {
        return base.VisitMatch_alternative_case(context);
    }

    public override SyntaxNode VisitMatch_block([NotNull] DassieParser.Match_blockContext context)
    {
        return base.VisitMatch_block(context);
    }

    public override SyntaxNode VisitMatch_case_expression([NotNull] DassieParser.Match_case_expressionContext context)
    {
        return base.VisitMatch_case_expression(context);
    }

    public override SyntaxNode VisitMatch_default_case([NotNull] DassieParser.Match_default_caseContext context)
    {
        return base.VisitMatch_default_case(context);
    }

    public override SyntaxNode VisitMatch_expr([NotNull] DassieParser.Match_exprContext context)
    {
        return base.VisitMatch_expr(context);
    }

    public override SyntaxNode VisitMatch_expression([NotNull] DassieParser.Match_expressionContext context)
    {
        return base.VisitMatch_expression(context);
    }

    public override SyntaxNode VisitMatch_first_case([NotNull] DassieParser.Match_first_caseContext context)
    {
        return base.VisitMatch_first_case(context);
    }

    public override SyntaxNode VisitMember_access_expression([NotNull] DassieParser.Member_access_expressionContext context)
    {
        return base.VisitMember_access_expression(context);
    }

    public override SyntaxNode VisitMember_access_modifier([NotNull] DassieParser.Member_access_modifierContext context)
    {
        return base.VisitMember_access_modifier(context);
    }

    public override SyntaxNode VisitMember_oop_modifier([NotNull] DassieParser.Member_oop_modifierContext context)
    {
        return base.VisitMember_oop_modifier(context);
    }

    public override SyntaxNode VisitMember_special_modifier([NotNull] DassieParser.Member_special_modifierContext context)
    {
        return base.VisitMember_special_modifier(context);
    }

    public override SyntaxNode VisitModulus_expression([NotNull] DassieParser.Modulus_expressionContext context)
    {
        return base.VisitModulus_expression(context);
    }

    public override SyntaxNode VisitMultiply_expression([NotNull] DassieParser.Multiply_expressionContext context)
    {
        return base.VisitMultiply_expression(context);
    }

    public override SyntaxNode VisitNested_type_access_modifier([NotNull] DassieParser.Nested_type_access_modifierContext context)
    {
        return base.VisitNested_type_access_modifier(context);
    }

    public override SyntaxNode VisitNewlined_expression([NotNull] DassieParser.Newlined_expressionContext context)
    {
        return base.VisitNewlined_expression(context);
    }

    public override SyntaxNode VisitOpen_ended_range_expression([NotNull] DassieParser.Open_ended_range_expressionContext context)
    {
        return base.VisitOpen_ended_range_expression(context);
    }

    public override SyntaxNode VisitOr_expression([NotNull] DassieParser.Or_expressionContext context)
    {
        return base.VisitOr_expression(context);
    }

    public override SyntaxNode VisitParameter([NotNull] DassieParser.ParameterContext context)
    {
        return base.VisitParameter(context);
    }

    public override SyntaxNode VisitParameter_list([NotNull] DassieParser.Parameter_listContext context)
    {
        return base.VisitParameter_list(context);
    }

    public override SyntaxNode VisitParameter_modifier([NotNull] DassieParser.Parameter_modifierContext context)
    {
        return base.VisitParameter_modifier(context);
    }

    public override SyntaxNode VisitPlaceholder([NotNull] DassieParser.PlaceholderContext context)
    {
        return base.VisitPlaceholder(context);
    }

    public override SyntaxNode VisitPostfix_if_branch([NotNull] DassieParser.Postfix_if_branchContext context)
    {
        return base.VisitPostfix_if_branch(context);
    }

    public override SyntaxNode VisitPostfix_if_expression([NotNull] DassieParser.Postfix_if_expressionContext context)
    {
        return base.VisitPostfix_if_expression(context);
    }

    public override SyntaxNode VisitPostfix_unless_branch([NotNull] DassieParser.Postfix_unless_branchContext context)
    {
        return base.VisitPostfix_unless_branch(context);
    }

    public override SyntaxNode VisitPostfix_unless_expression([NotNull] DassieParser.Postfix_unless_expressionContext context)
    {
        return base.VisitPostfix_unless_expression(context);
    }

    public override SyntaxNode VisitPower_expression([NotNull] DassieParser.Power_expressionContext context)
    {
        return base.VisitPower_expression(context);
    }

    public override SyntaxNode VisitPredicate([NotNull] DassieParser.PredicateContext context)
    {
        return base.VisitPredicate(context);
    }

    public override SyntaxNode VisitPrefix_if_expression([NotNull] DassieParser.Prefix_if_expressionContext context)
    {
        return base.VisitPrefix_if_expression(context);
    }

    public override SyntaxNode VisitPrefix_unless_expression([NotNull] DassieParser.Prefix_unless_expressionContext context)
    {
        return base.VisitPrefix_unless_expression(context);
    }

    public override SyntaxNode VisitProperty_getter([NotNull] DassieParser.Property_getterContext context)
    {
        return base.VisitProperty_getter(context);
    }

    public override SyntaxNode VisitProperty_or_event_block([NotNull] DassieParser.Property_or_event_blockContext context)
    {
        return base.VisitProperty_or_event_block(context);
    }

    public override SyntaxNode VisitProperty_setter([NotNull] DassieParser.Property_setterContext context)
    {
        return base.VisitProperty_setter(context);
    }

    public override SyntaxNode VisitRaise_expression([NotNull] DassieParser.Raise_expressionContext context)
    {
        return base.VisitRaise_expression(context);
    }

    public override SyntaxNode VisitRange_index_expression([NotNull] DassieParser.Range_index_expressionContext context)
    {
        return base.VisitRange_index_expression(context);
    }

    public override SyntaxNode VisitReal_atom([NotNull] DassieParser.Real_atomContext context)
    {
        return base.VisitReal_atom(context);
    }

    public override SyntaxNode VisitRemainder_expression([NotNull] DassieParser.Remainder_expressionContext context)
    {
        return base.VisitRemainder_expression(context);
    }

    public override SyntaxNode VisitRemove_handler([NotNull] DassieParser.Remove_handlerContext context)
    {
        return base.VisitRemove_handler(context);
    }

    public override SyntaxNode VisitRethrow_exception([NotNull] DassieParser.Rethrow_exceptionContext context)
    {
        return base.VisitRethrow_exception(context);
    }

    public override SyntaxNode VisitRight_pipe_expression([NotNull] DassieParser.Right_pipe_expressionContext context)
    {
        return base.VisitRight_pipe_expression(context);
    }

    public override SyntaxNode VisitRight_shift_expression([NotNull] DassieParser.Right_shift_expressionContext context)
    {
        return base.VisitRight_shift_expression(context);
    }

    public override SyntaxNode VisitSafe_conversion_expression([NotNull] DassieParser.Safe_conversion_expressionContext context)
    {
        return base.VisitSafe_conversion_expression(context);
    }

    public override SyntaxNode VisitSeparated_expression([NotNull] DassieParser.Separated_expressionContext context)
    {
        return base.VisitSeparated_expression(context);
    }

    public override SyntaxNode VisitSpecial_symbol([NotNull] DassieParser.Special_symbolContext context)
    {
        return base.VisitSpecial_symbol(context);
    }

    public override SyntaxNode VisitSpecial_symbol_expression([NotNull] DassieParser.Special_symbol_expressionContext context)
    {
        return base.VisitSpecial_symbol_expression(context);
    }

    public override SyntaxNode VisitString_atom([NotNull] DassieParser.String_atomContext context)
    {
        return base.VisitString_atom(context);
    }

    public override SyntaxNode VisitSubtraction_expression([NotNull] DassieParser.Subtraction_expressionContext context)
    {
        return base.VisitSubtraction_expression(context);
    }

    public override SyntaxNode VisitThis_atom([NotNull] DassieParser.This_atomContext context)
    {
        return base.VisitThis_atom(context);
    }

    public override SyntaxNode VisitTry_branch([NotNull] DassieParser.Try_branchContext context)
    {
        return base.VisitTry_branch(context);
    }

    public override SyntaxNode VisitTry_expression([NotNull] DassieParser.Try_expressionContext context)
    {
        return base.VisitTry_expression(context);
    }

    public override SyntaxNode VisitTuple_expression([NotNull] DassieParser.Tuple_expressionContext context)
    {
        return base.VisitTuple_expression(context);
    }

    public override SyntaxNode VisitType([NotNull] DassieParser.TypeContext context)
    {
        return base.VisitType(context);
    }

    public override SyntaxNode VisitType_access_modifier([NotNull] DassieParser.Type_access_modifierContext context)
    {
        return base.VisitType_access_modifier(context);
    }

    public override SyntaxNode VisitType_block([NotNull] DassieParser.Type_blockContext context)
    {
        return base.VisitType_block(context);
    }

    public override SyntaxNode VisitType_kind([NotNull] DassieParser.Type_kindContext context)
    {
        return base.VisitType_kind(context);
    }

    public override SyntaxNode VisitType_member([NotNull] DassieParser.Type_memberContext context)
    {
        return base.VisitType_member(context);
    }

    public override SyntaxNode VisitType_name([NotNull] DassieParser.Type_nameContext context)
    {
        return base.VisitType_name(context);
    }

    public override SyntaxNode VisitType_special_modifier([NotNull] DassieParser.Type_special_modifierContext context)
    {
        return base.VisitType_special_modifier(context);
    }

    public override SyntaxNode VisitUnion_or_tuple_type_member([NotNull] DassieParser.Union_or_tuple_type_memberContext context)
    {
        return base.VisitUnion_or_tuple_type_member(context);
    }

    public override SyntaxNode VisitUnless_branch([NotNull] DassieParser.Unless_branchContext context)
    {
        return base.VisitUnless_branch(context);
    }

    public override SyntaxNode VisitUntil_loop([NotNull] DassieParser.Until_loopContext context)
    {
        return base.VisitUntil_loop(context);
    }

    public override SyntaxNode VisitWhile_loop([NotNull] DassieParser.While_loopContext context)
    {
        return base.VisitWhile_loop(context);
    }

    public override SyntaxNode VisitWildcard_atom([NotNull] DassieParser.Wildcard_atomContext context)
    {
        return base.VisitWildcard_atom(context);
    }

    public override SyntaxNode VisitXor_expression([NotNull] DassieParser.Xor_expressionContext context)
    {
        return base.VisitXor_expression(context);
    }
}