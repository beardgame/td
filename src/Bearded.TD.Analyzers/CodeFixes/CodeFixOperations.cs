using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bearded.TD.Analyzers.CodeFixes;

static class CodeFixOperations
{
    public static async Task<Document> WithReplacedNode<TNode>(
        this Document document, TNode existingNode, TNode newNode, CancellationToken cancellationToken)
        where TNode : SyntaxNode
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root == null)
        {
            return document;
        }

        var newRoot = root.ReplaceNode(existingNode, newNode);
        return document.WithSyntaxRoot(newRoot);
    }

    public static ClassDeclarationSyntax WithAdditionalModifiers(
        this ClassDeclarationSyntax classDeclaration,
        params SyntaxKind[] modifiers)
    {
        var classModifiers = classDeclaration.Modifiers;

        foreach (var modifier in modifiers)
        {
            if (!classModifiers.Any(m => m.IsKind(modifier)))
            {
                classModifiers = classModifiers.Add(SyntaxFactory.Token(modifier));
            }
        }

        return classDeclaration.WithModifiers(classModifiers);
    }
}
