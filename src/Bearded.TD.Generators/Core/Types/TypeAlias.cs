namespace Bearded.TD.Generators.Types;

record struct TypeAlias(FullTypeName FullName, ShortTypeName Alias)
{
    public override string ToString() => $"using {Alias} = {FullName};";
}
