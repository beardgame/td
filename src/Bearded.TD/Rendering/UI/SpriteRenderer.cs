using Bearded.TD.Content;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
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
        var center = frame.TopLeft + frame.Size * 0.5;

        drawable.DrawWithWidth(
            ((Vector2)center).WithZ(),
            (float)control.Size,
            control.Color);
    }
}
