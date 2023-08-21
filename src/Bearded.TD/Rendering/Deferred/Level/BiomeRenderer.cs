using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using static Bearded.TD.Tiles.Direction;

namespace Bearded.TD.Rendering.Deferred.Level;

internal sealed class BiomeRenderer
{
    private readonly BiomeBuffer biomeBuffer;
    private readonly BiomeMaterials biomeMaterials;
    private readonly Tiles.Level level;
    private readonly BiomeLayer biomeLayer;

    private bool needsRedraw = true;

    public BiomeRenderer(GameInstance game, BiomeBuffer biomeBuffer, BiomeMaterials biomeMaterials, RenderContext context)
    {
        this.biomeBuffer = biomeBuffer;
        this.biomeMaterials = biomeMaterials;
        level = game.State.Level;
        biomeLayer = game.State.BiomeLayer;
    }

    public void Render()
    {
        if (!needsRedraw)
            return;

        render();

        needsRedraw = false;
    }

    private void render()
    {
        foreach (var tile in Tilemap.EnumerateTilemapWith(level.Radius))
        {
            var rightIsValid = level.IsValid(tile.Neighbor(Right));
            var downRightIsValid = level.IsValid(tile.Neighbor(DownRight));
            var downLeftIsValid = level.IsValid(tile.Neighbor(DownLeft));

            var tileId = id(tile);

            var biomeIds = new BiomeIds(
                tileId,
                rightIsValid ? id(tile.Neighbor(Right)) : tileId,
                downRightIsValid ? id(tile.Neighbor(DownRight)) : tileId,
                downLeftIsValid ? id(tile.Neighbor(DownLeft)) : tileId
            );

            biomeBuffer.SetTile(tile, biomeIds);
        }
    }

    private byte id(Tile tile)
    {
        var biome = biomeLayer[tile];
        return biomeMaterials.GetId(biome);
    }
}
