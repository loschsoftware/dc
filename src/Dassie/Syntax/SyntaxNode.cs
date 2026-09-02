using System;
using System.Collections.Generic;

namespace Dassie.Syntax;

internal readonly record struct TextSpan(int Start, int Length)
{
    public static TextSpan None { get; } = new(0, 0);
    public int End => Start + Length;
    public static TextSpan FromBounds(int start, int end) => new(start, end - start);
}

internal record SyntaxTrivia
{
    public SyntaxKind Kind { get; init; } = SyntaxKind.None;
    public string Text { get; init; }
    public TextSpan Span { get; init; } = TextSpan.None;
}

internal record SyntaxToken : SyntaxNode
{
    public static SyntaxToken None { get; } = new();
    public override SyntaxKind Kind => SyntaxKind.None;

    public string Text { get; init; }
    public object Value { get; init; }
    public IReadOnlyList<SyntaxTrivia> LeadingTrivia { get; init; } = [];
    public IReadOnlyList<SyntaxTrivia> TrailingTrivia { get; init; } = [];

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren();
}

internal abstract record SeparatedSyntaxList
{
    public abstract IEnumerable<SyntaxNode> GetChildren();
}

internal record SeparatedSyntaxList<TNode> : SeparatedSyntaxList
    where TNode : SyntaxNode
{
    public IReadOnlyList<TNode> Nodes { get; init; } = [];
    public IReadOnlyList<SyntaxToken> Separators { get; init; } = [];

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        int count = Math.Max(Nodes.Count, Separators.Count);
        for (int index = 0; index < count; index++)
        {
            if (index < Nodes.Count)
                yield return Nodes[index];
            if (index < Separators.Count)
                yield return Separators[index];
        }
    }
}

internal abstract record SyntaxNode
{
    public abstract SyntaxKind Kind { get; }
    public TextSpan Span { get; init; } = TextSpan.None;
    public TextSpan FullSpan { get; init; } = TextSpan.None;
    public SyntaxToken FirstToken { get; init; } = SyntaxToken.None;
    public SyntaxToken LastToken { get; init; } = SyntaxToken.None;

    public abstract IEnumerable<SyntaxNode> GetChildren();

    protected static IEnumerable<SyntaxNode> EnumerateChildren(params object[] children)
    {
        foreach (object child in children)
        {
            switch (child)
            {
                case SyntaxNode node:
                    yield return node;
                    break;
                case SeparatedSyntaxList list:
                    foreach (SyntaxNode listChild in list.GetChildren())
                        yield return listChild;
                    break;
                case IEnumerable<SyntaxNode> nodes:
                    foreach (SyntaxNode node in nodes)
                        yield return node;
                    break;
            }
        }
    }
}

internal record CompilationUnitSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    public IReadOnlyList<DirectiveSyntax> Directives { get; init; } = [];
    public FileBodySyntax Body { get; init; }
    public SyntaxToken EndOfFileToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        foreach (DirectiveSyntax directive in Directives)
            yield return directive;

        yield return Body;
        yield return EndOfFileToken;
    }
}

internal record FileBodySyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.FileBody;
    public IReadOnlyList<SyntaxNode> Items { get; init; } = [];

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Items);
}

internal abstract record DirectiveSyntax : SyntaxNode;

internal record ImportDirectiveSyntax : DirectiveSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ImportDirective;
    public SyntaxToken BangToken { get; init; }
    public SyntaxToken ImportKeyword { get; init; } = SyntaxToken.None;
    public SeparatedSyntaxList<NameSyntax> Names { get; init; } = new();
    public bool IsGlobalImport => BangToken is not null;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(BangToken, ImportKeyword, Names);
}

internal record ExportDirectiveSyntax : DirectiveSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ExportDirective;
    public SyntaxToken ExportKeyword { get; init; } = SyntaxToken.None;
    public NameSyntax Name { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(ExportKeyword, Name);
}

internal abstract record NameSyntax : SyntaxNode;

internal abstract record SimpleNameSyntax : NameSyntax;

internal record IdentifierNameSyntax : SimpleNameSyntax
{
    public override SyntaxKind Kind => SyntaxKind.IdentifierName;
    public SyntaxToken Identifier { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Identifier);
}

internal record QualifiedNameSyntax : NameSyntax
{
    public override SyntaxKind Kind => SyntaxKind.QualifiedName;
    public NameSyntax Left { get; init; }
    public SyntaxToken DotToken { get; init; } = SyntaxToken.None;
    public SimpleNameSyntax Right { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Left, DotToken, Right);
}

