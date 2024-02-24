using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Models.Fonts;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Rendering.Text;

static class TextDrawerExtensions
{
    public static TextDrawer<TVertex, TVertexParameters> MakeConcreteWith<TVertex, TVertexParameters>(
        this Font font, TextDrawerConfiguration config,
        IDrawableRenderers drawableRenderers,
        DrawOrderGroup drawGroup,
        int drawGroupOrderKey,
        CreateVertex<TVertex, TVertexParameters> createVertex,
        Shader? shader = null)
        where TVertex : struct, IVertexData
    {
        shader ??= font.Material.Shader;
        return drawableRenderers.GetOrCreateDrawableFor(
            font, shader, drawGroup, drawGroupOrderKey,
            () => TextDrawer.Create(font, config, createVertex, shader)
        );
    }
}
