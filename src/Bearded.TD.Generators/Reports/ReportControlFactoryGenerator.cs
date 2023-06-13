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

namespace Bearded.TD.Generators.Reports;

[Generator]
[UsedImplicitly]
sealed class ReportControlFactoryGenerator : IIncrementalGenerator
{
    private const string fullAttributeName = "Bearded.TD.UI.Reports.ReportsOnAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
// #if DEBUG
//             if (!Debugger.IsAttached)
//             {
//                 Debugger.Launch();
//             }
// #endif

        var controlsToGenerateFor =
            SyntaxProviders.ClassesWithAttribute(context.SyntaxProvider, fullAttributeName);

        var controlsWithCompilation = context.CompilationProvider.Combine(controlsToGenerateFor.Collect());

        context.RegisterSourceOutput(
            controlsWithCompilation, (spc, source) => execute(source.Left, source.Right, spc));
    }

    private static void execute(
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classesToGenerateFor,
        SourceProductionContext spc)
    {
        try
        {
            var reportsOnAttributeSymbol = compilation.GetTypeByMetadataName(fullAttributeName) ??
                throw new InvalidOperationException("Could not find reports on attribute class.");
            var executeContext = new ExecuteContext(
                compilation.GetTypeByMetadataName("Bearded.TD.Game.GameInstance") ??
                throw new InvalidOperationException("Could not find game instance class."),
                compilation.GetTypeByMetadataName("Bearded.TD.UI.Controls.ReportControl") ??
                throw new InvalidOperationException("Could not find report control class."),
                spc
            );

            var classBuilder = ImmutableArray.CreateBuilder<ControlToGenerateFor>();

            foreach (var classSymbol in classesToGenerateFor.Select(compilation.ResolveNamedTypeSymbol))
            {
                var attribute = classSymbol.GetAttributes().First(a =>
                    reportsOnAttributeSymbol.Equals(a.AttributeClass, SymbolEqualityComparer.Default));
                if (attribute.ConstructorArguments.Length != 2)
                {
                    throw new InvalidOperationException("Invalid number of constructor arguments for attribute.");
                }
                var typeToReportOn = (attribute.ConstructorArguments[0].Value as INamedTypeSymbol)!;

                if (!validateClass(classSymbol, typeToReportOn, executeContext, out var requiresGame))
                {
                    continue;
                }

                classBuilder.Add(new ControlToGenerateFor(classSymbol, typeToReportOn, requiresGame));
            }

            spc.AddSource(
                "ReportControlFactory.g.cs",
                SourceText.From(
                    ReportControlFactorySourceGenerator.GenerateFor(classBuilder.ToImmutable()),
                    Encoding.Default));
        }
        catch (Exception e)
        {
            spc.ReportDiagnostic(CreateDebugDiagnostic(e.ToString(), DiagnosticSeverity.Error));
        }
    }

    private sealed record ExecuteContext(
        ISymbol GameInstanceSymbol, ISymbol ReportControlSymbol, SourceProductionContext Spc);

    private static bool validateClass(
        INamedTypeSymbol symbol,
        ISymbol toReportOnSymbol,
        ExecuteContext context,
        out bool requiresGame)
    {
        if (!enumerateBaseTypes(symbol).Any(s => s.Equals(context.ReportControlSymbol, SymbolEqualityComparer.Default)))
        {
            context.Spc.ReportDiagnostic(Diagnostic.Create(
                missingInheritanceRule,
                symbol.DeclaringSyntaxReferences[0].GetSyntax().GetLocation()));
            requiresGame = default;
            return false;
        }

        var ctor = symbol.Constructors.FirstOrDefault(method =>
        {
            if (method.DeclaredAccessibility != Accessibility.Public || method.Parameters.Length is < 1 or > 2)
            {
                return false;
            }

            var parameterTypes = method.Parameters.Select(p => p.Type).ToImmutableArray();


            if (!parameterTypes[^1].Equals(toReportOnSymbol, SymbolEqualityComparer.Default))
            {
                return false;
            }

            if (parameterTypes.Length == 2 &&
                !parameterTypes[0].Equals(context.GameInstanceSymbol, SymbolEqualityComparer.Default))
            {
                return false;
            }

            return true;
        });

        if (ctor is null)
        {
            context.Spc.ReportDiagnostic(Diagnostic.Create(
                missingConstructorInControlRule,
                symbol.DeclaringSyntaxReferences[0].GetSyntax().GetLocation()));
            requiresGame = default;
            return false;
        }

        requiresGame = ctor.Parameters.Length == 2;
        return true;
    }

    private static IEnumerable<INamedTypeSymbol> enumerateBaseTypes(INamedTypeSymbol symbol)
    {
        while (symbol.BaseType is not null)
        {
            yield return symbol.BaseType;
            symbol = symbol.BaseType;
        }
    }

    private static DiagnosticDescriptor missingConstructorInControlRule { get; } = new(
        id: "TDG001",
        title: "No valid constructor for report control found",
        messageFormat:
        "Controls with the ReportsOn attribute must have a public constructor with (optionally) game instance and the report as parameter.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Controls with the ReportsOn attribute must have a public constructor with (optionally) game instance and the report as parameter."
    );

    private static DiagnosticDescriptor missingInheritanceRule { get; } = new(
        id: "TDG002",
        title: "Missing base class for report control",
        messageFormat:
        "Controls with the ReportsOn attribute must inherit from ReportControl.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Controls with the ReportsOn attribute must inherit from ReportControl."
    );
}
