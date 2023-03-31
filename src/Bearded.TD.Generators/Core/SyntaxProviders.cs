using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bearded.TD.Generators;

static class SyntaxProviders
{
    public static IncrementalValuesProvider<InterfaceDeclarationSyntax> Interfaces(SyntaxValueProvider valueProvider)
    {
        return valueProvider.CreateSyntaxProvider(
            predicate: (node, _) => node is InterfaceDeclarationSyntax,
            transform: (ctx, _) => (InterfaceDeclarationSyntax) ctx.Node);
    }

    public static IncrementalValuesProvider<InterfaceDeclarationSyntax> InterfacesWithAttribute(
        SyntaxValueProvider valueProvider, string fullAttributeName)
    {
        return valueProvider.CreateSyntaxProvider(
                predicate: (node, _) => node is InterfaceDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: (ctx, _) => SyntaxTransforms.AsInterfaceWithAttribute(ctx, fullAttributeName))
            .Where(i => i is not null)
            .Select((i, _) => i!);
    }

    public static IncrementalValuesProvider<ClassDeclarationSyntax> ClassesWithAttribute(
        SyntaxValueProvider valueProvider, string fullAttributeName)
    {
        return valueProvider.CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: (ctx, _) => SyntaxTransforms.AsClassWithAttribute(ctx, fullAttributeName))
            .Where(i => i is not null)
            .Select((i, _) => i!);
    }

    public static IncrementalValuesProvider<FieldDeclarationSyntax> FieldsWithAttribute(
        SyntaxValueProvider valueProvider, string fullAttributeName)
    {
        return valueProvider.CreateSyntaxProvider(
                predicate: (node, _) => node is FieldDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: (ctx, _) => SyntaxTransforms.AsFieldWithAttribute(ctx, fullAttributeName))
            .Where(i => i is not null)
            .Select((i, _) => i!);
    }
}
