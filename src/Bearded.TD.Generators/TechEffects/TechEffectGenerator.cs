using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Bearded.TD.Generators.TechEffects
{
    [Generator]
    [UsedImplicitly]
    public sealed class TechEffectGenerator : ISourceGenerator
    {
        public void Initialize(InitializationContext context)
        {
// #if DEBUG
//             if (!Debugger.IsAttached)
//             {
//                 Debugger.Launch();
//             }
// #endif

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(SourceGeneratorContext context)
        {
            try
            {
                if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
                {
                    return;
                }

                var templateInterface = context.Compilation
                        .GetTypeByMetadataName("Bearded.TD.Shared.TechEffects.IParametersTemplate`1")?
                        .ConstructUnboundGenericType() ??
                    throw new InvalidOperationException("Could not find parameters template interface.");
                var interfacesToGenerateFor =
                    findSymbolsImplementingInterface(context.Compilation, receiver.Interfaces, templateInterface);

                var attributeInterface = context.Compilation
                        .GetTypeByMetadataName("Bearded.TD.Shared.TechEffects.ModifiableAttribute") ??
                    throw new InvalidOperationException("Could not find modifiable attribute.");
                foreach (var namedTypeSymbol in interfacesToGenerateFor)
                {
                    var definition = ParametersTemplateDefinition.FromNamedSymbol(namedTypeSymbol, attributeInterface);
                    context.AddSource(
                        definition.ModifiableName,
                        SourceText.From(ModifiableSourceGenerator.GenerateFor(definition), Encoding.Default));
                    context.AddSource(
                        definition.TemplateName,
                        SourceText.From(TemplateSourceGenerator.GenerateFor(definition), Encoding.Default));
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(fakeDebugDiagnostic(e.ToString(), DiagnosticSeverity.Error));
            }
        }

        private static IEnumerable<INamedTypeSymbol> findSymbolsImplementingInterface(
            Compilation compilation, IEnumerable<InterfaceDeclarationSyntax> candidates, ISymbol target)
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

        private static Diagnostic fakeDebugDiagnostic(
            string text, DiagnosticSeverity severity = DiagnosticSeverity.Warning)
        {
            return Diagnostic.Create(
                new DiagnosticDescriptor("TD1", "Debug message", text, "Debug", severity, true),
                null);
        }
    }
}
