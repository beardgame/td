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

namespace Bearded.TD.Generators.Listeners
{
    [Generator]
    [UsedImplicitly]
    public sealed class ListenerGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
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

                var eventListenerAttribute = context.Compilation
                        .GetTypeByMetadataName("Bearded.TD.Shared.Events.EventListenerAttribute") ??
                    throw new InvalidOperationException("Could not find event listener attribute.");
                var listenerInterface = context.Compilation
                        .GetTypeByMetadataName("Bearded.TD.Shared.Events.IListener`1")?
                        .ConstructUnboundGenericType() ??
                    throw new InvalidOperationException("Could not find listener interface.");
                var previewListenerInterface = context.Compilation
                        .GetTypeByMetadataName("Bearded.TD.Shared.Events.IPreviewListener`1")?
                        .ConstructUnboundGenericType() ??
                    throw new InvalidOperationException("Could not find preview listener interface.");

                var classesToGenerateFor =
                    SymbolFilters.FindSymbolsWithAttribute(
                        context.Compilation, receiver.Classes, eventListenerAttribute);

                foreach (var classSymbol in classesToGenerateFor)
                {
                    var name = classSymbol.Name;
                    var modifiers = receiver.ClassModifiers[name];
                    var attribute = classSymbol
                        .GetAttributes()
                        .First(a =>
                            a.AttributeClass?.Equals(eventListenerAttribute, SymbolEqualityComparer.Default) ?? false);
                    var eventsType = attribute.ConstructorArguments.First().Value as ITypeSymbol
                        ?? throw new InvalidOperationException();
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

        private sealed class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> Classes { get; } = new();
            public Dictionary<string, ImmutableArray<string>> ClassModifiers { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                switch (syntaxNode)
                {
                    case ClassDeclarationSyntax classSyntax:
                        Classes.Add(classSyntax);
                        ClassModifiers.Add(
                            classSyntax.Identifier.Text, classSyntax.Modifiers.Select(m => m.Text).ToImmutableArray());
                        break;
                }
            }
        }
    }
}
