using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Shared.Events;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Bearded.TD.Analyzers.SourceGenerators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
[UsedImplicitly]
sealed partial class Analyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        TD103AttributeIsNotInPartialClass.Rule
    );

    private static readonly ImmutableHashSet<string> attributeTypes = ImmutableHashSet.Create(
        nameof(HandlerAttribute)
    );

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(analyze, SymbolKind.Field, SymbolKind.Method, SymbolKind.Property);
    }

    private static void analyze(SymbolAnalysisContext context)
    {
        var symbol = context.Symbol;
        var attributes = symbol.GetAttributes();
        var attribute = attributes
            .FirstOrDefault(static a =>
            {
                var name = a.AttributeClass?.Name;
                return name is not null && attributeTypes.Contains(name);
            });
        if (attribute is null)
        {
            return;
        }

        TD103AttributeIsNotInPartialClass.Check(context, symbol.ContainingType, attribute.AttributeClass!);
    }
}
