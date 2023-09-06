//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.12.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Users\Jonas\source\repos\loschsoftware\lsc\src\LoschScript\Parser\LoschScriptParser.g4 by ANTLR 4.12.0

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace LoschScript.Parser {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="LoschScriptParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.12.0")]
[System.CLSCompliant(false)]
public interface ILoschScriptParserVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.compilation_unit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCompilation_unit([NotNull] LoschScriptParser.Compilation_unitContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.file_body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFile_body([NotNull] LoschScriptParser.File_bodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.top_level_statements"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTop_level_statements([NotNull] LoschScriptParser.Top_level_statementsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.full_program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFull_program([NotNull] LoschScriptParser.Full_programContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>basic_import</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBasic_import([NotNull] LoschScriptParser.Basic_importContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>type_import</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_import([NotNull] LoschScriptParser.Type_importContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>alias</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAlias([NotNull] LoschScriptParser.AliasContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.export_directive"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExport_directive([NotNull] LoschScriptParser.Export_directiveContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.full_identifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFull_identifier([NotNull] LoschScriptParser.Full_identifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.code_block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCode_block([NotNull] LoschScriptParser.Code_blockContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>attributed_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAttributed_expression([NotNull] LoschScriptParser.Attributed_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>equality_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEquality_expression([NotNull] LoschScriptParser.Equality_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>subtraction_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSubtraction_expression([NotNull] LoschScriptParser.Subtraction_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>bitwise_complement_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBitwise_complement_expression([NotNull] LoschScriptParser.Bitwise_complement_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>until_loop</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUntil_loop([NotNull] LoschScriptParser.Until_loopContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>right_pipe_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRight_pipe_expression([NotNull] LoschScriptParser.Right_pipe_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>while_loop</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWhile_loop([NotNull] LoschScriptParser.While_loopContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>xor_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitXor_expression([NotNull] LoschScriptParser.Xor_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>prefix_if_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrefix_if_expression([NotNull] LoschScriptParser.Prefix_if_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>logical_negation_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogical_negation_expression([NotNull] LoschScriptParser.Logical_negation_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>right_shift_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRight_shift_expression([NotNull] LoschScriptParser.Right_shift_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>array_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArray_expression([NotNull] LoschScriptParser.Array_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>multiply_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMultiply_expression([NotNull] LoschScriptParser.Multiply_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>loop_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLoop_expression([NotNull] LoschScriptParser.Loop_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>logical_or_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogical_or_expression([NotNull] LoschScriptParser.Logical_or_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>block_postfix_unless_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlock_postfix_unless_expression([NotNull] LoschScriptParser.Block_postfix_unless_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>and_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAnd_expression([NotNull] LoschScriptParser.And_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>range_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRange_expression([NotNull] LoschScriptParser.Range_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>unary_negation_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnary_negation_expression([NotNull] LoschScriptParser.Unary_negation_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>member_access_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMember_access_expression([NotNull] LoschScriptParser.Member_access_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>newlined_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNewlined_expression([NotNull] LoschScriptParser.Newlined_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>remainder_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRemainder_expression([NotNull] LoschScriptParser.Remainder_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>identifier_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIdentifier_expression([NotNull] LoschScriptParser.Identifier_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>typeof_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeof_expression([NotNull] LoschScriptParser.Typeof_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>block_postfix_if_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlock_postfix_if_expression([NotNull] LoschScriptParser.Block_postfix_if_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>block_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlock_expression([NotNull] LoschScriptParser.Block_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>postfix_unless_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPostfix_unless_expression([NotNull] LoschScriptParser.Postfix_unless_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>power_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPower_expression([NotNull] LoschScriptParser.Power_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>left_pipe_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLeft_pipe_expression([NotNull] LoschScriptParser.Left_pipe_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>dictionary_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDictionary_expression([NotNull] LoschScriptParser.Dictionary_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>prefix_unless_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrefix_unless_expression([NotNull] LoschScriptParser.Prefix_unless_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>addition_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAddition_expression([NotNull] LoschScriptParser.Addition_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>atom_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAtom_expression([NotNull] LoschScriptParser.Atom_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>index_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIndex_expression([NotNull] LoschScriptParser.Index_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>comparison_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitComparison_expression([NotNull] LoschScriptParser.Comparison_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>full_identifier_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFull_identifier_expression([NotNull] LoschScriptParser.Full_identifier_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>local_declaration_or_assignment</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLocal_declaration_or_assignment([NotNull] LoschScriptParser.Local_declaration_or_assignmentContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>tuple_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTuple_expression([NotNull] LoschScriptParser.Tuple_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>or_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOr_expression([NotNull] LoschScriptParser.Or_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>nameof_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNameof_expression([NotNull] LoschScriptParser.Nameof_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>postfix_if_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPostfix_if_expression([NotNull] LoschScriptParser.Postfix_if_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>divide_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDivide_expression([NotNull] LoschScriptParser.Divide_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>array_element_assignment</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArray_element_assignment([NotNull] LoschScriptParser.Array_element_assignmentContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>logical_and_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogical_and_expression([NotNull] LoschScriptParser.Logical_and_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>dotted_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDotted_expression([NotNull] LoschScriptParser.Dotted_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>left_shift_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLeft_shift_expression([NotNull] LoschScriptParser.Left_shift_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>full_identifier_member_access_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFull_identifier_member_access_expression([NotNull] LoschScriptParser.Full_identifier_member_access_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>implementation_query_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitImplementation_query_expression([NotNull] LoschScriptParser.Implementation_query_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAtom([NotNull] LoschScriptParser.AtomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.this_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitThis_atom([NotNull] LoschScriptParser.This_atomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.expression_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpression_atom([NotNull] LoschScriptParser.Expression_atomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.integer_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInteger_atom([NotNull] LoschScriptParser.Integer_atomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.real_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReal_atom([NotNull] LoschScriptParser.Real_atomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.boolean_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBoolean_atom([NotNull] LoschScriptParser.Boolean_atomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.string_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitString_atom([NotNull] LoschScriptParser.String_atomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.character_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCharacter_atom([NotNull] LoschScriptParser.Character_atomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.empty_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEmpty_atom([NotNull] LoschScriptParser.Empty_atomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.wildcard_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWildcard_atom([NotNull] LoschScriptParser.Wildcard_atomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.identifier_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIdentifier_atom([NotNull] LoschScriptParser.Identifier_atomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.type_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_name([NotNull] LoschScriptParser.Type_nameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.builtin_type_alias"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBuiltin_type_alias([NotNull] LoschScriptParser.Builtin_type_aliasContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.param_list_type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParam_list_type([NotNull] LoschScriptParser.Param_list_typeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.if_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIf_branch([NotNull] LoschScriptParser.If_branchContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.postfix_if_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPostfix_if_branch([NotNull] LoschScriptParser.Postfix_if_branchContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.elif_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitElif_branch([NotNull] LoschScriptParser.Elif_branchContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.else_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitElse_branch([NotNull] LoschScriptParser.Else_branchContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.unless_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnless_branch([NotNull] LoschScriptParser.Unless_branchContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.else_unless_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitElse_unless_branch([NotNull] LoschScriptParser.Else_unless_branchContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.postfix_unless_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPostfix_unless_branch([NotNull] LoschScriptParser.Postfix_unless_branchContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.range"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRange([NotNull] LoschScriptParser.RangeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.index"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIndex([NotNull] LoschScriptParser.IndexContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.arglist"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArglist([NotNull] LoschScriptParser.ArglistContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.attribute"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAttribute([NotNull] LoschScriptParser.AttributeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.generic_identifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGeneric_identifier([NotNull] LoschScriptParser.Generic_identifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.field_access_modifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitField_access_modifier([NotNull] LoschScriptParser.Field_access_modifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.field_declaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitField_declaration([NotNull] LoschScriptParser.Field_declarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.placeholder"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPlaceholder([NotNull] LoschScriptParser.PlaceholderContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.type_access_modifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_access_modifier([NotNull] LoschScriptParser.Type_access_modifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.nested_type_access_modifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNested_type_access_modifier([NotNull] LoschScriptParser.Nested_type_access_modifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.type_special_modifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_special_modifier([NotNull] LoschScriptParser.Type_special_modifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType([NotNull] LoschScriptParser.TypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.type_parameter_list"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_parameter_list([NotNull] LoschScriptParser.Type_parameter_listContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.type_parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_parameter([NotNull] LoschScriptParser.Type_parameterContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.type_parameter_constraint"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_parameter_constraint([NotNull] LoschScriptParser.Type_parameter_constraintContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.inheritance_list"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInheritance_list([NotNull] LoschScriptParser.Inheritance_listContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.type_kind"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_kind([NotNull] LoschScriptParser.Type_kindContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.member_access_modifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMember_access_modifier([NotNull] LoschScriptParser.Member_access_modifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.member_oop_modifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMember_oop_modifier([NotNull] LoschScriptParser.Member_oop_modifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.member_special_modifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMember_special_modifier([NotNull] LoschScriptParser.Member_special_modifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.type_member"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_member([NotNull] LoschScriptParser.Type_memberContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.parameter_list"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameter_list([NotNull] LoschScriptParser.Parameter_listContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.parameter_modifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameter_modifier([NotNull] LoschScriptParser.Parameter_modifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameter([NotNull] LoschScriptParser.ParameterContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.parameter_constraint"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameter_constraint([NotNull] LoschScriptParser.Parameter_constraintContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.type_block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_block([NotNull] LoschScriptParser.Type_blockContext context);
}
} // namespace LoschScript.Parser
