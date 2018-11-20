namespace Bearded.TD.Game.Buildings
{
    struct BuildingDestroyed : IEvent
    {
        public Building Builder { get; }

        public BuildingDestroyed(Building building)
        {
            Builder = building;
        }
    }
}
