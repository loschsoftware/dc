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
	/// Visit a parse tree produced by the <c>implementation_query_exception</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitImplementation_query_exception([NotNull] LoschScriptParser.Implementation_query_exceptionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>bitwise_complement_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBitwise_complement_expression([NotNull] LoschScriptParser.Bitwise_complement_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>xor_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitXor_expression([NotNull] LoschScriptParser.Xor_expressionContext context);
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
	/// Visit a parse tree produced by the <c>multiply_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMultiply_expression([NotNull] LoschScriptParser.Multiply_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>logical_or_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogical_or_expression([NotNull] LoschScriptParser.Logical_or_expressionContext context);
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
	/// Visit a parse tree produced by the <c>remainder_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRemainder_expression([NotNull] LoschScriptParser.Remainder_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>typeof_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeof_expression([NotNull] LoschScriptParser.Typeof_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>power_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPower_expression([NotNull] LoschScriptParser.Power_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>assignment</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignment([NotNull] LoschScriptParser.AssignmentContext context);
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
	/// Visit a parse tree produced by the <c>comparison_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitComparison_expression([NotNull] LoschScriptParser.Comparison_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>unary_plus_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnary_plus_expression([NotNull] LoschScriptParser.Unary_plus_expressionContext context);
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
	/// Visit a parse tree produced by the <c>divide_expression</c>
	/// labeled alternative in <see cref="LoschScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDivide_expression([NotNull] LoschScriptParser.Divide_expressionContext context);
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
	/// Visit a parse tree produced by <see cref="LoschScriptParser.atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAtom([NotNull] LoschScriptParser.AtomContext context);
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
	/// Visit a parse tree produced by <see cref="LoschScriptParser.assignment_operator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignment_operator([NotNull] LoschScriptParser.Assignment_operatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.range"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRange([NotNull] LoschScriptParser.RangeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LoschScriptParser.member_access"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMember_access([NotNull] LoschScriptParser.Member_accessContext context);
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
	/// Visit a parse tree produced by <see cref="LoschScriptParser.type_definition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType_definition([NotNull] LoschScriptParser.Type_definitionContext context);
}
} // namespace LoschScript.Parser
