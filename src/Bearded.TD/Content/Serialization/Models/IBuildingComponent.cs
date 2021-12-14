namespace Bearded.TD.Content.Serialization.Models;

interface IBuildingComponent : IComponent
{
    bool OnBuilding { get; }
    bool OnGhost { get; }
}