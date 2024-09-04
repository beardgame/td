using System;

namespace Bearded.TD.Game.Simulation.Resources;

interface IResourceType
{
    static abstract string Name { get; }
    static abstract ResourceType Type { get; }
}

enum ResourceType : byte
{
    Scrap,
    CoreEnergy,
}

abstract class Scrap : IResourceType
{
    private Scrap() { }
    public static string Name => "Scrap";
    public static ResourceType Type => ResourceType.Scrap;
}

abstract class CoreEnergy : IResourceType
{
    private CoreEnergy() { }
    public static string Name => "Core Energy";
    public static ResourceType Type => ResourceType.CoreEnergy;
}

static class ResourceTypeExtensions
{
    public static Resource<Scrap> Scrap(this int value) => new(value);
    public static Resource<Scrap> Scrap(this double value) => new(value);

    public static Resource<CoreEnergy> CoreEnergy(this int value) => new(value);
    public static Resource<CoreEnergy> CoreEnergy(this double value) => new(value);

    public static void Switch(this ResourceType type, double amount, Action<Resource<Scrap>> scrap, Action<Resource<CoreEnergy>> coreEnergy)
    {
        switch (type)
        {
            case ResourceType.Scrap:
                scrap(amount.Scrap());
                break;
            case ResourceType.CoreEnergy:
                coreEnergy(amount.CoreEnergy());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static T Switch<T>(this ResourceType type, double amount, Func<Resource<Scrap>, T> scrap, Func<Resource<CoreEnergy>, T> coreEnergy)
    {
        return type switch
        {
            ResourceType.Scrap => scrap(amount.Scrap()),
            ResourceType.CoreEnergy => coreEnergy(amount.CoreEnergy()),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