internal record GenericNameSyntax : SimpleNameSyntax
{
    public override SyntaxKind Kind => SyntaxKind.GenericName;
    public NameSyntax Name { get; init; }
    public GenericArgumentListSyntax TypeArguments { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Name, TypeArguments);
}

internal abstract record TypeSyntax : SyntaxNode;

internal record NameTypeSyntax : TypeSyntax
{
    public override SyntaxKind Kind => SyntaxKind.NameType;
    public NameSyntax Name { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Name);
}

internal record GenericTypeSyntax : TypeSyntax
{
    public override SyntaxKind Kind => SyntaxKind.GenericType;
    public TypeSyntax Type { get; init; }
    public GenericArgumentListSyntax Arguments { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Type, Arguments);
}

internal record UnionTypeSyntax : TypeSyntax
{
    public override SyntaxKind Kind => SyntaxKind.UnionType;
    public SyntaxToken OpenParenToken { get; init; } = SyntaxToken.None;
    public SeparatedSyntaxList<TypeMemberSyntax> Members { get; init; } = new();
    public SyntaxToken CloseParenToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenParenToken, Members, CloseParenToken);
}

internal record TupleTypeSyntax : TypeSyntax
{
    public override SyntaxKind Kind => SyntaxKind.TupleType;
    public SyntaxToken OpenParenToken { get; init; } = SyntaxToken.None;
    public SeparatedSyntaxList<TypeMemberSyntax> Members { get; init; } = new();
    public SyntaxToken CloseParenToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenParenToken, Members, CloseParenToken);
}

internal abstract record TypeMemberSyntax : SyntaxNode;

internal record NamedTypeMemberSyntax : TypeMemberSyntax
{
    public override SyntaxKind Kind => SyntaxKind.NamedTypeMember;
    public SyntaxToken Name { get; init; }
    public SyntaxToken ColonToken { get; init; }
    public TypeSyntax Type { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Name, ColonToken, Type);
}

internal abstract record GenericArgumentSyntax : SyntaxNode;

internal record GenericArgumentListSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.GenericArgumentList;
    public SyntaxToken OpenBracketToken { get; init; } = SyntaxToken.None;
    public SeparatedSyntaxList<GenericArgumentSyntax> Arguments { get; init; } = new();
    public SyntaxToken CloseBracketToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenBracketToken, Arguments, CloseBracketToken);
}

internal record TypeGenericArgumentSyntax : GenericArgumentSyntax
{
    public override SyntaxKind Kind => SyntaxKind.TypeGenericArgument;
    public TypeSyntax Type { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Type);
}

internal record ExpressionGenericArgumentSyntax : GenericArgumentSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ExpressionGenericArgument;
    public ExpressionSyntax Expression { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Expression);
}

internal record PredicateGenericArgumentSyntax : GenericArgumentSyntax
{
    public override SyntaxKind Kind => SyntaxKind.PredicateGenericArgument;
    public PredicateSyntax Predicate { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Predicate);
}

internal abstract record DeclarationSyntax : SyntaxNode;

internal abstract record MemberDeclarationSyntax : DeclarationSyntax;

internal record TypeDeclarationSyntax : DeclarationSyntax
{
    public override SyntaxKind Kind => SyntaxKind.TypeDeclaration;
    public AttributeListSyntax Attributes { get; init; } = new();
    public ModifierListSyntax Modifiers { get; init; } = new();
    public TypeKindSyntax TypeKind { get; init; }
    public SyntaxToken Identifier { get; init; } = SyntaxToken.None;
    public GenericParameterListSyntax TypeParameters { get; init; }
    public ParameterListSyntax PrimaryParameters { get; init; }
    public BaseListSyntax BaseList { get; init; }
    public TypeBodySyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Attributes, Modifiers, TypeKind, Identifier, TypeParameters, PrimaryParameters, BaseList, Body);
}

internal record TypeKindSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.TypeKind;
    public TypeKind Value { get; init; }
    public IReadOnlyList<SyntaxToken> Tokens { get; init; } = [];

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Tokens);
}

[Flags]
internal enum TypeKind
{
    Ref = 1,
    Val = 2,
    Template = 4,
    Module = 8,
    Bang = 16,
    Ampersand = 32,

    ByRefLikeVal = Val | Ampersand,
    ReadOnlyVal = Val | Bang,
    ReadOnlyByRefLikeVal = Val | Ampersand | Bang
}

internal abstract record TypeBodySyntax : SyntaxNode;

