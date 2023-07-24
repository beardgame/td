using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.World;

sealed class BiomeLayer
{
    private readonly Tilemap<IBiome> tilemap;

    public BiomeLayer(int radius)
    {
        tilemap = new Tilemap<IBiome>(radius);
    }

    public void SetBiome(Tile tile, IBiome biome)
    {
        tilemap[tile] = biome;
    }

    public IBiome this[Tile tile] => tilemap[tile];
}
