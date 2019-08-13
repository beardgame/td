namespace Bearded.TD.Game.Buildings
{
    struct BuildingCreated : IEvent
    {
        public Building Building { get; }

        public BuildingCreated(Building building)
        {
            Building = building;
        }
    }
}
