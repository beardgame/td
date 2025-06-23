using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Bearded.TD.Analyzers.SourceGenerators;

sealed partial class Analyzer
{
    internal static class TD103AttributeIsNotInPartialClass
    {
        internal const string Identifier = "TD103";

        public static DiagnosticDescriptor Rule { get; } = new(
            id: Identifier,
            title: "Attribute must be used inside a partial class",
            messageFormat: "{0} must be used inside a partial class",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Types annotated with source generator annotations that generate parts of the class must be" +
            "contained within a partial class."
        );

        public static void Check(
            SymbolAnalysisContext context, INamedTypeSymbol containingType, INamedTypeSymbol attributeType)
        {
            if (containingType.IsPartial()) return;

            var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], attributeType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
