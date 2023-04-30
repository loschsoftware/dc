//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.12.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Users\Jonas\source\repos\lsc\src\LoschScript\Parser\LoschScriptParser.g4 by ANTLR 4.12.0

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
using IErrorNode = Antlr4.Runtime.Tree.IErrorNode;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="ILoschScriptParserListener"/>,
/// which can be extended to create a listener which only needs to handle a subset
/// of the available methods.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.12.0")]
[System.Diagnostics.DebuggerNonUserCode]
[System.CLSCompliant(false)]
public partial class LoschScriptParserBaseListener : ILoschScriptParserListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.compilation_unit"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCompilation_unit([NotNull] LoschScriptParser.Compilation_unitContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.compilation_unit"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCompilation_unit([NotNull] LoschScriptParser.Compilation_unitContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.file_body"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFile_body([NotNull] LoschScriptParser.File_bodyContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.file_body"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFile_body([NotNull] LoschScriptParser.File_bodyContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.top_level_statements"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterTop_level_statements([NotNull] LoschScriptParser.Top_level_statementsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.top_level_statements"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitTop_level_statements([NotNull] LoschScriptParser.Top_level_statementsContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.full_program"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFull_program([NotNull] LoschScriptParser.Full_programContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.full_program"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFull_program([NotNull] LoschScriptParser.Full_programContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>basic_import</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBasic_import([NotNull] LoschScriptParser.Basic_importContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>basic_import</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBasic_import([NotNull] LoschScriptParser.Basic_importContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>type_import</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterType_import([NotNull] LoschScriptParser.Type_importContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>type_import</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitType_import([NotNull] LoschScriptParser.Type_importContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>alias</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAlias([NotNull] LoschScriptParser.AliasContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>alias</c>
	/// labeled alternative in <see cref="LoschScriptParser.import_directive"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAlias([NotNull] LoschScriptParser.AliasContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.export_directive"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExport_directive([NotNull] LoschScriptParser.Export_directiveContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.export_directive"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExport_directive([NotNull] LoschScriptParser.Export_directiveContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.full_identifier"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFull_identifier([NotNull] LoschScriptParser.Full_identifierContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.full_identifier"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFull_identifier([NotNull] LoschScriptParser.Full_identifierContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>attributed_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAttributed_expression([NotNull] LoschScriptParser.Attributed_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>attributed_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAttributed_expression([NotNull] LoschScriptParser.Attributed_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>equality_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterEquality_expression([NotNull] LoschScriptParser.Equality_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>equality_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitEquality_expression([NotNull] LoschScriptParser.Equality_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>subtraction_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterSubtraction_expression([NotNull] LoschScriptParser.Subtraction_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>subtraction_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitSubtraction_expression([NotNull] LoschScriptParser.Subtraction_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>implementation_query_exception</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterImplementation_query_exception([NotNull] LoschScriptParser.Implementation_query_exceptionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>implementation_query_exception</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitImplementation_query_exception([NotNull] LoschScriptParser.Implementation_query_exceptionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>bitwise_complement_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBitwise_complement_expression([NotNull] LoschScriptParser.Bitwise_complement_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>bitwise_complement_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBitwise_complement_expression([NotNull] LoschScriptParser.Bitwise_complement_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>xor_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterXor_expression([NotNull] LoschScriptParser.Xor_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>xor_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitXor_expression([NotNull] LoschScriptParser.Xor_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>logical_negation_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLogical_negation_expression([NotNull] LoschScriptParser.Logical_negation_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>logical_negation_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLogical_negation_expression([NotNull] LoschScriptParser.Logical_negation_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>right_shift_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterRight_shift_expression([NotNull] LoschScriptParser.Right_shift_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>right_shift_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitRight_shift_expression([NotNull] LoschScriptParser.Right_shift_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>multiply_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterMultiply_expression([NotNull] LoschScriptParser.Multiply_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>multiply_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitMultiply_expression([NotNull] LoschScriptParser.Multiply_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>logical_or_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLogical_or_expression([NotNull] LoschScriptParser.Logical_or_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>logical_or_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLogical_or_expression([NotNull] LoschScriptParser.Logical_or_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>range_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterRange_expression([NotNull] LoschScriptParser.Range_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>range_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitRange_expression([NotNull] LoschScriptParser.Range_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>unary_negation_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterUnary_negation_expression([NotNull] LoschScriptParser.Unary_negation_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>unary_negation_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitUnary_negation_expression([NotNull] LoschScriptParser.Unary_negation_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>member_access_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterMember_access_expression([NotNull] LoschScriptParser.Member_access_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>member_access_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitMember_access_expression([NotNull] LoschScriptParser.Member_access_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>remainder_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterRemainder_expression([NotNull] LoschScriptParser.Remainder_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>remainder_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitRemainder_expression([NotNull] LoschScriptParser.Remainder_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>typeof_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterTypeof_expression([NotNull] LoschScriptParser.Typeof_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>typeof_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitTypeof_expression([NotNull] LoschScriptParser.Typeof_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>power_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPower_expression([NotNull] LoschScriptParser.Power_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>power_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPower_expression([NotNull] LoschScriptParser.Power_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>assignment</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAssignment([NotNull] LoschScriptParser.AssignmentContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>assignment</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAssignment([NotNull] LoschScriptParser.AssignmentContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>addition_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAddition_expression([NotNull] LoschScriptParser.Addition_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>addition_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAddition_expression([NotNull] LoschScriptParser.Addition_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>atom_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAtom_expression([NotNull] LoschScriptParser.Atom_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>atom_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAtom_expression([NotNull] LoschScriptParser.Atom_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>comparison_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterComparison_expression([NotNull] LoschScriptParser.Comparison_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>comparison_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitComparison_expression([NotNull] LoschScriptParser.Comparison_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>unary_plus_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterUnary_plus_expression([NotNull] LoschScriptParser.Unary_plus_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>unary_plus_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitUnary_plus_expression([NotNull] LoschScriptParser.Unary_plus_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>or_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOr_expression([NotNull] LoschScriptParser.Or_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>or_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOr_expression([NotNull] LoschScriptParser.Or_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>nameof_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterNameof_expression([NotNull] LoschScriptParser.Nameof_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>nameof_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitNameof_expression([NotNull] LoschScriptParser.Nameof_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>divide_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDivide_expression([NotNull] LoschScriptParser.Divide_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>divide_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDivide_expression([NotNull] LoschScriptParser.Divide_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>logical_and_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLogical_and_expression([NotNull] LoschScriptParser.Logical_and_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>logical_and_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLogical_and_expression([NotNull] LoschScriptParser.Logical_and_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>dotted_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDotted_expression([NotNull] LoschScriptParser.Dotted_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>dotted_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDotted_expression([NotNull] LoschScriptParser.Dotted_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>left_shift_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLeft_shift_expression([NotNull] LoschScriptParser.Left_shift_expressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>left_shift_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLeft_shift_expression([NotNull] LoschScriptParser.Left_shift_expressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAtom([NotNull] LoschScriptParser.AtomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAtom([NotNull] LoschScriptParser.AtomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.expression_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpression_atom([NotNull] LoschScriptParser.Expression_atomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.expression_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpression_atom([NotNull] LoschScriptParser.Expression_atomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.integer_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterInteger_atom([NotNull] LoschScriptParser.Integer_atomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.integer_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitInteger_atom([NotNull] LoschScriptParser.Integer_atomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.real_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterReal_atom([NotNull] LoschScriptParser.Real_atomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.real_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitReal_atom([NotNull] LoschScriptParser.Real_atomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.boolean_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBoolean_atom([NotNull] LoschScriptParser.Boolean_atomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.boolean_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBoolean_atom([NotNull] LoschScriptParser.Boolean_atomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.string_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterString_atom([NotNull] LoschScriptParser.String_atomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.string_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitString_atom([NotNull] LoschScriptParser.String_atomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.character_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCharacter_atom([NotNull] LoschScriptParser.Character_atomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.character_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCharacter_atom([NotNull] LoschScriptParser.Character_atomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.empty_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterEmpty_atom([NotNull] LoschScriptParser.Empty_atomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.empty_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitEmpty_atom([NotNull] LoschScriptParser.Empty_atomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.wildcard_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterWildcard_atom([NotNull] LoschScriptParser.Wildcard_atomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.wildcard_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitWildcard_atom([NotNull] LoschScriptParser.Wildcard_atomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.identifier_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterIdentifier_atom([NotNull] LoschScriptParser.Identifier_atomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.identifier_atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitIdentifier_atom([NotNull] LoschScriptParser.Identifier_atomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.assignment_operator"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAssignment_operator([NotNull] LoschScriptParser.Assignment_operatorContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.assignment_operator"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAssignment_operator([NotNull] LoschScriptParser.Assignment_operatorContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.range"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterRange([NotNull] LoschScriptParser.RangeContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.range"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitRange([NotNull] LoschScriptParser.RangeContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.member_access"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterMember_access([NotNull] LoschScriptParser.Member_accessContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.member_access"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitMember_access([NotNull] LoschScriptParser.Member_accessContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.arglist"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterArglist([NotNull] LoschScriptParser.ArglistContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.arglist"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitArglist([NotNull] LoschScriptParser.ArglistContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.attribute"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAttribute([NotNull] LoschScriptParser.AttributeContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.attribute"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAttribute([NotNull] LoschScriptParser.AttributeContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="LoschScriptParser.type_definition"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterType_definition([NotNull] LoschScriptParser.Type_definitionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="LoschScriptParser.type_definition"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitType_definition([NotNull] LoschScriptParser.Type_definitionContext context) { }

	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void EnterEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void ExitEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitTerminal([NotNull] ITerminalNode node) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitErrorNode([NotNull] IErrorNode node) { }
}
} // namespace LoschScript.Parser
