using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Dassie.Messages;
using Dassie.Parser;
using System.Collections.Generic;
using System.Linq;

namespace Dassie.Syntax;

internal class SyntaxTreeGenerator(DiagnosticManager dm) : DassieParserBaseVisitor<SyntaxNode>
{
    private static SyntaxToken ToSyntaxToken(IToken token) => new()
    {
        Text = token.Text,
        Value = token.Text,
        Span = TextSpan.FromBounds(token.StartIndex, token.StopIndex)
    };

    private static SyntaxToken Token(SyntaxKind kind, ITerminalNode terminal)
    {
        if (terminal == null)
            return null;

        return Token(kind, terminal.GetText());
    }

    private static SyntaxToken Token(SyntaxKind kind, string text) => Token(kind, text, text);
    private static SyntaxToken Token<T>(SyntaxKind kind, string text, T value) => new()
    {
        TokenKind = kind,
        Text = text,
        Value = value
    };

    private static SyntaxToken Identifier(ITerminalNode node)
    {
        return new SyntaxToken()
        {
            TokenKind = SyntaxKind.IdentifierToken,
            Text = node?.GetText(),
            Value = node.GetIdentifier()
        };
    }

    private static TextSpan GetSpan(ParserRuleContext rule)
    {
        return TextSpan.FromBounds(rule.Start.StartIndex, rule.Stop.StopIndex);
    }

    private static TextSpan GetSpan(ITerminalNode node)
    {
        return TextSpan.FromBounds(node.Symbol.StartIndex, node.Symbol.StopIndex);
    }

    private static TextSpan GetSpan(IToken start, IToken end)
    {
        return TextSpan.FromBounds(start.StartIndex, end.StopIndex);
    }

    private SyntaxNode VisitOrNull(IParseTree tree)
    {
        if (tree == null)
            return null;

        return Visit(tree);
    }

    private static ModifierListSyntax GetModifierList(IEnumerable<IParseTree> modifiers)
    {
        return new()
        {
            Modifiers = modifiers.Select(m => new SyntaxToken()
            {
                Text = m.GetText(),
                Value = m.GetText()
            }).ToList()
        };
    }

