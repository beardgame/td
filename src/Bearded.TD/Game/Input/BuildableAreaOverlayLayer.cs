using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Input;

sealed class BuildableAreaOverlayLayer(GameState game) : IOverlayLayer
{
    public DrawOrder DrawOrder => DrawOrder.BuildableArea;

    public void Draw(IOverlayDrawer context)
    {
        var revealedBlockedTiles = Tilemap.EnumerateTilemapWith(game.Level.Radius)
            .Where(t => isTileRevealed(t) && !BuildableTileChecker.TileIsBuildable(game, t));
        foreach (var tile in revealedBlockedTiles)
        {
            context.Draw(tile, OverlayBrush.BlockedTile);
        }
    }

    private bool isTileRevealed(Tile tile)
    {
        return game.VisibilityLayer[tile].IsRevealed();
    }
}
