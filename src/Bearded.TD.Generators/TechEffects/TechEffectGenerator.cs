using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bearded.TD.Generators.TechEffects
{
    [Generator]
    public sealed class TechEffectGenerator : ISourceGenerator
    {
        public void Initialize(InitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(SourceGeneratorContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
            {
                return;
            }

            var templateInterface = context.Compilation
                .GetTypeByMetadataName("Bearded.TD.Shared.TechEffects.IParametersTemplate`1")?
                .ConstructUnboundGenericType();
            var interfacesToGenerateFor =
                findSymbolsImplementingInterface(context.Compilation, receiver.Interfaces, templateInterface);
        }

        private IEnumerable<INamedTypeSymbol> findSymbolsImplementingInterface(
            Compilation compilation, IEnumerable<InterfaceDeclarationSyntax> candidates, INamedTypeSymbol target)
        {
            foreach (var interfaceSyntax in candidates)
            {
                var classModel = compilation.GetSemanticModel(interfaceSyntax.SyntaxTree);
                var classSymbol = classModel.GetDeclaredSymbol(interfaceSyntax) as INamedTypeSymbol
                    ?? throw new InvalidCastException("Expected a named type");

                var candidateInterfaces = classSymbol.Interfaces
                    .Where(i => i.IsGenericType)
                    .Select(i => i.ConstructUnboundGenericType());

                if (candidateInterfaces.Any(i => i.Equals(target, SymbolEqualityComparer.Default)))
                {
                    yield return classSymbol;
                }
            }
        }

        private sealed class SyntaxReceiver : ISyntaxReceiver
        {
            public List<InterfaceDeclarationSyntax> Interfaces { get; } = new List<InterfaceDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is InterfaceDeclarationSyntax interfaceSyntax)
                {
                    Interfaces.Add(interfaceSyntax);
                }
            }
        }

        // private static Diagnostic fakeDebugDiagnostic(string text)
        // {
        //     return Diagnostic.Create(
        //         new DiagnosticDescriptor("TD1", "Debug message", text, "Debug", DiagnosticSeverity.Warning, true),
        //         null);
        // }
    }
}