internal record BlockTypeBodySyntax : TypeBodySyntax
{
    public override SyntaxKind Kind => SyntaxKind.BlockTypeBody;
    public SyntaxToken OpenBraceToken { get; init; } = SyntaxToken.None;
    public IReadOnlyList<SyntaxNode> Members { get; init; } = [];
    public SyntaxToken CloseBraceToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenBraceToken, Members, CloseBraceToken);
}

internal record AliasTypeBodySyntax : TypeBodySyntax
{
    public override SyntaxKind Kind => SyntaxKind.AliasTypeBody;
    public TypeSyntax Type { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Type);
}

internal record BaseListSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.BaseList;
    public SyntaxToken ColonToken { get; init; } = SyntaxToken.None;
    public SeparatedSyntaxList<TypeSyntax> Types { get; init; } = new();

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(ColonToken, Types);
}

internal record FieldDeclarationSyntax : MemberDeclarationSyntax
{
    public override SyntaxKind Kind => SyntaxKind.FieldDeclaration;
    public AttributeListSyntax Attributes { get; init; } = new();
    public ModifierListSyntax Modifiers { get; init; } = new();
    public SyntaxToken VarOrValKeyword { get; init; }
    public SyntaxToken Identifier { get; init; } = SyntaxToken.None;
    public SyntaxToken ColonToken { get; init; }
    public TypeSyntax Type { get; init; }
    public SyntaxToken EqualsToken { get; init; }
    public ExpressionSyntax Initializer { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Attributes, Modifiers, VarOrValKeyword, Identifier, ColonToken, Type, EqualsToken, Initializer);
}

internal record FunctionDeclarationSyntax : MemberDeclarationSyntax
{
    public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;
    public AttributeListSyntax Attributes { get; init; } = new();
    public ModifierListSyntax Modifiers { get; init; } = new();
    public SyntaxToken Identifier { get; init; } = SyntaxToken.None;
    public GenericParameterListSyntax TypeParameters { get; init; }
    public ParameterListSyntax Parameters { get; init; }
    public SyntaxToken ColonToken { get; init; }
    public TypeSyntax ReturnType { get; init; }
    public SyntaxToken EqualsToken { get; init; }
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Attributes, Modifiers, Identifier, TypeParameters, Parameters, ColonToken, ReturnType, EqualsToken, Body);
}

internal record LocalFunctionDeclarationSyntax : DeclarationSyntax
{
    public override SyntaxKind Kind => SyntaxKind.LocalFunctionDeclaration;
    public SyntaxToken VarOrValKeyword { get; init; }
    public SyntaxToken Identifier { get; init; } = SyntaxToken.None;
    public GenericParameterListSyntax TypeParameters { get; init; }
    public ParameterListSyntax Parameters { get; init; }
    public SyntaxToken ColonToken { get; init; }
    public TypeSyntax ReturnType { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(VarOrValKeyword, Identifier, TypeParameters, Parameters, ColonToken, ReturnType, EqualsToken, Body);
}

internal record OperatorDeclarationSyntax : MemberDeclarationSyntax
{
    public override SyntaxKind Kind => SyntaxKind.OperatorDeclaration;
    public AttributeListSyntax Attributes { get; init; } = new();
    public ModifierListSyntax Modifiers { get; init; } = new();
    public SyntaxToken OperatorToken { get; init; } = SyntaxToken.None;
    public ParameterListSyntax Parameters { get; init; }
    public SyntaxToken ColonToken { get; init; }
    public TypeSyntax ReturnType { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Attributes, Modifiers, OperatorToken, Parameters, ColonToken, ReturnType, EqualsToken, Body);
}

internal abstract record AccessorOwnerDeclarationSyntax : MemberDeclarationSyntax
{
    public AttributeListSyntax Attributes { get; init; } = new();
    public ModifierListSyntax Modifiers { get; init; } = new();
    public SyntaxToken Identifier { get; init; } = SyntaxToken.None;
    public GenericParameterListSyntax TypeParameters { get; init; }
    public SyntaxToken ColonToken { get; init; }
    public TypeSyntax Type { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public AccessorBlockSyntax Accessors { get; init; }
}

internal record PropertyDeclarationSyntax : AccessorOwnerDeclarationSyntax
{
    public override SyntaxKind Kind => SyntaxKind.PropertyDeclaration;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Attributes, Modifiers, Identifier, TypeParameters, ColonToken, Type, EqualsToken, Accessors);
}

internal record EventDeclarationSyntax : AccessorOwnerDeclarationSyntax
{
    public override SyntaxKind Kind => SyntaxKind.EventDeclaration;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Attributes, Modifiers, Identifier, TypeParameters, ColonToken, Type, EqualsToken, Accessors);
}

