using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace Dassie.Generators;

[Generator]
public sealed class ConfigPropertyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<IPropertySymbol> properties = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is PropertyDeclarationSyntax p && p.AttributeLists.Count > 0,
                static (ctx, _) => GetPropertySymbol(ctx))
            .Where(static p => p is not null)!;

        context.RegisterSourceOutput(properties, static (spc, property) =>
        {
            if (property is null)
                return;

            if (!IsConfigProperty(property))
                return;

            if (!IsInConfigObjectHierarchy(property.ContainingType))
                return;

            if (!HasPartialDefinitionWithoutBody(property))
                return;

            if (property.IsStatic)
                return;

            string source = GeneratePropertyImplementation(property);
            string hintName = $"{property.ContainingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat).Replace('<', '_').Replace('>', '_').Replace('.', '_')}.{property.Name}.ConfigProperty.g.cs";
            spc.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        });
    }

    private static IPropertySymbol GetPropertySymbol(GeneratorSyntaxContext context)
    {
        if (context.Node is not PropertyDeclarationSyntax p)
            return null;

        return context.SemanticModel.GetDeclaredSymbol(p);
    }

    private static bool IsConfigProperty(IPropertySymbol property)
    {
        return property.GetAttributes().Any(a => a.AttributeClass?.Name == "ConfigPropertyAttribute");
    }

    private static bool IsInConfigObjectHierarchy(INamedTypeSymbol type)
    {
        INamedTypeSymbol current = type;
        while (current is not null)
        {
            if (current.ToDisplayString() == "Dassie.Configuration.ConfigObject")
                return true;

            current = current.BaseType;
        }

        return false;
    }

    private static bool HasPartialDefinitionWithoutBody(IPropertySymbol property)
    {
        foreach (SyntaxReference syntaxRef in property.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is not PropertyDeclarationSyntax decl)
                continue;

            if (!decl.Modifiers.Any(SyntaxKind.PartialKeyword))
                continue;

            if (decl.AccessorList is null)
                continue;

            bool hasBody = decl.ExpressionBody is not null
                || decl.AccessorList.Accessors.Any(a => a.Body is not null || a.ExpressionBody is not null);

            if (!hasBody)
                return true;
        }

        return false;
    }

    private static string GeneratePropertyImplementation(IPropertySymbol property)
    {
        StringBuilder sb = new();

        if (!property.ContainingNamespace.IsGlobalNamespace)
        {
            sb.Append("namespace ").Append(property.ContainingNamespace.ToDisplayString()).AppendLine(";");
            sb.AppendLine();
        }

        AppendContainingTypesOpen(sb, property.ContainingType);

        string accessibility = AccessibilityToCode(property.DeclaredAccessibility);
        string typeName = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string propertyName = EscapeIdentifier(property.Name);

        sb.Append("    ").Append(accessibility).Append(" partial ").Append(typeName).Append(' ').Append(propertyName).AppendLine();
        sb.AppendLine("    {");

        if (property.GetMethod is not null)
            sb.Append("        get => Get<").Append(typeName).Append(">(nameof(").Append(propertyName).AppendLine("));");

        if (property.SetMethod is not null)
            sb.Append("        set => Set(nameof(").Append(propertyName).AppendLine("), value);");

        sb.AppendLine("    }");

        AppendContainingTypesClose(sb, property.ContainingType);

        return sb.ToString();
    }

    private static void AppendContainingTypesOpen(StringBuilder sb, INamedTypeSymbol type)
    {
        if (type.ContainingType is not null)
            AppendContainingTypesOpen(sb, type.ContainingType);

        string accessibility = AccessibilityToCode(type.DeclaredAccessibility);
        string kind = type.TypeKind == TypeKind.Struct ? "struct" : "class";
        string typeName = type.Name;

        sb.Append(accessibility).Append(" partial ").Append(kind).Append(' ').Append(typeName).AppendLine();
        sb.AppendLine("{");
    }

    private static void AppendContainingTypesClose(StringBuilder sb, INamedTypeSymbol type)
    {
        sb.AppendLine("}");
        if (type.ContainingType is not null)
            AppendContainingTypesClose(sb, type.ContainingType);
    }

    private static string AccessibilityToCode(Accessibility accessibility)
        => accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "private"
        };

    private static string EscapeIdentifier(string name)
    {
        return SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;
    }
}