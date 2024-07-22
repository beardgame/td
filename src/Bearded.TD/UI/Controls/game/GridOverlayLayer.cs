using System.Linq;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Tiles;

namespace Bearded.TD.UI.Controls;

sealed class GridOverlayLayer(GameState game) : IOverlayLayer
{
    public DrawOrder DrawOrder => DrawOrder.GridLines;

    public void Draw(IOverlayDrawer context)
    {
        var revealedTiles = Tilemap.EnumerateTilemapWith(game.Level.Radius)
            .Where(isTileRevealed);

        context.Draw(Area.From(revealedTiles), OverlayBrush.GridLines);
    }

    private bool isTileRevealed(Tile tile)
    {
        return game.VisibilityLayer[tile].IsRevealed();
    }
}
