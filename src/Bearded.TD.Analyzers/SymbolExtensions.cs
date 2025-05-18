using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bearded.TD.Analyzers;

public static class SymbolExtensions
{
    public static bool IsPartial(this INamedTypeSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Any(syntax =>
            syntax.GetSyntax() is BaseTypeDeclarationSyntax declaration
            && declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))
        );
    }
}
