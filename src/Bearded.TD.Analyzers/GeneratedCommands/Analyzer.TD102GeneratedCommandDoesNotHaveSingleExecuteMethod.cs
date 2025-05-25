using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Bearded.TD.Analyzers.GeneratedCommands;

sealed partial class Analyzer
{
    internal static class TD102GeneratedCommandDoesNotHaveSingleExecuteMethod
    {
        internal const string Identifier = "TD102";

        private const string requiredMethodName = "execute";

        public static DiagnosticDescriptor Rule { get; } = new(
            id: Identifier,
            title: $"Generated command must have single {requiredMethodName} method",
            messageFormat:
            $"{{0}} must have exactly one '{requiredMethodName}' method, but has {{1}}.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description:
            $"Types annotated with [{generateCommandAttributeType}] must have exactly one '{requiredMethodName}' method."
        );

        public static void Check(SymbolAnalysisContext context, INamedTypeSymbol commandType)
        {
            var executeMethodCount = commandType
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Count(m => m.Name == requiredMethodName);

            if (executeMethodCount == 1)
                return;

            var diagnostic = Diagnostic.Create(Rule, commandType.Locations[0], commandType.Name, executeMethodCount);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
