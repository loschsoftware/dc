//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.12.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Users\Jonas\Source\Repos\lsc\src\LoschScript\Parser\LoschScriptParser.g4 by ANTLR 4.12.0

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
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="LoschScriptParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.12.0")]
[System.CLSCompliant(false)]
public interface ILoschScriptParserListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.compilation_unit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCompilation_unit([NotNull] LoschScriptParser.Compilation_unitContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.compilation_unit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCompilation_unit([NotNull] LoschScriptParser.Compilation_unitContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.file_body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFile_body([NotNull] LoschScriptParser.File_bodyContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.file_body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFile_body([NotNull] LoschScriptParser.File_bodyContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.top_level_statements"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTop_level_statements([NotNull] LoschScriptParser.Top_level_statementsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.top_level_statements"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTop_level_statements([NotNull] LoschScriptParser.Top_level_statementsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.full_program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFull_program([NotNull] LoschScriptParser.Full_programContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.full_program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFull_program([NotNull] LoschScriptParser.Full_programContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>basic_import</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBasic_import([NotNull] LoschScriptParser.Basic_importContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>basic_import</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBasic_import([NotNull] LoschScriptParser.Basic_importContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>type_import</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterType_import([NotNull] LoschScriptParser.Type_importContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>type_import</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitType_import([NotNull] LoschScriptParser.Type_importContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>alias</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAlias([NotNull] LoschScriptParser.AliasContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>alias</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAlias([NotNull] LoschScriptParser.AliasContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.export_directive"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExport_directive([NotNull] LoschScriptParser.Export_directiveContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.export_directive"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExport_directive([NotNull] LoschScriptParser.Export_directiveContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.full_identifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFull_identifier([NotNull] LoschScriptParser.Full_identifierContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.full_identifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFull_identifier([NotNull] LoschScriptParser.Full_identifierContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.code_block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCode_block([NotNull] LoschScriptParser.Code_blockContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.code_block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCode_block([NotNull] LoschScriptParser.Code_blockContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>attributed_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAttributed_expression([NotNull] LoschScriptParser.Attributed_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>attributed_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAttributed_expression([NotNull] LoschScriptParser.Attributed_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>equality_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEquality_expression([NotNull] LoschScriptParser.Equality_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>equality_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEquality_expression([NotNull] LoschScriptParser.Equality_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>subtraction_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSubtraction_expression([NotNull] LoschScriptParser.Subtraction_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>subtraction_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSubtraction_expression([NotNull] LoschScriptParser.Subtraction_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>implementation_query_exception</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterImplementation_query_exception([NotNull] LoschScriptParser.Implementation_query_exceptionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>implementation_query_exception</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitImplementation_query_exception([NotNull] LoschScriptParser.Implementation_query_exceptionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>bitwise_complement_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBitwise_complement_expression([NotNull] LoschScriptParser.Bitwise_complement_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>bitwise_complement_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBitwise_complement_expression([NotNull] LoschScriptParser.Bitwise_complement_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>xor_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterXor_expression([NotNull] LoschScriptParser.Xor_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>xor_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitXor_expression([NotNull] LoschScriptParser.Xor_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>prefix_if_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPrefix_if_expression([NotNull] LoschScriptParser.Prefix_if_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>prefix_if_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPrefix_if_expression([NotNull] LoschScriptParser.Prefix_if_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>logical_negation_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLogical_negation_expression([NotNull] LoschScriptParser.Logical_negation_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>logical_negation_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLogical_negation_expression([NotNull] LoschScriptParser.Logical_negation_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>right_shift_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRight_shift_expression([NotNull] LoschScriptParser.Right_shift_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>right_shift_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRight_shift_expression([NotNull] LoschScriptParser.Right_shift_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>multiply_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMultiply_expression([NotNull] LoschScriptParser.Multiply_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>multiply_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMultiply_expression([NotNull] LoschScriptParser.Multiply_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>logical_or_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLogical_or_expression([NotNull] LoschScriptParser.Logical_or_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>logical_or_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLogical_or_expression([NotNull] LoschScriptParser.Logical_or_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>block_postfix_unless_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBlock_postfix_unless_expression([NotNull] LoschScriptParser.Block_postfix_unless_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>block_postfix_unless_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBlock_postfix_unless_expression([NotNull] LoschScriptParser.Block_postfix_unless_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>range_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRange_expression([NotNull] LoschScriptParser.Range_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>range_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRange_expression([NotNull] LoschScriptParser.Range_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>unary_negation_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterUnary_negation_expression([NotNull] LoschScriptParser.Unary_negation_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>unary_negation_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitUnary_negation_expression([NotNull] LoschScriptParser.Unary_negation_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>member_access_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMember_access_expression([NotNull] LoschScriptParser.Member_access_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>member_access_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMember_access_expression([NotNull] LoschScriptParser.Member_access_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>remainder_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRemainder_expression([NotNull] LoschScriptParser.Remainder_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>remainder_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRemainder_expression([NotNull] LoschScriptParser.Remainder_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>typeof_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTypeof_expression([NotNull] LoschScriptParser.Typeof_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>typeof_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTypeof_expression([NotNull] LoschScriptParser.Typeof_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>postfix_unless_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPostfix_unless_expression([NotNull] LoschScriptParser.Postfix_unless_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>postfix_unless_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPostfix_unless_expression([NotNull] LoschScriptParser.Postfix_unless_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>power_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPower_expression([NotNull] LoschScriptParser.Power_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>power_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPower_expression([NotNull] LoschScriptParser.Power_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>assignment</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAssignment([NotNull] LoschScriptParser.AssignmentContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>assignment</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAssignment([NotNull] LoschScriptParser.AssignmentContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>prefix_unless_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPrefix_unless_expression([NotNull] LoschScriptParser.Prefix_unless_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>prefix_unless_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPrefix_unless_expression([NotNull] LoschScriptParser.Prefix_unless_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>addition_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAddition_expression([NotNull] LoschScriptParser.Addition_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>addition_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAddition_expression([NotNull] LoschScriptParser.Addition_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>atom_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAtom_expression([NotNull] LoschScriptParser.Atom_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>atom_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAtom_expression([NotNull] LoschScriptParser.Atom_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>comparison_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterComparison_expression([NotNull] LoschScriptParser.Comparison_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>comparison_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitComparison_expression([NotNull] LoschScriptParser.Comparison_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>unary_plus_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterUnary_plus_expression([NotNull] LoschScriptParser.Unary_plus_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>unary_plus_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitUnary_plus_expression([NotNull] LoschScriptParser.Unary_plus_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>or_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterOr_expression([NotNull] LoschScriptParser.Or_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>or_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitOr_expression([NotNull] LoschScriptParser.Or_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>nameof_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNameof_expression([NotNull] LoschScriptParser.Nameof_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>nameof_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNameof_expression([NotNull] LoschScriptParser.Nameof_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>postfix_if_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPostfix_if_expression([NotNull] LoschScriptParser.Postfix_if_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>postfix_if_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPostfix_if_expression([NotNull] LoschScriptParser.Postfix_if_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>divide_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDivide_expression([NotNull] LoschScriptParser.Divide_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>divide_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDivide_expression([NotNull] LoschScriptParser.Divide_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>logical_and_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLogical_and_expression([NotNull] LoschScriptParser.Logical_and_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>logical_and_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLogical_and_expression([NotNull] LoschScriptParser.Logical_and_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>dotted_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDotted_expression([NotNull] LoschScriptParser.Dotted_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>dotted_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDotted_expression([NotNull] LoschScriptParser.Dotted_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>left_shift_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLeft_shift_expression([NotNull] LoschScriptParser.Left_shift_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>left_shift_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLeft_shift_expression([NotNull] LoschScriptParser.Left_shift_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAtom([NotNull] LoschScriptParser.AtomContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAtom([NotNull] LoschScriptParser.AtomContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.expression_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpression_atom([NotNull] LoschScriptParser.Expression_atomContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.expression_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpression_atom([NotNull] LoschScriptParser.Expression_atomContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.integer_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterInteger_atom([NotNull] LoschScriptParser.Integer_atomContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.integer_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitInteger_atom([NotNull] LoschScriptParser.Integer_atomContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.real_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterReal_atom([NotNull] LoschScriptParser.Real_atomContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.real_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitReal_atom([NotNull] LoschScriptParser.Real_atomContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.boolean_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBoolean_atom([NotNull] LoschScriptParser.Boolean_atomContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.boolean_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBoolean_atom([NotNull] LoschScriptParser.Boolean_atomContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.string_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterString_atom([NotNull] LoschScriptParser.String_atomContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.string_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitString_atom([NotNull] LoschScriptParser.String_atomContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.character_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCharacter_atom([NotNull] LoschScriptParser.Character_atomContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.character_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCharacter_atom([NotNull] LoschScriptParser.Character_atomContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.empty_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEmpty_atom([NotNull] LoschScriptParser.Empty_atomContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.empty_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEmpty_atom([NotNull] LoschScriptParser.Empty_atomContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.wildcard_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterWildcard_atom([NotNull] LoschScriptParser.Wildcard_atomContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.wildcard_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitWildcard_atom([NotNull] LoschScriptParser.Wildcard_atomContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.identifier_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIdentifier_atom([NotNull] LoschScriptParser.Identifier_atomContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.identifier_atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIdentifier_atom([NotNull] LoschScriptParser.Identifier_atomContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.assignment_operator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAssignment_operator([NotNull] LoschScriptParser.Assignment_operatorContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.assignment_operator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAssignment_operator([NotNull] LoschScriptParser.Assignment_operatorContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.if_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIf_branch([NotNull] LoschScriptParser.If_branchContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.if_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIf_branch([NotNull] LoschScriptParser.If_branchContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.postfix_if_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPostfix_if_branch([NotNull] LoschScriptParser.Postfix_if_branchContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.postfix_if_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPostfix_if_branch([NotNull] LoschScriptParser.Postfix_if_branchContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.elif_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterElif_branch([NotNull] LoschScriptParser.Elif_branchContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.elif_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitElif_branch([NotNull] LoschScriptParser.Elif_branchContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.else_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterElse_branch([NotNull] LoschScriptParser.Else_branchContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.else_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitElse_branch([NotNull] LoschScriptParser.Else_branchContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.unless_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterUnless_branch([NotNull] LoschScriptParser.Unless_branchContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.unless_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitUnless_branch([NotNull] LoschScriptParser.Unless_branchContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.else_unless_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterElse_unless_branch([NotNull] LoschScriptParser.Else_unless_branchContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.else_unless_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitElse_unless_branch([NotNull] LoschScriptParser.Else_unless_branchContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.postfix_unless_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPostfix_unless_branch([NotNull] LoschScriptParser.Postfix_unless_branchContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.postfix_unless_branch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPostfix_unless_branch([NotNull] LoschScriptParser.Postfix_unless_branchContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.range"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRange([NotNull] LoschScriptParser.RangeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.range"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRange([NotNull] LoschScriptParser.RangeContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.arglist"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterArglist([NotNull] LoschScriptParser.ArglistContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.arglist"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitArglist([NotNull] LoschScriptParser.ArglistContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.attribute"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAttribute([NotNull] LoschScriptParser.AttributeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.attribute"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAttribute([NotNull] LoschScriptParser.AttributeContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.type_definition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterType_definition([NotNull] LoschScriptParser.Type_definitionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.type_definition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitType_definition([NotNull] LoschScriptParser.Type_definitionContext context);
}
} // namespace LoschScript.Parser