internal record AccessorBlockSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.AccessorBlock;
    public SyntaxToken OpenBraceToken { get; init; } = SyntaxToken.None;
    public IReadOnlyList<AccessorDeclarationSyntax> Accessors { get; init; } = [];
    public SyntaxToken CloseBraceToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenBraceToken, Accessors, CloseBraceToken);
}

internal record AccessorDeclarationSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.AccessorDeclaration;
    public SyntaxToken Keyword { get; init; } = SyntaxToken.None;
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Keyword, EqualsToken, Body);
}

internal record ExternalBlockSyntax : MemberDeclarationSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ExternalBlock;
    public SyntaxToken ExternKeyword { get; init; } = SyntaxToken.None;
    public SyntaxToken StringLiteral { get; init; } = SyntaxToken.None;
    public SyntaxToken AsToken { get; init; }
    public SyntaxToken TypeName { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public SyntaxToken OpenBraceToken { get; init; } = SyntaxToken.None;
    public IReadOnlyList<SyntaxNode> Members { get; init; } = [];
    public SyntaxToken CloseBraceToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(ExternKeyword, StringLiteral, AsToken, TypeName, EqualsToken, OpenBraceToken, Members, CloseBraceToken);
}

internal record AccessModifierMemberGroupSyntax : MemberDeclarationSyntax
{
    public override SyntaxKind Kind => SyntaxKind.AccessModifierMemberGroup;
    public ModifierListSyntax Modifiers { get; init; } = new();
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public SyntaxToken OpenBraceToken { get; init; } = SyntaxToken.None;
    public IReadOnlyList<SyntaxNode> Members { get; init; } = [];
    public SyntaxToken CloseBraceToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Modifiers, EqualsToken, OpenBraceToken, Members, CloseBraceToken);
}

internal record SpecialSymbolSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.SpecialSymbol;
    public SyntaxToken StartToken { get; init; } = SyntaxToken.None;
    public SyntaxToken Name { get; init; } = SyntaxToken.None;
    public IReadOnlyList<ExpressionSyntax> Arguments { get; init; } = [];
    public SyntaxToken CloseBraceToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(StartToken, Name, Arguments, CloseBraceToken);
}

internal record AttributeListSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.AttributeList;
    public IReadOnlyList<AttributeSyntax> Attributes { get; init; } = [];

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Attributes);
}

internal record AttributeSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.Attribute;
    public SyntaxToken LessThanToken { get; init; } = SyntaxToken.None;
    public SyntaxToken TargetToken { get; init; }
    public SyntaxToken TargetColonToken { get; init; }
    public TypeSyntax Type { get; init; }
    public ArgumentListSyntax Arguments { get; init; }
    public SyntaxToken GreaterThanToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(LessThanToken, TargetToken, TargetColonToken, Type, Arguments, GreaterThanToken);
}

internal record ModifierListSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.ModifierList;
    public IReadOnlyList<SyntaxToken> Modifiers { get; init; } = [];

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Modifiers);
}

internal record ParameterListSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.ParameterList;
    public SyntaxToken OpenParenToken { get; init; }
    public SeparatedSyntaxList<ParameterSyntax> Parameters { get; init; } = new();
    public SyntaxToken CloseParenToken { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenParenToken, Parameters, CloseParenToken);
}

internal record ParameterSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.Parameter;
    public AttributeListSyntax Attributes { get; init; } = new();
    public SyntaxToken ValOrVarKeyword { get; init; }
    public ParameterModifierSyntax Modifier { get; init; }
    public SyntaxToken Identifier { get; init; } = SyntaxToken.None;
    public SyntaxToken DoubleDotToken { get; init; }
    public SyntaxToken ColonToken { get; init; }
    public TypeSyntax Type { get; init; }
    public SyntaxToken EqualsToken { get; init; }
    public ExpressionSyntax DefaultValue { get; init; }
    public bool IsVariadicOrRange => DoubleDotToken is not null;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Attributes, ValOrVarKeyword, Modifier, Identifier, DoubleDotToken, ColonToken, Type, EqualsToken, DefaultValue);
}

internal record ParameterModifierSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.ParameterModifier;
    public SyntaxToken ModifierToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(ModifierToken);
}

