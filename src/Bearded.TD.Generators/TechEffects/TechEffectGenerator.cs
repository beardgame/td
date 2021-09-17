using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Bearded.TD.Generators.DiagnosticFactory;

namespace Bearded.TD.Generators.TechEffects
{
    [Generator]
    [UsedImplicitly]
    public sealed class TechEffectGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
// #if DEBUG
//             if (!Debugger.IsAttached)
//             {
//                 Debugger.Launch();
//             }
// #endif

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                {
                    return;
                }

                var convertsAttribute = context.Compilation
                        .GetTypeByMetadataName("Bearded.TD.Shared.TechEffects.ConvertsAttributeAttribute") ??
                    throw new InvalidOperationException("Could not find converts attribute attribute.");
                var attributeConverterType = context.Compilation
                        .GetTypeByMetadataName("Bearded.TD.Shared.TechEffects.AttributeConverter`1")?
                        .ConstructUnboundGenericType() ??
                    throw new InvalidOperationException("Could not find attribute converter class.");
                var attributeConverters = findFieldsWithAnnotation(
                        context.Compilation, receiver.AttributeConverterCandidates, convertsAttribute)
                    .Select(a => (Attribute: a, Type: extractAttributeConverterType(a, attributeConverterType)))
                    .Where(tuple => tuple.Type != null)
                    .ToImmutableDictionary(tuple => tuple.Type!, tuple => tuple.Attribute);

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
                    var definition = ParametersTemplateDefinition.FromNamedSymbol(
                        namedTypeSymbol, attributeInterface, attributeConverters);
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
                context.ReportDiagnostic(CreateDebugDiagnostic(e.ToString(), DiagnosticSeverity.Error));
            }
        }

        private static IEnumerable<IFieldSymbol> findFieldsWithAnnotation(
            Compilation compilation, IEnumerable<FieldDeclarationSyntax> candidates, ISymbol target)
        {
            foreach (var fieldSyntax in candidates)
            {
                var fieldModel = compilation.GetSemanticModel(fieldSyntax.SyntaxTree);
                var fieldSymbolsWithAttribute = fieldSyntax.Declaration.Variables
                    .Select(v => fieldModel.GetDeclaredSymbol(v) as IFieldSymbol)
                    .Where(symbol => symbol != null)
                    .Where(symbol =>
                        symbol?.GetAttributes().Any(a =>
                            a.AttributeClass?.Equals(target, SymbolEqualityComparer.Default) ?? false) ?? false)
                    .Select(symbol => symbol!);

                foreach (var symbol in fieldSymbolsWithAttribute)
                {
                    yield return symbol;
                }
            }
        }

        private static ITypeSymbol? extractAttributeConverterType(
            IFieldSymbol fieldSymbol, ISymbol attributeConverterSymbol)
        {
            var fieldType = fieldSymbol.Type as INamedTypeSymbol ??
                throw new InvalidOperationException("Field type must be a named type.");
            return fieldType.ConstructUnboundGenericType()
                .Equals(attributeConverterSymbol, SymbolEqualityComparer.Default)
                ? fieldType.TypeArguments[0]
                : null;
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
            public List<InterfaceDeclarationSyntax> Interfaces { get; } = new();

            public List<FieldDeclarationSyntax> AttributeConverterCandidates { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                switch (syntaxNode)
                {
                    case InterfaceDeclarationSyntax interfaceSyntax:
                        Interfaces.Add(interfaceSyntax);
                        break;
                    case FieldDeclarationSyntax fieldSyntax:
                        if (fieldSyntax.AttributeLists.Any())
                        {
                            AttributeConverterCandidates.Add(fieldSyntax);
                        }

                        break;
                }
            }
        }
    }
}
