using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Bearded.TD.Generators.DiagnosticFactory;

namespace Bearded.TD.Generators.Proxies
{
    [Generator]
    [UsedImplicitly]
    public sealed class ProxyGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
// #if DEBUG
//             if (!Debugger.IsAttached)
//             {
//                 Debugger.Launch();
//             }
// #endif

            var interfacesToGenerateFor = context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: (node, _) => node is InterfaceDeclarationSyntax { AttributeLists.Count: > 0 },
                    transform: (ctx, _) => toInterfaceToGenerateFor(ctx))
                .Where(i => i is not null)
                .Select((i, _) => i!);

            var interfacesWithCompilation = context.CompilationProvider.Combine(interfacesToGenerateFor.Collect());

            context.RegisterSourceOutput(
                interfacesWithCompilation, (spc, source) => execute(source.Left, source.Right, spc));
        }

        private static InterfaceDeclarationSyntax? toInterfaceToGenerateFor(GeneratorSyntaxContext ctx)
        {
            var automaticProxyAttribute = ctx.SemanticModel.Compilation.GetTypeByMetadataName(
                "Bearded.TD.Shared.Proxies.AutomaticProxyAttribute");

            // Predicate already filters on type.
            var syntax = (InterfaceDeclarationSyntax) ctx.Node;
            foreach (var attribute in syntax.AttributeLists.SelectMany(attributeList => attributeList.Attributes))
            {
                if (ctx.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }
                var attributeTypeSymbol = attributeSymbol.ContainingType;
                if (attributeTypeSymbol.Equals(automaticProxyAttribute, SymbolEqualityComparer.Default))
                {
                    return syntax;
                }
            }

            return null;
        }

        private static void execute(
            Compilation compilation,
            ImmutableArray<InterfaceDeclarationSyntax> interfacesToGenerateFor,
            SourceProductionContext context)
        {
            try
            {
                foreach (var interfaceSymbol in interfacesToGenerateFor.Select(syntax => toSymbol(compilation, syntax)))
                {
                    var name = NameFactory.FromInterfaceName(interfaceSymbol.Name).ClassNameWithSuffix("Proxy");
                    var source = SourceText.From(ProxySourceGenerator.GenerateFor(name, interfaceSymbol),
                        Encoding.Default);
                    context.AddSource(name, source);
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(CreateDebugDiagnostic(e.ToString(), DiagnosticSeverity.Error));
            }
        }

        private static INamedTypeSymbol toSymbol(Compilation compilation, SyntaxNode typeSyntax)
        {
            var classModel = compilation.GetSemanticModel(typeSyntax.SyntaxTree);
            return classModel.GetDeclaredSymbol(typeSyntax) as INamedTypeSymbol
                ?? throw new InvalidCastException("Expected a named type");
        }
    }
}
