using Microsoft.CodeAnalysis;

namespace Bearded.TD.Generators.Types;

record struct TypeName(ShortTypeName ShortName, FullTypeName FullName)
{
    public static TypeName From(ITypeSymbol typeSymbol)
    {
        return new TypeName(ShortTypeName.From(typeSymbol), FullTypeName.From(typeSymbol));
    }
}

record struct ShortTypeName(string Name)
{
    public static ShortTypeName From(ITypeSymbol typeSymbol)
    {
        return new ShortTypeName(
            Name: typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
        );
    }

    public override string ToString() => Name;
}

record struct FullTypeName(string Name)
{
    public static FullTypeName From(ITypeSymbol typeSymbol)
    {
        return new FullTypeName(
            Name: typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
        );
    }

    public override string ToString() => Name;
}
