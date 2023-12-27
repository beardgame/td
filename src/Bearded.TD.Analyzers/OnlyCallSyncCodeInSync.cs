using System;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Bearded.TD.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
[UsedImplicitly]
sealed class OnlyCallSyncCodeInSync : DiagnosticAnalyzer
{
    private const string td002Identifier = "TD002";

    private static DiagnosticDescriptor syncFromNonSyncRule { get; } = new(
        id: td002Identifier,
        title: "MustBeSync code called outside of synchronous context",
        messageFormat:
        "Non-sync method {0} called {1} which must be sync",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Methods that are marked as MustBeSync may only be called from other methods that are called MustBeSync or IsSync."
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(syncFromNonSyncRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();

        // Analyzer needs to get callbacks for generated code, and might report diagnostics in generated code.
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(onCompilationStart);
    }

    private static void onCompilationStart(CompilationStartAnalysisContext compilationContext)
    {
        Diagnostic.Create("TD901", "usage", "comp start", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true,
            1);

        var isSyncAttr =
            compilationContext.Compilation.GetTypeByMetadataName("Bearded.TD.Shared.Annotations.IsSyncAttribute") ??
            throw new Exception();
        var mustBeSyncAttr =
            compilationContext.Compilation.GetTypeByMetadataName("Bearded.TD.Shared.Annotations.MustBeSyncAttribute") ??
            throw new Exception();

        compilationContext.RegisterOperationAction(context =>
        {
            var methodReference = (IMethodReferenceOperation) context.Operation;
            var attributes = methodReference.Method.GetAttributes();
            Diagnostic.Create("TD902", "usage", $"{methodReference.Method} ref found", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true,
                1);
            if (!attributes.Any(a => SymbolEqualityComparer.Default.Equals(mustBeSyncAttr, a.AttributeClass)))
            {
                return;
            }

            var curr = context.ContainingSymbol;
            while (curr is not IMethodSymbol && curr.ContainingSymbol != null)
            {
                curr = curr.ContainingSymbol;
            }

            Console.WriteLine($"Found call to sync method at {curr}");

            var isSync = curr is IMethodSymbol method && method.GetAttributes().Any(a =>
                SymbolEqualityComparer.Default.Equals(mustBeSyncAttr, a.AttributeClass) ||
                SymbolEqualityComparer.Default.Equals(isSyncAttr, a.AttributeClass));

            if (isSync)
            {
                return;
            }

            var diagnostic = Diagnostic.Create(
                syncFromNonSyncRule,
                context.Operation.Syntax.GetLocation(),
                context.ContainingSymbol,
                methodReference.Method);
            context.ReportDiagnostic(diagnostic);
        }, OperationKind.MethodReference);
    }
}
