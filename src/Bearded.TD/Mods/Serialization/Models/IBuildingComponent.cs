namespace Bearded.TD.Mods.Serialization.Models
{
    interface IBuildingComponent : IComponent
    {
        bool OnBuilding { get; }
        bool OnPlaceholder { get; }
        bool OnGhost { get; }
    }
}
