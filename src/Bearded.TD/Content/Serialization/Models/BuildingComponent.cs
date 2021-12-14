namespace Bearded.TD.Content.Serialization.Models;

sealed class BuildingComponent<TParameters> : Component<TParameters>, IBuildingComponent
{
    public bool OnBuilding { get; set; } = true;
    public bool OnGhost { get; set; }
}