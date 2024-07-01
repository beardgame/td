using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Input;

sealed class BuildableAreaOverlay : IOverlayLayer
{
    private readonly GameState game;

    public DrawOrder DrawOrder => DrawOrder.BuildableArea;

    public BuildableAreaOverlay(GameState game)
    {
        this.game = game;
    }

    public void Draw(IOverlayDrawer context)
    {
        var revealedBlockedTiles = Tilemap.EnumerateTilemapWith(game.Level.Radius)
            .Where(t => isTileRevealed(t) && !BuildableTileChecker.TileIsBuildable(game, t));
        foreach (var tile in revealedBlockedTiles)
        {
            context.Tile(Color.Red, tile);
        }
    }

    private bool isTileRevealed(Tile tile)
    {
        return game.VisibilityLayer[tile].IsRevealed();
    }
}
