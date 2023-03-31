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

namespace Bearded.TD.Generators.Listeners;

[Generator]
[UsedImplicitly]
public sealed class ListenerGenerator : IIncrementalGenerator
{
    private const string fullAttributeName = "Bearded.TD.Shared.Events.EventListenerAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
// #if DEBUG
//             if (!Debugger.IsAttached)
//             {
//                 Debugger.Launch();
//             }
// #endif

        var classesToGenerateFor = SyntaxProviders.ClassesWithAttribute(context.SyntaxProvider, fullAttributeName);

        var classesWithCompilation = context.CompilationProvider.Combine(classesToGenerateFor.Collect());

        context.RegisterSourceOutput(
            classesWithCompilation, (spc, source) => execute(source.Left, source.Right, spc));
    }

    private static void execute(
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classesToGenerateFor,
        SourceProductionContext context)
    {
        try
        {
            var eventListenerAttribute = compilation.GetTypeByMetadataName(fullAttributeName) ??
                throw new InvalidOperationException("Could not find event listener attribute");
            var listenerInterface = compilation
                    .GetTypeByMetadataName("Bearded.TD.Shared.Events.IListener`1")?
                    .ConstructUnboundGenericType() ??
                throw new InvalidOperationException("Could not find listener interface.");
            var previewListenerInterface = compilation
                    .GetTypeByMetadataName("Bearded.TD.Shared.Events.IPreviewListener`1")?
                    .ConstructUnboundGenericType() ??
                throw new InvalidOperationException("Could not find preview listener interface.");

            foreach (var classSyntax in classesToGenerateFor)
            {
                var classSymbol = compilation.ResolveSymbol(classSyntax);

                var name = classSymbol.Name;
                var modifiers = classSyntax.Modifiers.Select(m => m.Text).ToImmutableArray();
                var attribute = classSymbol
                    .GetAttributes()
                    .First(a =>
                        a.AttributeClass?.Equals(eventListenerAttribute, SymbolEqualityComparer.Default) ?? false);
                var eventsType = attribute.ConstructorArguments.First().Value as ITypeSymbol
                    ?? throw new InvalidOperationException("Could not find type symbol.");
                var listenerInterfaces = classSymbol.Interfaces.Where(i =>
                {
                    var unboundedType = i.ConstructUnboundGenericType();
                    return unboundedType.Equals(listenerInterface, SymbolEqualityComparer.Default) ||
                        unboundedType.Equals(previewListenerInterface, SymbolEqualityComparer.Default);
                });
                var events = listenerInterfaces.Select(i => i.TypeArguments.Single()).ToImmutableArray();
                var source = SourceText.From(
                    ListenerSourceGenerator.GenerateFor(classSymbol, modifiers, eventsType, events),
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
