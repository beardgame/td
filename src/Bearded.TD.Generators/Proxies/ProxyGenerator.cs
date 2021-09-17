using System;
using System.Collections.Generic;
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
    public sealed class ProxyGenerator : ISourceGenerator
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
                if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
                {
                    return;
                }

                var automaticProxyAttribute = context.Compilation
                        .GetTypeByMetadataName("Bearded.TD.Shared.Proxies.AutomaticProxyAttribute") ??
                    throw new InvalidOperationException("Could not find automatic proxy attribute.");

                var interfacesToGeneratorFor =
                    SymbolFilters.FindSymbolsWithAttribute(
                        context.Compilation, receiver.Interfaces, automaticProxyAttribute);

                foreach (var interfaceSymbol in interfacesToGeneratorFor)
                {
                    var name = NameFactory.FromInterfaceName(interfaceSymbol.Name).ClassNameWithSuffix("Proxy");
                    var source = SourceText.From(ProxySourceGenerator.GenerateFor(name, interfaceSymbol), Encoding.Default);
                    context.AddSource(name, source);
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(CreateDebugDiagnostic(e.ToString(), DiagnosticSeverity.Error));
            }
        }

        private sealed class SyntaxReceiver : ISyntaxReceiver
        {
            public List<InterfaceDeclarationSyntax> Interfaces { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                switch (syntaxNode)
                {
                    case InterfaceDeclarationSyntax interfaceSyntax:
                        Interfaces.Add(interfaceSyntax);
                        break;
                }
            }
        }
    }
}