    public override SyntaxNode VisitAccess_modifier_member_group([NotNull] DassieParser.Access_modifier_member_groupContext context)
    {
        List<SyntaxNode> children = [];

        foreach (IParseTree child in context.type_member())
            children.Add(Visit(child));

        return new AccessModifierMemberGroupSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OpenBraceToken = Token(SyntaxKind.OpenBraceToken, context.Open_Brace()),
            CloseBraceToken = Token(SyntaxKind.CloseBraceToken, context.Close_Brace()),
            EqualsToken = Token(SyntaxKind.EqualsToken, context.Equals()),
            Modifiers = GetModifierList([context.member_access_modifier(), context.member_oop_modifier(), .. context.member_special_modifier()]),
            Members = children
        };
    }

    public override SyntaxNode VisitAddition_expression([NotNull] DassieParser.Addition_expressionContext context)
    {
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.PlusToken, context.Plus()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitAdd_handler([NotNull] DassieParser.Add_handlerContext context)
    {
        return new AccessorDeclarationSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            Keyword = Token(SyntaxKind.AddHandlerKeyword, context.Add_Handler()),
            EqualsToken = Token(SyntaxKind.EqualsToken, context.Equals()),
            Body = (ExpressionSyntax)Visit(context.expression())
        };
    }

    public override SyntaxNode VisitAnd_expression([NotNull] DassieParser.And_expressionContext context)
    {
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.AmpersandToken, context.Ampersand()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitAnonymous_function_expression([NotNull] DassieParser.Anonymous_function_expressionContext context)
    {
        return new LambdaExpressionSyntax()
        {

        };
    }

    public override SyntaxNode VisitArglist([NotNull] DassieParser.ArglistContext context)
    {
        return new ArgumentListSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            Arguments = new()
            {
                Separators = context.Comma().Select(c => Token(SyntaxKind.CommaToken, c)).ToList(),
                Nodes = context.expression().Select(e => new ArgumentSyntax()
                {
                    FirstToken = ToSyntaxToken(e.Start),
                    LastToken = ToSyntaxToken(e.Stop),
                    Span = GetSpan(e),
                    Name = null, // TODO: Match name with expression correctly, if that is even possible...
                    ColonToken = null,
                    Expression = (ExpressionSyntax)Visit(e)
                }).ToList()
            },
            DoubleCommaToken = Token(SyntaxKind.DoubleCommaToken, context.Double_Comma())
        };
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
        return new ImportDirectiveSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            BangToken = Token(SyntaxKind.ExclamationMarkToken, context.Exclamation_Mark()),
            ImportKeyword = Token(SyntaxKind.ImportKeyword, context.Import()),
            Names =  new()
            {
                Nodes = context.full_identifier().Select(f => (NameSyntax)Visit(f)).ToList(),
                Separators = context.Comma().Select(c => Token(SyntaxKind.CommaToken, c)).ToList()
            }
        };
    }

    public override SyntaxNode VisitBitwise_complement_expression([NotNull] DassieParser.Bitwise_complement_expressionContext context)
    {
        return new UnaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.TildeToken, context.Tilde()),
            Operand = (ExpressionSyntax)Visit(context.expression())
        };
    }

    public override SyntaxNode VisitBlock_expression([NotNull] DassieParser.Block_expressionContext context)
    {
        return Visit(context.code_block());
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
        return new RangeExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            DoubleDotToken = Token(SyntaxKind.DoubleDotToken, context.Double_Dot()),
            Start = null,
            End = (ExpressionSyntax)Visit(context.expression())
        };
    }

    public override SyntaxNode VisitCode_block([NotNull] DassieParser.Code_blockContext context)
    {
        return new BlockExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OpenBraceToken = Token(SyntaxKind.OpenBraceToken, context.Open_Brace()),
            CloseBraceToken = Token(SyntaxKind.CloseBraceToken, context.Close_Brace()),
            Placeholder = (PlaceholderExpressionSyntax)Visit(context.placeholder()),
            Expressions = context.expression().Select(Visit).Cast<ExpressionSyntax>().ToList()
        };
    }

    public override SyntaxNode VisitComparison_expression([NotNull] DassieParser.Comparison_expressionContext context)
    {
        SyntaxKind tokenKind = context.op.Text switch
        {
            "<" => SyntaxKind.LessThanToken,
            "<=" => SyntaxKind.LessEqualsToken,
            ">" => SyntaxKind.GreaterThanToken,
            _ => SyntaxKind.GreaterEqualsToken
        };

        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1]),
            OperatorToken = Token(tokenKind, context.op.Text)
        };
    }

    public override SyntaxNode VisitCompilation_unit([NotNull] DassieParser.Compilation_unitContext context)
    {
        return new CompilationUnitSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            Directives = context.import_directive().Select(Visit).Cast<DirectiveSyntax>().ToList(),
            Body = (FileBodySyntax)Visit(context.file_body()),
            EndOfFileToken = Token(SyntaxKind.EndOfFileToken, context.Eof())
        };
    }

    public override SyntaxNode VisitConversion_expression([NotNull] DassieParser.Conversion_expressionContext context)
    {
        return new ConversionExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            Expression = (ExpressionSyntax)Visit(context.expression()),
            OperatorToken = Token(SyntaxKind.LessThanColonToken, context.Less_Than_Colon()),
            Type = (TypeSyntax)Visit(context.type_name())
        };
    }

    public override SyntaxNode VisitCustom_operator_binary_expression([NotNull] DassieParser.Custom_operator_binary_expressionContext context)
    {
        return base.VisitCustom_operator_binary_expression(context);
    }

    public override SyntaxNode VisitDelimited_range_expression([NotNull] DassieParser.Delimited_range_expressionContext context)
    {
        return new RangeExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            DoubleDotToken = Token(SyntaxKind.DoubleDotToken, context.Double_Dot()),
            Start = (ExpressionSyntax)Visit(context.expression()[0]),
            End = (ExpressionSyntax)Visit(context.expression()[1])
        };
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
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.SlashToken, context.Slash()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitElif_branch([NotNull] DassieParser.Elif_branchContext context)
    {
        return new ElseIfClauseSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            ColonToken = Token(SyntaxKind.ColonToken, context.Colon()),
            Condition = (ExpressionSyntax)Visit(context.expression()[0]),
            EqualsToken = Token(SyntaxKind.EqualsToken, context.Equals()),
            Body = (ExpressionSyntax)Visit((IParseTree)context.code_block() ?? context.expression()[1])
        };
    }

    public override SyntaxNode VisitElse_branch([NotNull] DassieParser.Else_branchContext context)
    {
        return new ElseClauseSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            ColonToken = Token(SyntaxKind.ColonToken, context.Colon()),
            EqualsToken = Token(SyntaxKind.EqualsToken, context.Equals()),
            Body = (ExpressionSyntax)Visit((IParseTree)context.code_block() ?? context.expression())
        };
    }

    public override SyntaxNode VisitElse_unless_branch([NotNull] DassieParser.Else_unless_branchContext context)
    {
        return new ElseUnlessClauseSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            ElseUnlessToken = Token(SyntaxKind.ExclamationColonToken, context.Exclamation_Colon()),
            Condition = (ExpressionSyntax)Visit(context.expression()[0]),
            EqualsToken = Token(SyntaxKind.EqualsToken, context.Equals()),
            Body = (ExpressionSyntax)Visit((IParseTree)context.code_block() ?? context.expression()[1])
        };
    }

    public override SyntaxNode VisitEmpty_atom([NotNull] DassieParser.Empty_atomContext context)
    {
        return new EmptyExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OpenParenToken = Token(SyntaxKind.OpenParenToken, context.Open_Paren()),
            CloseParenToken = Token(SyntaxKind.CloseParenToken, context.Close_Paren())
        };
    }

    public override SyntaxNode VisitEquality_expression([NotNull] DassieParser.Equality_expressionContext context)
    {
        SyntaxKind tokenKind;

        if (context.op.Text == "==")
            tokenKind = SyntaxKind.DoubleEqualsToken;
        else
            tokenKind = SyntaxKind.ExclamationEqualsToken;

        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1]),
            OperatorToken = Token(tokenKind, context.op.Text)
        };
    }

    public override SyntaxNode VisitExport_directive([NotNull] DassieParser.Export_directiveContext context)
    {
        return new ExportDirectiveSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            ExportKeyword = Token(SyntaxKind.ExportKeyword, context.Export()),
            Name = (NameSyntax)Visit(context.full_identifier())
        };
    }

    public override SyntaxNode VisitExpression_atom([NotNull] DassieParser.Expression_atomContext context)
    {
        return new ParenthesizedExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OpenParenToken = Token(SyntaxKind.OpenParenToken, context.Open_Paren()),
            Expression = (ExpressionSyntax)Visit(context.expression()),
            CloseParenToken = Token(SyntaxKind.CloseParenToken, context.Close_Paren())
        };
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
        return new FileBodySyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            Items = context.children.Select(Visit).ToList()
        };
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
        if (context.Identifier().Length == 1)
        {
            return new IdentifierNameSyntax()
            {
                FirstToken = ToSyntaxToken(context.Start),
                LastToken = ToSyntaxToken(context.Stop),
                Span = GetSpan(context),
                Identifier = Identifier(context.Identifier()[0])
            };
        }

        List<IParseTree> remainingChildren = context.children.Take(context.ChildCount - 2).ToList();
        DassieParser.Full_identifierContext left = new((ParserRuleContext)context.Parent, context.invokingState)
        {
            Start = ((ITerminalNode)context.children[1]).Symbol,
            Stop = ((ITerminalNode)remainingChildren.Last()).Symbol,
            children = remainingChildren
        };

        ITerminalNode last = context.Identifier().Last();

        return new QualifiedNameSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            Left = (NameSyntax)VisitFull_identifier(left),
            DotToken = Token(SyntaxKind.DotToken, context.Dot().Last()),
            Right = new IdentifierNameSyntax()
            {
                FirstToken = ToSyntaxToken(last.Symbol),
                LastToken = ToSyntaxToken(last.Symbol),
                Span = GetSpan(last),
                Identifier = Identifier(last)
            }
        };
    }

    public override SyntaxNode VisitFull_identifier_member_access_expression([NotNull] DassieParser.Full_identifier_member_access_expressionContext context)
    {
        NameSyntax fullId = (NameSyntax)Visit(context.full_identifier());

        if (context.generic_arg_list() != null)
        {
            fullId = new GenericNameSyntax()
            {
                FirstToken = fullId.FirstToken,
                LastToken = ToSyntaxToken(context.generic_arg_list().Stop),
                Span = GetSpan(context.full_identifier().Start, context.generic_arg_list().Stop),
                Name = fullId,
                TypeArguments = (GenericArgumentListSyntax)Visit(context.generic_arg_list())
            };
        }

        return new InvocationExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            Callee = new NameExpressionSyntax()
            {
                FirstToken = fullId.FirstToken,
                LastToken = fullId.LastToken,
                Span = fullId.Span,
                Name = fullId
            },
            Arguments = (ArgumentListSyntax)VisitOrNull(context.arglist())
        };
    }
    
    public override SyntaxNode VisitFull_program([NotNull] DassieParser.Full_programContext context)
    {
        return new FileBodySyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Start),
            Span = GetSpan(context),
            Items = context.children.Select(Visit).ToList()
        };
    }

    public override SyntaxNode VisitFull_range_expression([NotNull] DassieParser.Full_range_expressionContext context)
    {
        return new RangeExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            DoubleDotToken = Token(SyntaxKind.DoubleDotToken, context.Double_Dot()),
            Start = null,
            End = null
        };
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

    public override SyntaxNode VisitIf_branch([NotNull] DassieParser.If_branchContext context)
    {
        return new IfClauseSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            QuestionToken = Token(SyntaxKind.QuestionMarkToken, context.Question_Mark()),
            Condition = (ExpressionSyntax)Visit(context.expression()[0]),
            EqualsToken = Token(SyntaxKind.EqualsToken, context.Equals()),
            Body = (ExpressionSyntax)Visit((IParseTree)context.code_block() ?? context.expression()[1])
        };
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
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.ArrowLeftToken, context.Arrow_Left()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitLeft_shift_expression([NotNull] DassieParser.Left_shift_expressionContext context)
    {
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.DoubleLessThanToken, context.Double_Less_Than()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
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
        return new LockExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            LockKeyword = Token(SyntaxKind.LockKeyword, context.Lock()),
            Target = (ExpressionSyntax)Visit(context.expression()[0]),
            EqualsToken = Token(SyntaxKind.EqualsToken, context.Equals()),
            Body = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitLogical_and_expression([NotNull] DassieParser.Logical_and_expressionContext context)
    {
        return base.VisitLogical_and_expression(context);
    }

    public override SyntaxNode VisitLogical_negation_expression([NotNull] DassieParser.Logical_negation_expressionContext context)
    {
        return new UnaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.ExclamationMarkToken, context.Exclamation_Mark()),
            Operand = (ExpressionSyntax)Visit(context.expression())
        };
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
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.DoublePercentToken, context.Double_Percent()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitMultiply_expression([NotNull] DassieParser.Multiply_expressionContext context)
    {
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.AsteriskToken, context.Asterisk()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitNested_type_access_modifier([NotNull] DassieParser.Nested_type_access_modifierContext context)
    {
        return base.VisitNested_type_access_modifier(context);
    }

    public override SyntaxNode VisitNewlined_expression([NotNull] DassieParser.Newlined_expressionContext context)
    {
        return Visit(context.expression());
    }

    public override SyntaxNode VisitOpen_ended_range_expression([NotNull] DassieParser.Open_ended_range_expressionContext context)
    {
        return new RangeExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            DoubleDotToken = Token(SyntaxKind.DoubleDotToken, context.Double_Dot()),
            Start = (ExpressionSyntax)Visit(context.expression()),
            End = null
        };
    }

    public override SyntaxNode VisitOr_expression([NotNull] DassieParser.Or_expressionContext context)
    {
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.BarToken, context.Bar()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
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
        return new PlaceholderExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            DotToken = Token(SyntaxKind.DotToken, context.Dot())
        };
    }

    public override SyntaxNode VisitPostfix_if_expression([NotNull] DassieParser.Postfix_if_expressionContext context)
    {
        return new PostfixIfExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            QuestionToken = Token(SyntaxKind.QuestionMarkToken, context.postfix_if_branch().Question_Mark()),
            Expression = (ExpressionSyntax)Visit(context.expression()),
            Condition = (ExpressionSyntax)Visit(context.postfix_if_branch().expression())
        };
    }

    public override SyntaxNode VisitPostfix_unless_expression([NotNull] DassieParser.Postfix_unless_expressionContext context)
    {
        return new PostfixUnlessExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            UnlessToken = Token(SyntaxKind.ExclamationQuestionToken, context.postfix_unless_branch().Exclamation_Question()),
            Expression = (ExpressionSyntax)Visit(context.expression()),
            Condition = (ExpressionSyntax)Visit(context.postfix_unless_branch().expression())
        };
    }

    public override SyntaxNode VisitPower_expression([NotNull] DassieParser.Power_expressionContext context)
    {
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.DoubleAsteriskToken, context.Double_Asterisk()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitPredicate([NotNull] DassieParser.PredicateContext context)
    {
        return base.VisitPredicate(context);
    }

    public override SyntaxNode VisitPrefix_if_expression([NotNull] DassieParser.Prefix_if_expressionContext context)
    {
        return new IfExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            IfClause = (IfClauseSyntax)Visit(context.if_branch()),
            ElseClause = (ElseClauseSyntax)Visit(context.else_branch()),
            ElseIfClauses = context.elif_branch()?.Select(Visit).Cast<ElseIfClauseSyntax>().ToList()
        };
    }

    public override SyntaxNode VisitPrefix_unless_expression([NotNull] DassieParser.Prefix_unless_expressionContext context)
    {
        return new UnlessExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            UnlessClause = (UnlessClauseSyntax)Visit(context.unless_branch()),
            ElseClause = (ElseClauseSyntax)Visit(context.else_branch()),
            ElseUnlessClauses = context.else_unless_branch()?.Select(Visit).Cast<ElseUnlessClauseSyntax>().ToList()
        };
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
        return new RaiseExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            RaiseKeyword = Token(SyntaxKind.RaiseKeyword, context.Raise()),
            Expression = (ExpressionSyntax)Visit(context.expression())
        };
    }

    public override SyntaxNode VisitRange_index_expression([NotNull] DassieParser.Range_index_expressionContext context)
    {
        return new RangeIndexExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            CaretToken = Token(SyntaxKind.CaretToken, context.Caret()),
            Index = (ExpressionSyntax)Visit(context.integer_atom())
        };
    }

    public override SyntaxNode VisitReal_atom([NotNull] DassieParser.Real_atomContext context)
    {
        return base.VisitReal_atom(context);
    }

    public override SyntaxNode VisitRemainder_expression([NotNull] DassieParser.Remainder_expressionContext context)
    {
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.PercentToken, context.Percent()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitRemove_handler([NotNull] DassieParser.Remove_handlerContext context)
    {
        return base.VisitRemove_handler(context);
    }

    public override SyntaxNode VisitRethrow_exception([NotNull] DassieParser.Rethrow_exceptionContext context)
    {
        return new RaiseExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            RaiseKeyword = Token(SyntaxKind.RaiseKeyword, context.Raise()),
            Expression = null
        };
    }

    public override SyntaxNode VisitRight_pipe_expression([NotNull] DassieParser.Right_pipe_expressionContext context)
    {
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.ArrowRightToken, context.Arrow_Right()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitRight_shift_expression([NotNull] DassieParser.Right_shift_expressionContext context)
    {
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.DoubleGreaterThanToken, context.Double_Greater_Than()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitSafe_conversion_expression([NotNull] DassieParser.Safe_conversion_expressionContext context)
    {
        return new ConversionExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            Expression = (ExpressionSyntax)Visit(context.expression()),
            OperatorToken = Token(SyntaxKind.LessThanQuestionMarkColonToken, context.Less_Than_Question_Mark_Colon()),
            Type = (TypeSyntax)Visit(context.type_name())
        };
    }

    public override SyntaxNode VisitSeparated_expression([NotNull] DassieParser.Separated_expressionContext context)
    {
        return Visit(context.expression());
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
        // TODO: Make separate InterpolatedStringSyntax for interpolated strings
        return base.VisitString_atom(context);
    }

    public override SyntaxNode VisitSubtraction_expression([NotNull] DassieParser.Subtraction_expressionContext context)
    {
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.MinusToken, context.Minus()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitThis_atom([NotNull] DassieParser.This_atomContext context)
    {
        return new ThisExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            ThisKeyword = Token(SyntaxKind.ThisKeyword, context.This())
        };
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
        SeparatedSyntaxList<ExpressionSyntax> items = new()
        {
            Nodes = context.expression().Select(Visit).Cast<ExpressionSyntax>().ToList(),
            Separators = context.Comma().Select(t => Token(SyntaxKind.CommaToken, t)).ToList()
        };

        return new TupleExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OpenParenToken = Token(SyntaxKind.OpenParenToken, context.Open_Paren()),
            CloseParenToken = Token(SyntaxKind.CloseParenToken, context.Close_Paren()),
            Elements = items
        };
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
        if (context.identifier_atom() != null)
        {
            return new NameTypeSyntax()
            {
                FirstToken = ToSyntaxToken(context.Start),
                LastToken = ToSyntaxToken(context.Stop),
                Span = GetSpan(context),
                Name = (NameSyntax)Visit(context.identifier_atom())
            };
        }

        SeparatedSyntaxList<TypeMemberSyntax> typeMemberList = new()
        {
            Nodes = context.union_or_tuple_type_member()?.Select(Visit).Cast<TypeMemberSyntax>().ToList()
        };

        if (context.Bar()?.Length > 0)
        {
            return new UnionTypeSyntax()
            {
                FirstToken = ToSyntaxToken(context.Start),
                LastToken = ToSyntaxToken(context.Stop),
                Span = GetSpan(context),
                OpenParenToken = Token(SyntaxKind.OpenParenToken, context.Open_Paren()),
                CloseParenToken = Token(SyntaxKind.CloseParenToken, context.Close_Paren()),
                Members = typeMemberList with
                {
                    Separators = context.Bar().Select(b => Token(SyntaxKind.BarToken, b)).ToList()
                }
            };
        }

        if (context.Comma()?.Length > 0)
        {
            return new TupleTypeSyntax()
            {
                FirstToken = ToSyntaxToken(context.Start),
                LastToken = ToSyntaxToken(context.Stop),
                Span = GetSpan(context),
                OpenParenToken = Token(SyntaxKind.OpenParenToken, context.Open_Paren()),
                CloseParenToken = Token(SyntaxKind.CloseParenToken, context.Close_Paren()),
                Members = typeMemberList with
                {
                    Separators = context.Comma().Select(b => Token(SyntaxKind.CommaToken, b)).ToList()
                }
            };
        }

        // TODO: Handle remaining type names
        return base.VisitType_name(context);
    }

    public override SyntaxNode VisitType_special_modifier([NotNull] DassieParser.Type_special_modifierContext context)
    {
        return base.VisitType_special_modifier(context);
    }

    public override SyntaxNode VisitUnion_or_tuple_type_member([NotNull] DassieParser.Union_or_tuple_type_memberContext context)
    {
        return new NamedTypeMemberSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            Name = Identifier(context.Identifier()),
            ColonToken = Token(SyntaxKind.ColonToken, context.Colon()),
            Type = (TypeSyntax)Visit(context.type_name())
        };
    }

    public override SyntaxNode VisitUnless_branch([NotNull] DassieParser.Unless_branchContext context)
    {
        return new UnlessClauseSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            UnlessToken = Token(SyntaxKind.ExclamationQuestionToken, context.Exclamation_Question()),
            Condition = (ExpressionSyntax)Visit(context.expression()[0]),
            EqualsToken = Token(SyntaxKind.EqualsToken, context.Equals()),
            Body = (ExpressionSyntax)Visit((IParseTree)context.code_block() ?? context.expression()[1])
        };
    }

    public override SyntaxNode VisitUntil_loop([NotNull] DassieParser.Until_loopContext context)
    {
        return new UntilExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            UntilToken = Token(SyntaxKind.ExclamationAtToken, context.Exclamation_At()),
            Condition = (ExpressionSyntax)Visit(context.expression()[0]),
            EqualsToken = Token(SyntaxKind.EqualsToken, context.Equals()),
            Body = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitWhile_loop([NotNull] DassieParser.While_loopContext context)
    {
        return new WhileExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            AtToken = Token(SyntaxKind.AtSignToken, context.At_Sign()),
            Condition = (ExpressionSyntax)Visit(context.expression()[0]),
            EqualsToken = Token(SyntaxKind.EqualsToken, context.Equals()),
            Body = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }

    public override SyntaxNode VisitWildcard_atom([NotNull] DassieParser.Wildcard_atomContext context)
    {
        return new WildcardExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            UnderscoreToken = Token(SyntaxKind.UnderscoreToken, context.Underscore())
        };
    }

    public override SyntaxNode VisitXor_expression([NotNull] DassieParser.Xor_expressionContext context)
    {
        return new BinaryExpressionSyntax()
        {
            FirstToken = ToSyntaxToken(context.Start),
            LastToken = ToSyntaxToken(context.Stop),
            Span = GetSpan(context),
            OperatorToken = Token(SyntaxKind.CaretToken, context.Caret()),
            Left = (ExpressionSyntax)Visit(context.expression()[0]),
            Right = (ExpressionSyntax)Visit(context.expression()[1])
        };
    }
}