using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Bearded.TD.Analyzers.GeneratedCommands;

sealed partial class Analyzer
{
    internal static class TD101GeneratedCommandIsNotStaticPartialClass
    {
        internal const string Identifier = "TD101";

        public static DiagnosticDescriptor Rule { get; } = new(
            id: Identifier,
            title: "Generated command must be a static partial class",
            messageFormat:
            "{0} must be a static partial class for command to generate",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description:
            $"Types annotated with [{generateCommandAttributeType}] must be static partial classes."
        );

        public static void Check(SymbolAnalysisContext context, INamedTypeSymbol commandType)
        {
            if (commandType.IsStatic && commandType.IsPartial())
                return;

            var diagnostic = Diagnostic.Create(Rule, commandType.Locations[0], commandType.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
