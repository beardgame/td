namespace Bearded.TD.Game.Overlays;

interface IOverlayLayer
{
    DrawOrder DrawOrder { get; }
    void Draw(IOverlayDrawer context);
}
