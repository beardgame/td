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

        var tl = (Vector2)frame.TopLeft;
        var size = (Vector2) frame.Size;

        if (size.X < 0)
        {
            tl.X += size.X;
            size.X = -size.X;
        }
        if (size.Y < 0)
        {
            tl.Y += size.Y;
            size.Y = -size.Y;
        }

        drawable.Draw(control.Layout with { Frame = new Rectangle(tl, size.X, size.Y) }, control.Color);
    }
}
