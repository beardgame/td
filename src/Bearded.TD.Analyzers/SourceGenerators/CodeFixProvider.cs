using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bearded.TD.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Bearded.TD.Analyzers.SourceGenerators.Analyzer;

namespace Bearded.TD.Analyzers.SourceGenerators;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixProvider)), Shared]
public sealed class CodeFixProvider : Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(
            TD103AttributeIsNotInPartialClass.Identifier
        );

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
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

        if (context.Diagnostics.Any(d => d.Id == TD103AttributeIsNotInPartialClass.Identifier))
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Make class partial",
                    c => makePartialAsync(context.Document, classDeclaration, c),
                    nameof(CodeFixProvider)
                ),
                diagnostic
            );
        }
    }

    private static async Task<Document> makePartialAsync(
        Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
    {
        var newClassDeclaration = classDeclaration.WithAdditionalModifiers(SyntaxKind.PartialKeyword);

        return await document.WithReplacedNode(classDeclaration, newClassDeclaration, cancellationToken);
    }
}
