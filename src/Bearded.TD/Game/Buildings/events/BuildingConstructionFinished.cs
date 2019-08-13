namespace Bearded.TD.Game.Buildings
{
    struct BuildingConstructionFinished : IEvent
    {
        public Building Building { get; }

        public BuildingConstructionFinished(Building building)
        {
            Building = building;
        }
    }
}
