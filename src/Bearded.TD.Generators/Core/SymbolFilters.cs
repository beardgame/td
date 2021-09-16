using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bearded.TD.Generators
{
    static class SymbolFilters
    {
        public static IEnumerable<INamedTypeSymbol> FindSymbolsWithAttribute(
            Compilation compilation, IEnumerable<TypeDeclarationSyntax> types, ISymbol attribute)
        {
            foreach (var typeSyntax in types)
            {
                var classModel = compilation.GetSemanticModel(typeSyntax.SyntaxTree);
                var classSymbol = classModel.GetDeclaredSymbol(typeSyntax) as INamedTypeSymbol
                    ?? throw new InvalidCastException("Expected a named type");

                if (classSymbol.GetAttributes().Any(
                    a => a.AttributeClass?.Equals(attribute, SymbolEqualityComparer.Default) ?? false))
                {
                    yield return classSymbol;
                }
            }
        }
    }
}
