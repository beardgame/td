namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class BuildingComponent<TParameters> : IBuildingComponent
    {
        public string Id { get; set; }
        public bool OnBuilding { get; set; } = true;
        public bool OnPlaceholder { get; set; }
        public bool OnGhost { get; set; }
        public TParameters Parameters { get; set; }

        object IComponent.Parameters => Parameters;
    }
}
