using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bearded.TD.Generators;

static class Symbols
{
    public static INamedTypeSymbol ResolveNamedTypeSymbol(this Compilation compilation, TypeDeclarationSyntax node) =>
        compilation.resolveSymbol<INamedTypeSymbol>(node);

    public static IFieldSymbol ResolveFieldSymbol(this Compilation compilation, FieldDeclarationSyntax node)
    {
        var model = compilation.GetSemanticModel(node.SyntaxTree);
        return node.Declaration.Variables
                .Select(v => model.GetDeclaredSymbol(v) as IFieldSymbol)
                .SingleOrDefault(symbol => symbol is not null)
            ?? throw new InvalidCastException($"Could not cast field, found none or multiple declarations.");
    }

    private static T resolveSymbol<T>(this Compilation compilation, SyntaxNode node)
        where T : class, ISymbol
    {
        var model = compilation.GetSemanticModel(node.SyntaxTree);
        var symbol = model.GetDeclaredSymbol(node);
        return symbol as T ?? throw new InvalidCastException($"Could not cast {symbol}, expected a {typeof(T)}.");
    }
}
