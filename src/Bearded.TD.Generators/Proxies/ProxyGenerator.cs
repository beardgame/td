using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Bearded.TD.Generators.DiagnosticFactory;

namespace Bearded.TD.Generators.Proxies;

[Generator]
[UsedImplicitly]
public sealed class ProxyGenerator : IIncrementalGenerator
{
    private const string fullAttributeName = "Bearded.TD.Shared.Proxies.AutomaticProxyAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
// #if DEBUG
//             if (!Debugger.IsAttached)
//             {
//                 Debugger.Launch();
//             }
// #endif

        var interfacesToGenerateFor =
            SyntaxProviders.InterfacesWithAttribute(context.SyntaxProvider, fullAttributeName);

        var interfacesWithCompilation = context.CompilationProvider.Combine(interfacesToGenerateFor.Collect());

        context.RegisterSourceOutput(
            interfacesWithCompilation, (spc, source) => execute(source.Left, source.Right, spc));
    }

    private static void execute(
        Compilation compilation,
        ImmutableArray<InterfaceDeclarationSyntax> interfacesToGenerateFor,
        SourceProductionContext context)
    {
        try
        {
            foreach (var interfaceSymbol in interfacesToGenerateFor.Select(compilation.ResolveNamedTypeSymbol))
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
}
