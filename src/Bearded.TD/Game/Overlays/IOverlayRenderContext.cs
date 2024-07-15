using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Overlays;

interface IOverlayDrawer
{
    void Draw(Tile tile, OverlayBrush brush);
    void Draw(IArea area, OverlayBrush brush);
}
