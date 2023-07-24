using Bearded.Graphics;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Game.Simulation.World;

static class Biomes
{
    public static IBiome Default => new Biome(ModAwareId.Invalid, Color.HotPink);

    private sealed record Biome(ModAwareId Id, Color OverlayColor) : IBiome;
}
