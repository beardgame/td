using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Bearded.TD.Analyzers.GeneratedCommands.Analyzer;

namespace Bearded.TD.Analyzers.GeneratedCommands;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixProvider)), Shared]
public class CodeFixProvider : Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(
            TD101GeneratedCommandIsNotStaticPartialClass.Identifier,
            TD102GeneratedCommandDoesNotHaveSingleExecuteMethod.Identifier
        );

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var classDeclaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>().FirstOrDefault();

        if (classDeclaration == null)
            return;

        if (context.Diagnostics.Any(d => d.Id == TD101GeneratedCommandIsNotStaticPartialClass.Identifier))
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Make class static partial",
                    c => makeStaticPartialAsync(context.Document, classDeclaration, c),
                    nameof(CodeFixProvider)
                ),
                diagnostic
            );
        }

        if (context.Diagnostics.Any(d => d.Id == TD102GeneratedCommandDoesNotHaveSingleExecuteMethod.Identifier))
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Add execute method",
                    c => addExecuteMethod(context.Document, classDeclaration, c),
                    nameof(CodeFixProvider)
                ),
                diagnostic
            );
        }
    }

    private static async Task<Document> makeStaticPartialAsync(
        Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);

        if (root == null)
            return document;

        var modifiers = classDeclaration.Modifiers;

        if (!modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

        if (!modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

        var newClassDeclaration = classDeclaration.WithModifiers(modifiers);

        var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }


    private static async Task<Document> addExecuteMethod(
        Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);

        if (root == null)
            return document;

        var executeMethod = SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier("execute")
            )
            .WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword)
            ))
            .WithBody(SyntaxFactory.Block());

        var newClassDeclaration = classDeclaration.AddMembers(executeMethod);

        var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }
}
