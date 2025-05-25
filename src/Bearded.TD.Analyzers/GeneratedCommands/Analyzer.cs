using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Shared.Commands;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Bearded.TD.Analyzers.GeneratedCommands;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
[UsedImplicitly]
sealed partial class Analyzer : DiagnosticAnalyzer
{
    private static readonly Type generateCommandAttributeType = typeof(GenerateCommandAttribute);
    private static readonly string commandAttributeName = generateCommandAttributeType.Name;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(
            TD102GeneratedCommandDoesNotHaveSingleExecuteMethod.Rule,
            TD101GeneratedCommandIsNotStaticPartialClass.Rule
        );

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(analyze, SymbolKind.NamedType);
    }

    private static void analyze(SymbolAnalysisContext context)
    {
        var commandType = (INamedTypeSymbol)context.Symbol;

        var attributes = commandType.GetAttributes();

        if (!attributes.Any(a => a.AttributeClass?.Name == commandAttributeName))
            return;

        TD101GeneratedCommandIsNotStaticPartialClass.Check(context, commandType);
        TD102GeneratedCommandDoesNotHaveSingleExecuteMethod.Check(context, commandType);
    }
}
