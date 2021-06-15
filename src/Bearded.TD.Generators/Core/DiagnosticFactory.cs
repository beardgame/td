using Microsoft.CodeAnalysis;

namespace Bearded.TD.Generators
{
    static class DiagnosticFactory
    {
        public static Diagnostic CreateDebugDiagnostic(
            string text, DiagnosticSeverity severity = DiagnosticSeverity.Warning)
        {
            return Diagnostic.Create(
                new DiagnosticDescriptor("TD1", "Debug message", text, "Debug", severity, true),
                null);
        }
    }
}
