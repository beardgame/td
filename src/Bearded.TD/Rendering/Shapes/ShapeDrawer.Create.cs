using Bearded.TD.Content.Models;

namespace Bearded.TD.Rendering.Shapes;

sealed partial class ShapeDrawer
{
    private sealed class DrawableTemplate : IDrawableTemplate
    {
        public static DrawableTemplate Instance { get; } = new();
    }

    public static ShapeDrawer GetOrCreate(
        IDrawableRenderers renderers,
        Shader shader,
        DrawOrderGroup drawGroup,
        int drawGroupOrderKey)
    {
        return renderers.GetOrCreateDrawableFor(
            DrawableTemplate.Instance, shader, drawGroup, drawGroupOrderKey,
            () => new ShapeDrawer(shader)
        );
    }
}
