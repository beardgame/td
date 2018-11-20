namespace Bearded.TD.Game.Buildings
{
    struct BuildingCreated : IEvent
    {
        public Building Builder { get; }

        public BuildingCreated(Building building)
        {
            Builder = building;
        }
    }
}
