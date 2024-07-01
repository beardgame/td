using Bearded.Graphics;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Overlays;

interface IOverlayDrawer
{
    void Tile(Color color, Tile tile, Unit height = default);
}