internal record GenericParameterListSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.GenericParameterList;
    public SyntaxToken OpenBracketToken { get; init; } = SyntaxToken.None;
    public SeparatedSyntaxList<GenericParameterSyntax> Parameters { get; init; } = new();
    public SyntaxToken CloseBracketToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenBracketToken, Parameters, CloseBracketToken);
}

internal abstract record GenericParameterSyntax : SyntaxNode
{
    public SyntaxToken Identifier { get; init; } = SyntaxToken.None;
    public TypeSyntax Type { get; init; }
}

internal record TypeGenericParameterSyntax : GenericParameterSyntax
{
    public override SyntaxKind Kind => SyntaxKind.TypeGenericParameter;
    public IReadOnlyList<GenericParameterAttributeSyntax> Attributes { get; init; } = [];
    public GenericParameterVarianceSyntax Variance { get; init; }
    public SyntaxToken ColonToken { get; init; }
    public SeparatedSyntaxList<TypeSyntax> Constraints { get; init; } = new();

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Identifier, Type, Attributes, Variance, ColonToken, Constraints);
}

internal record ValueGenericParameterSyntax : GenericParameterSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ValueGenericParameter;
    public SyntaxToken QuoteToken { get; init; } = SyntaxToken.None;
    public SyntaxToken ColonToken { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Identifier, Type, QuoteToken, ColonToken);
}

internal record GenericParameterAttributeSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.GenericParameterAttribute;
    public SyntaxToken AttributeToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(AttributeToken);
}

internal record GenericParameterVarianceSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.GenericParameterVariance;
    public SyntaxToken VarianceToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(VarianceToken);
}

internal abstract record ExpressionSyntax : SyntaxNode;

internal record LiteralExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
    public SyntaxToken LiteralToken { get; init; } = SyntaxToken.None;
    public object Value { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(LiteralToken);
}

internal record NameExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.NameExpression;
    public NameSyntax Name { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Name);
}

internal record ThisExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ThisExpression;
    public SyntaxToken ThisKeyword { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(ThisKeyword);
}

internal record EmptyExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.EmptyExpression;
    public SyntaxToken OpenParenToken { get; init; } = SyntaxToken.None;
    public SyntaxToken CloseParenToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenParenToken, CloseParenToken);
}

internal record WildcardExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.WildcardExpression;
    public SyntaxToken UnderscoreToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(UnderscoreToken);
}

internal record ParenthesizedExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
    public SyntaxToken OpenParenToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Expression { get; init; }
    public SyntaxToken CloseParenToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenParenToken, Expression, CloseParenToken);
}

internal record TupleExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.TupleExpression;
    public SyntaxToken OpenParenToken { get; init; } = SyntaxToken.None;
    public SeparatedSyntaxList<ExpressionSyntax> Elements { get; init; } = new();
    public SyntaxToken CloseParenToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenParenToken, Elements, CloseParenToken);
}

internal record BlockExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.BlockExpression;
    public SyntaxToken OpenBraceToken { get; init; } = SyntaxToken.None;
    public IReadOnlyList<ExpressionSyntax> Expressions { get; init; } = [];
    public PlaceholderExpressionSyntax Placeholder { get; init; }
    public SyntaxToken CloseBraceToken { get; init; } = SyntaxToken.None;
    public bool ContainsPlaceholder => Placeholder is not null;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenBraceToken, Expressions, Placeholder, CloseBraceToken);
}

internal record PlaceholderExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.PlaceholderExpression;
    public SyntaxToken DotToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(DotToken);
}

internal record UnaryExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
    public SyntaxToken OperatorToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Operand { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OperatorToken, Operand);
}

internal record BinaryExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
    public ExpressionSyntax Left { get; init; }
    public SyntaxToken OperatorToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Right { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Left, OperatorToken, Right);
}

internal record AssignmentExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
    public ExpressionSyntax Left { get; init; }
    public SyntaxToken OperatorToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Right { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Left, OperatorToken, Right);
}

internal record LocalDeclarationOrAssignmentExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.LocalDeclarationOrAssignmentExpression;
    public SyntaxToken VarOrValKeyword { get; init; }
    public SyntaxToken Identifier { get; init; } = SyntaxToken.None;
    public SyntaxToken ColonToken { get; init; }
    public TypeSyntax Type { get; init; }
    public SyntaxToken OperatorToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Value { get; init; }
    public bool IsImplicitDeclarationCandidate => VarOrValKeyword is null;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(VarOrValKeyword, Identifier, ColonToken, Type, OperatorToken, Value);
}

