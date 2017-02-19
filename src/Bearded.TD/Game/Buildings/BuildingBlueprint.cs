using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.Buildings
{
    struct BuildingBlueprint
    {
        public Footprint Footprint { get; }
        public int MaxHealth { get; }

        public BuildingBlueprint(Footprint footprint, int maxHealth)
        {
            Footprint = footprint;
            MaxHealth = maxHealth;
        }
    }
}
