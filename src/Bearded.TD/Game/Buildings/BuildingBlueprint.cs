using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.Buildings
{
    struct BuildingBlueprint
    {
        public TileSelection FootprintSelector { get; }
        public int MaxHealth { get; }

        public BuildingBlueprint(TileSelection footprint, int maxHealth)
        {
            FootprintSelector = footprint;
            MaxHealth = maxHealth;
        }
    }
}
