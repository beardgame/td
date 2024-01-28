using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

sealed class Sprite : Control
{
    public required ModAwareSpriteId SpriteId { get; set; }
    public double Size { get; set; } = 24;
    public Color Color { get; set; } = Color.White;

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}
