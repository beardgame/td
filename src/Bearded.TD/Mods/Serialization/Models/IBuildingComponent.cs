namespace Bearded.TD.Mods.Serialization.Models
{
    interface IBuildingComponent
    {
        string Id { get; set; }
        bool OnBuilding { get; set; }
        bool OnPlaceholder { get; set; }
        bool OnGhost { get; set; }
    }
}
