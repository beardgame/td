using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Rendering;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Controls;

sealed class Sprite : Control
{
    public required ModAwareSpriteId SpriteId { get; set; }

    public SpriteLayout Layout { get; set; } =
        new(frame: default, SpriteSize.ContainInFrame, scale: new Vector2(1, -1));

    public Color Color { get; set; } = Color.White;

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}
