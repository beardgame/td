namespace Bearded.TD.Mods.Serialization.Models
{
    interface IComponent
    {
        string Id { get; }
        object Parameters { get; }
    }

    interface IBuildingComponent : IComponent
    {
        bool OnBuilding { get; }
        bool OnPlaceholder { get; }
        bool OnGhost { get; }
    }
}