internal record MemberAccessExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.MemberAccessExpression;
    public ExpressionSyntax Receiver { get; init; }
    public SyntaxToken DotToken { get; init; } = SyntaxToken.None;
    public SimpleNameSyntax Name { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Receiver, DotToken, Name);
}

internal record InvocationExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.InvocationExpression;
    public ExpressionSyntax Callee { get; init; }
    public ArgumentListSyntax Arguments { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Callee, Arguments);
}

internal record ElementAccessExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ElementAccessExpression;
    public ExpressionSyntax Receiver { get; init; }
    public BracketedArgumentListSyntax Arguments { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Receiver, Arguments);
}

internal record IndexExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.IndexExpression;
    public ExpressionSyntax Receiver { get; init; }
    public SyntaxToken DoubleColonToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Index { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Receiver, DoubleColonToken, Index);
}

internal record ArgumentListSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.ArgumentList;
    public SeparatedSyntaxList<ArgumentSyntax> Arguments { get; init; } = new();
    public SyntaxToken DoubleCommaToken { get; init; }
    public bool HasDoubleComma => DoubleCommaToken is not null;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Arguments, DoubleCommaToken);
}

internal record BracketedArgumentListSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.BracketedArgumentList;
    public SyntaxToken OpenBracketToken { get; init; } = SyntaxToken.None;
    public SeparatedSyntaxList<ArgumentSyntax> Arguments { get; init; } = new();
    public SyntaxToken CloseBracketToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenBracketToken, Arguments, CloseBracketToken);
}

internal record ArgumentSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.Argument;
    public SyntaxToken Name { get; init; }
    public SyntaxToken ColonToken { get; init; }
    public ExpressionSyntax Expression { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Name, ColonToken, Expression);
}

internal record IfExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.IfExpression;
    public IfClauseSyntax IfClause { get; init; }
    public IReadOnlyList<ElseIfClauseSyntax> ElseIfClauses { get; init; } = [];
    public ElseClauseSyntax ElseClause { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(IfClause, ElseIfClauses, ElseClause);
}

internal record PostfixIfExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.PostfixIfExpression;
    public ExpressionSyntax Expression { get; init; }
    public SyntaxToken QuestionToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Condition { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Expression, QuestionToken, Condition);
}

internal record UnlessExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.UnlessExpression;
    public UnlessClauseSyntax UnlessClause { get; init; }
    public IReadOnlyList<ElseUnlessClauseSyntax> ElseUnlessClauses { get; init; } = [];
    public ElseClauseSyntax ElseClause { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(UnlessClause, ElseUnlessClauses, ElseClause);
}

internal record PostfixUnlessExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.PostfixUnlessExpression;
    public ExpressionSyntax Expression { get; init; }
    public SyntaxToken UnlessToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Condition { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Expression, UnlessToken, Condition);
}

internal record IfClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.IfClause;
    public SyntaxToken QuestionToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Condition { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(QuestionToken, Condition, EqualsToken, Body);
}

internal record ElseIfClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.ElseIfClause;
    public SyntaxToken ColonToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Condition { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(ColonToken, Condition, EqualsToken, Body);
}

internal record ElseClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.ElseClause;
    public SyntaxToken ColonToken { get; init; } = SyntaxToken.None;
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(ColonToken, EqualsToken, Body);
}

internal record UnlessClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.UnlessClause;
    public SyntaxToken UnlessToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Condition { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(UnlessToken, Condition, EqualsToken, Body);
}

internal record ElseUnlessClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.ElseUnlessClause;
    public SyntaxToken ElseUnlessToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Condition { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(ElseUnlessToken, Condition, EqualsToken, Body);
}

internal record MatchExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.MatchExpression;
    public SyntaxToken DollarToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Expression { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public MatchBlockSyntax Block { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(DollarToken, Expression, EqualsToken, Block);
}

internal record MatchBlockSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.MatchBlock;
    public SyntaxToken OpenBraceToken { get; init; } = SyntaxToken.None;
    public IReadOnlyList<MatchCaseSyntax> Cases { get; init; } = [];
    public SyntaxToken CloseBraceToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenBraceToken, Cases, CloseBraceToken);
}

internal record MatchCaseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.MatchCase;
    public SyntaxToken MarkerToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax PatternOrCondition { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }
    public bool IsDefault => PatternOrCondition is null;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(MarkerToken, PatternOrCondition, EqualsToken, Body);
}

