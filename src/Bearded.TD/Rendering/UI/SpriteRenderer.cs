using Bearded.TD.Content;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities.Geometry;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.UI;

sealed class SpriteRenderer(
    ContentManager content,
    IDrawableRenderers renderers,
    Shader shader)
    : IRenderer<Sprite>
{
    public void Render(Sprite control)
    {
        var id = control.SpriteId;
        var sprite = content.ResolveSpriteSet(id.SpriteSet).GetSprite(id.Id);
        var drawable = sprite.MakeConcreteWith(
            renderers, DrawOrderGroup.UISpritesTop, 0, UVColorVertex.Create, shader);

        var frame = control.Frame;

        var rect = new Rectangle((Vector2)frame.TopLeft, (float)frame.Size.X, (float)frame.Size.Y);

        drawable.Draw(
            new SpriteLayout(rect, SpriteSize.ContainInFrame,
                scale: new Vector2(1, -1) * 0.75f
            ),
            control.Color
        );
    }
}
