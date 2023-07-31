using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Content.Models;

sealed class Biome : IBiome
{
    public ModAwareId Id { get; }
    public Color OverlayColor { get; }

    public Biome(ModAwareId id, Color overlayColor)
    {
        Id = id;
        OverlayColor = overlayColor;
    }
}
