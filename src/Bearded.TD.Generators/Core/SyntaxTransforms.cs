using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bearded.TD.Generators;

static class SyntaxTransforms
{
    public static InterfaceDeclarationSyntax? AsInterfaceWithAttribute(
        GeneratorSyntaxContext context, string fullAttributeName) =>
        asTypeWithAttribute<InterfaceDeclarationSyntax>(context, fullAttributeName);

    public static ClassDeclarationSyntax? AsClassWithAttribute(
        GeneratorSyntaxContext context, string fullAttributeName) =>
        asTypeWithAttribute<ClassDeclarationSyntax>(context, fullAttributeName);

    private static T? asTypeWithAttribute<T>(GeneratorSyntaxContext context, string fullAttributeName)
        where T : TypeDeclarationSyntax
    {
        var desiredAttributeTypeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(fullAttributeName) ??
            throw new InvalidOperationException($"Could not find attribute with name {fullAttributeName}");

        var syntax = (T) context.Node;
        return syntax.AttributeLists.containsAttribute(desiredAttributeTypeSymbol, context.SemanticModel)
            ? syntax
            : null;
    }

    public static FieldDeclarationSyntax? AsFieldWithAttribute(GeneratorSyntaxContext context, string fullAttributeName)
    {
        var desiredAttributeTypeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(fullAttributeName) ??
            throw new InvalidOperationException($"Could not find attribute with name {fullAttributeName}");

        var syntax = (FieldDeclarationSyntax) context.Node;
        return syntax.AttributeLists.containsAttribute(desiredAttributeTypeSymbol, context.SemanticModel)
            ? syntax
            : null;
    }

    private static bool containsAttribute(
        this SyntaxList<AttributeListSyntax> attributeLists,
        ISymbol desiredAttributeTypeSymbol,
        SemanticModel semanticModel)
    {
        foreach (var attribute in attributeLists.SelectMany(attributeList => attributeList.Attributes))
        {
            if (semanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol)
            {
                continue;
            }

            var attributeTypeSymbol = attributeSymbol.ContainingType;
            if (attributeTypeSymbol.Equals(desiredAttributeTypeSymbol, SymbolEqualityComparer.Default))
            {
                return true;
            }
        }

        return false;
    }
}
