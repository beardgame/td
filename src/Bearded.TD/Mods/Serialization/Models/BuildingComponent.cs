namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class BuildingComponent<TParameters> : Component<TParameters>, IBuildingComponent
    {
        public bool OnBuilding { get; set; } = true;
        public bool OnPlaceholder { get; set; }
        public bool OnGhost { get; set; }
    }
}
