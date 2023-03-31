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
        foreach (var attribute in syntax.AttributeLists.SelectMany(attributeList => attributeList.Attributes))
        {
            if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol)
            {
                continue;
            }

            var attributeTypeSymbol = attributeSymbol.ContainingType;
            if (attributeTypeSymbol.Equals(desiredAttributeTypeSymbol, SymbolEqualityComparer.Default))
            {
                return syntax;
            }
        }

        return null;
    }
}