internal record TryExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.TryExpression;
    public TryClauseSyntax TryClause { get; init; }
    public IReadOnlyList<CatchClauseSyntax> Catches { get; init; } = [];
    public FaultClauseSyntax Fault { get; init; }
    public FinallyClauseSyntax Finally { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(TryClause, Catches, Fault, Finally);
}

internal record TryClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.TryClause;
    public SyntaxToken TryKeyword { get; init; } = SyntaxToken.None;
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(TryKeyword, EqualsToken, Body);
}

internal record CatchClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.CatchClause;
    public SyntaxToken CatchKeyword { get; init; } = SyntaxToken.None;
    public SyntaxToken Identifier { get; init; }
    public SyntaxToken ColonToken { get; init; }
    public TypeSyntax Type { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(CatchKeyword, Identifier, ColonToken, Type, EqualsToken, Body);
}

internal record FaultClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.FaultClause;
    public SyntaxToken FaultKeyword { get; init; } = SyntaxToken.None;
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(FaultKeyword, EqualsToken, Body);
}

internal record FinallyClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.FinallyClause;
    public SyntaxToken FinallyKeyword { get; init; } = SyntaxToken.None;
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(FinallyKeyword, EqualsToken, Body);
}

internal record ForEachExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ForEachExpression;
    public SyntaxToken AtToken { get; init; } = SyntaxToken.None;
    public IReadOnlyList<ForEachVariableSyntax> Variables { get; init; } = [];
    public SyntaxToken SourceSeparatorToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Source { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(AtToken, Variables, SourceSeparatorToken, Source, EqualsToken, Body);
}

internal record ForEachVariableSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.ForEachVariable;
    public SyntaxToken VarOrValKeyword { get; init; }
    public SyntaxToken Identifier { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(VarOrValKeyword, Identifier);
}

internal record WhileExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.WhileExpression;
    public SyntaxToken AtToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Condition { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(AtToken, Condition, EqualsToken, Body);
}

internal record UntilExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.UntilExpression;
    public SyntaxToken UntilToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Condition { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(UntilToken, Condition, EqualsToken, Body);
}

internal record LockExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.LockExpression;
    public SyntaxToken LockKeyword { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Target { get; init; }
    public SyntaxToken EqualsToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(LockKeyword, Target, EqualsToken, Body);
}

internal record RaiseExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.RaiseExpression;
    public SyntaxToken RaiseKeyword { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Expression { get; init; }
    public bool IsRethrow => Expression is null;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(RaiseKeyword, Expression);
}

internal record ArrayExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ArrayExpression;
    public SyntaxToken AtOpenBracketToken { get; init; } = SyntaxToken.None;
    public SeparatedSyntaxList<ExpressionSyntax> Elements { get; init; } = new();
    public SyntaxToken CloseBracketToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(AtOpenBracketToken, Elements, CloseBracketToken);
}

internal record ListExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ListExpression;
    public SyntaxToken OpenBracketToken { get; init; } = SyntaxToken.None;
    public SeparatedSyntaxList<ExpressionSyntax> Elements { get; init; } = new();
    public SyntaxToken CloseBracketToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenBracketToken, Elements, CloseBracketToken);
}

internal record DictionaryExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.DictionaryExpression;
    public SyntaxToken OpenBracketToken { get; init; } = SyntaxToken.None;
    public SeparatedSyntaxList<KeyValueExpressionSyntax> Elements { get; init; } = new();
    public SyntaxToken CloseBracketToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenBracketToken, Elements, CloseBracketToken);
}

internal record KeyValueExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.KeyValueExpression;
    public SyntaxToken OpenBracketToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Key { get; init; }
    public SyntaxToken CommaToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Value { get; init; }
    public SyntaxToken CloseBracketToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenBracketToken, Key, CommaToken, Value, CloseBracketToken);
}

internal record RangeExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.RangeExpression;
    public ExpressionSyntax Start { get; init; }
    public SyntaxToken DoubleDotToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax End { get; init; }
    public bool IsOpenStart => Start is null;
    public bool IsOpenEnd => End is null;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Start, DoubleDotToken, End);
}

internal record RangeIndexExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.RangeIndexExpression;
    public SyntaxToken CaretToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Index { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(CaretToken, Index);
}

internal record LambdaExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.LambdaExpression;
    public ParameterListSyntax Parameters { get; init; }
    public SyntaxToken ColonToken { get; init; }
    public TypeSyntax ReturnType { get; init; }
    public SyntaxToken ArrowToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Body { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Parameters, ColonToken, ReturnType, ArrowToken, Body);
}

