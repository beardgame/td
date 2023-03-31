using System;
using Microsoft.CodeAnalysis;

namespace Bearded.TD.Generators;

static class Symbols
{
    public static INamedTypeSymbol ResolveSymbol(this Compilation compilation, SyntaxNode typeSyntax)
    {
        var classModel = compilation.GetSemanticModel(typeSyntax.SyntaxTree);
        return classModel.GetDeclaredSymbol(typeSyntax) as INamedTypeSymbol
            ?? throw new InvalidCastException("Expected a named type");
    }
}
