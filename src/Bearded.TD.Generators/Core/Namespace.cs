using Microsoft.CodeAnalysis;

namespace Bearded.TD.Generators;

record struct Namespace(bool Global, string Name)
{
    public static Namespace From(INamespaceSymbol namespaceSymbol)
    {
        return new Namespace(
            namespaceSymbol.IsGlobalNamespace,
            $"{namespaceSymbol}");
    }

    public string AsFileScopedDeclaration() => Global ? "" : $"namespace {Name};";
}