internal record FunctionPointerExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.FunctionPointerExpression;
    public SyntaxToken FuncKeyword { get; init; } = SyntaxToken.None;
    public FunctionPointerParameterListSyntax Parameters { get; init; } = new();
    public ExpressionSyntax Target { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(FuncKeyword, Parameters, Target);
}

internal record FunctionPointerParameterListSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.FunctionPointerParameterList;
    public SyntaxToken OpenParenToken { get; init; }
    public SeparatedSyntaxList<TypeSyntax> ParameterTypes { get; init; } = new();
    public SyntaxToken CloseParenToken { get; init; }
    public SyntaxToken ColonToken { get; init; }
    public TypeSyntax ReturnType { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OpenParenToken, ParameterTypes, CloseParenToken, ColonToken, ReturnType);
}

internal record ConversionExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ConversionExpression;
    public ExpressionSyntax Expression { get; init; }
    public SyntaxToken OperatorToken { get; init; } = SyntaxToken.None;
    public TypeSyntax Type { get; init; }
    public bool IsSafe => OperatorToken.Kind == SyntaxKind.LessThanQuestionMarkColonToken;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Expression, OperatorToken, Type);
}

internal record TypeTestExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.TypeTestExpression;
    public ExpressionSyntax Expression { get; init; }
    public SyntaxToken OperatorToken { get; init; } = SyntaxToken.None;
    public TypeSyntax Type { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Expression, OperatorToken, Type);
}

internal record AttributedExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.AttributedExpression;
    public AttributeListSyntax Attributes { get; init; } = new();
    public ExpressionSyntax Expression { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Attributes, Expression);
}

internal record SpecialSymbolExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.SpecialSymbolExpression;
    public SpecialSymbolSyntax SpecialSymbol { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(SpecialSymbol);
}

internal record TerminatedExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.TerminatedExpression;
    public ExpressionSyntax Expression { get; init; }
    public SyntaxToken TerminatorToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Expression, TerminatorToken);
}

internal abstract record PredicateSyntax : SyntaxNode;

internal record NamePredicateSyntax : PredicateSyntax
{
    public override SyntaxKind Kind => SyntaxKind.NamePredicate;
    public NameSyntax Name { get; init; }
    public SyntaxToken ExclamationToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Name, ExclamationToken);
}

internal record BinaryPredicateSyntax : PredicateSyntax
{
    public override SyntaxKind Kind => SyntaxKind.BinaryPredicate;
    public PredicateSyntax Left { get; init; }
    public SyntaxToken OperatorToken { get; init; } = SyntaxToken.None;
    public PredicateSyntax Right { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Left, OperatorToken, Right);
}

internal record RelationalPredicateSyntax : PredicateSyntax
{
    public override SyntaxKind Kind => SyntaxKind.RelationalPredicate;
    public SyntaxToken OperatorToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Expression { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OperatorToken, Expression);
}

internal record UnaryPredicateSyntax : PredicateSyntax
{
    public override SyntaxKind Kind => SyntaxKind.UnaryPredicate;
    public SyntaxToken OperatorToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Expression { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OperatorToken, Expression);
}

internal record CustomOperatorPredicateSyntax : PredicateSyntax
{
    public override SyntaxKind Kind => SyntaxKind.CustomOperatorPredicate;
    public SyntaxToken OperatorToken { get; init; } = SyntaxToken.None;
    public ExpressionSyntax Expression { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(OperatorToken, Expression);
}

internal abstract record PatternSyntax : SyntaxNode;

internal record ExpressionPatternSyntax : PatternSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ExpressionPattern;
    public ExpressionSyntax Expression { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Expression);
}

internal record DiscardPatternSyntax : PatternSyntax
{
    public override SyntaxKind Kind => SyntaxKind.DiscardPattern;
    public SyntaxToken UnderscoreToken { get; init; } = SyntaxToken.None;

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(UnderscoreToken);
}

internal record ErrorSyntaxNode : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.Error;
    public IReadOnlyList<SyntaxToken> Tokens { get; init; } = [];
    public IReadOnlyList<SyntaxNode> Children { get; init; } = [];

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Tokens, Children);
}

internal record SkippedTokensTriviaSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.SkippedTokensTrivia;
    public IReadOnlyList<SyntaxToken> Tokens { get; init; } = [];

    public override IEnumerable<SyntaxNode> GetChildren() => EnumerateChildren(Tokens);
}
