namespace Bearded.TD.Game.Overlays;

interface IOverlayLayer
{
    DrawOrder DrawOrder { get; }
    void Render(IOverlayDrawer context);
}
