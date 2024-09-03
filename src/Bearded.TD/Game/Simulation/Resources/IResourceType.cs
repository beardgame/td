namespace Bearded.TD.Game.Simulation.Resources;

interface IResourceType
{
    static abstract string Name { get; }
}

abstract class Scrap : IResourceType
{
    private Scrap() { }
    public static string Name => "Scrap";
}

abstract class CoreEnergy : IResourceType
{
    private CoreEnergy() { }
    public static string Name => "Core Energy";
}

static class ResourceTypeExtensions
{
    public static Resource<Scrap> Scrap(this int value) => new(value);
    public static Resource<Scrap> Scrap(this double value) => new(value);

    public static Resource<CoreEnergy> CoreEnergy(this int value) => new(value);
    public static Resource<CoreEnergy> CoreEnergy(this double value) => new(value);
}
