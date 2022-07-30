using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Bearded.TD.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
[UsedImplicitly]
sealed class NoGameInComponentOnAdded : DiagnosticAnalyzer
{
    private const string td001Identifier = "TD001";
    private const string bannedPropertyId = "P:Bearded.TD.Game.Simulation.GameObjects.GameObject.Game";

    private static readonly ImmutableArray<string> bannedScopeIds = ImmutableArray.Create(
        "M:Bearded.TD.Game.Simulation.GameObjects.Component.OnAdded",
        "M:Bearded.TD.Game.Simulation.GameObjects.Component`1.OnAdded");

    private static DiagnosticDescriptor gameInOnAddedIsBannedRule { get; } = new(
        id: td001Identifier,
        title: "Access to game in OnAdded is banned",
        messageFormat:
        "Access to 'Owner.Game' is banned in the 'OnAdded' method of components, but was accessed in {0}",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Access to 'Owner.Game' has been banned in the 'OnAdded' method of components. Use 'Activate' instead."
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(gameInOnAddedIsBannedRule);

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
        var bannedSymbol = getBannedSymbol(compilationContext.Compilation);
        var scopes = getScopes(compilationContext.Compilation);

        compilationContext.RegisterOperationBlockStartAction(blockStartAction =>
        {
            if (!isBannedScope(scopes, blockStartAction.OwningSymbol))
            {
                return;
            }

            blockStartAction.RegisterOperationAction(context =>
            {
                var propertyReference = (IPropertyReferenceOperation) context.Operation;
                if (!SymbolEqualityComparer.Default.Equals(propertyReference.Property, bannedSymbol))
                {
                    return;
                }

                var diagnostic = Diagnostic.Create(
                    gameInOnAddedIsBannedRule,
                    context.Operation.Syntax.GetLocation(),
                    context.ContainingSymbol.ContainingType);
                context.ReportDiagnostic(diagnostic);
            }, OperationKind.PropertyReference);
        });
    }

    private static bool isBannedScope(ImmutableArray<ISymbol> scopes, ISymbol symbol)
    {
        return symbol is IMethodSymbol method
            && scopes.Any(scope => SymbolEqualityComparer.Default.Equals(toRootOverriddenMethod(method), scope));
    }

    private static IMethodSymbol toRootOverriddenMethod(IMethodSymbol methodSymbol)
    {
        while (methodSymbol.OverriddenMethod != null)
        {
            methodSymbol = methodSymbol.OverriddenMethod;
        }
        return methodSymbol;
    }

    private static ISymbol getBannedSymbol(Compilation compilation)
    {
        if (tryFindSingleSymbol(bannedPropertyId, compilation, out var symbol))
        {
            return symbol;
        }
        throw new Exception("Banned symbol should match exactly one symbol");
    }

    private static ImmutableArray<ISymbol> getScopes(Compilation compilation)
    {
        return bannedScopeIds
            .Select(id =>
            {
                if (tryFindSingleSymbol(id, compilation, out var symbol) && symbol is IMethodSymbol)
                {
                    return symbol;
                }
                throw new Exception("Scope should match exactly one method symbol");
            })
            .ToImmutableArray();
    }

    private static bool tryFindSingleSymbol(
        string documentationCommentId, Compilation compilation, [NotNullWhen(true)] out ISymbol? symbol)
    {
        var symbols = DocumentationCommentId.GetSymbolsForDeclarationId(documentationCommentId, compilation);
        if (symbols.Length == 1)
        {
            symbol = symbols[0];
            return true;
        }

        symbol = default;
        return false;
    }
}
