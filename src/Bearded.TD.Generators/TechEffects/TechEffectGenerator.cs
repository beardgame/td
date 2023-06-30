using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Bearded.TD.Generators.DiagnosticFactory;

namespace Bearded.TD.Generators.TechEffects
{
    [Generator]
    [UsedImplicitly]
    public sealed class TechEffectGenerator : IIncrementalGenerator
    {
        private const string convertsAttributeFullName = "Bearded.TD.Shared.TechEffects.ConvertsAttributeAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
// #if DEBUG
//             if (!Debugger.IsAttached)
//             {
//                 Debugger.Launch();
//             }
// #endif
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var convertsAttributeFields =
                SyntaxProviders.FieldsWithAttribute(context.SyntaxProvider, convertsAttributeFullName);
            var interfaces = SyntaxProviders.Interfaces(context.SyntaxProvider);

            var combined = context.CompilationProvider
                .Combine(convertsAttributeFields.Collect())
                .Combine(interfaces.Collect());

            context.RegisterSourceOutput(
                combined, (spc, source) => execute(source.Left.Left, source.Left.Right, source.Right, spc));
        }

        private static void execute(
            Compilation compilation,
            ImmutableArray<FieldDeclarationSyntax> convertsAttributeFields,
            ImmutableArray<InterfaceDeclarationSyntax> allInterfaces,
            SourceProductionContext context)
        {
            try
            {
                var attributeConverters = attributeConverterDictionary(
                    compilation, convertsAttributeFields.Select(compilation.ResolveFieldSymbol));

                var templateInterface = compilation
                        .GetTypeByMetadataName("Bearded.TD.Shared.TechEffects.IParametersTemplate`1")?
                        .ConstructUnboundGenericType() ??
                    throw new InvalidOperationException("Could not find parameters template interface.");
                var interfacesToGenerateFor =
                    findSymbolsImplementingInterface(compilation, allInterfaces, templateInterface);

                var attributeInterface = compilation
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

        private static ImmutableDictionary<ITypeSymbol, IFieldSymbol> attributeConverterDictionary(
            Compilation compilation, IEnumerable<IFieldSymbol> attributes)
        {
            var attributeConverterType = compilation
                    .GetTypeByMetadataName("Bearded.TD.Shared.TechEffects.AttributeConverter`1")?
                    .ConstructUnboundGenericType() ??
                throw new InvalidOperationException("Could not find attribute converter class.");
            var dict = new Dictionary<ITypeSymbol, IFieldSymbol>(SymbolEqualityComparer.Default);

            foreach (var a in attributes)
            {
                var converterType = extractAttributeConverterType(a, attributeConverterType);
                if (converterType != null)
                {
                    dict.Add(converterType, a);
                }
            }

            return dict.ToImmutableDictionary(SymbolEqualityComparer.Default);
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
    }
}
