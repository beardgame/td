using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Models.Fonts;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Rendering.Text;

static class TextDrawerExtensions
{
    private sealed record FontTemplate(Font Font, TextDrawerConfiguration Config) : IDrawableTemplate;

    public static TextDrawer<TVertex, TVertexParameters> MakeConcreteWith<TVertex, TVertexParameters>(
        this Font font, TextDrawerConfiguration config,
        IDrawableRenderers drawableRenderers,
        DrawOrderGroup drawGroup,
        int drawGroupOrderKey,
        CreateVertex<TVertex, TVertexParameters> createVertex,
        Shader? shader = null)
        where TVertex : struct, IVertexData
    {
        var template = new FontTemplate(font, config);
        shader ??= font.Material.Shader;
        return drawableRenderers.GetOrCreateDrawableFor(
            template, shader, drawGroup, drawGroupOrderKey,
            () => TextDrawer.Create(font, config, createVertex)
        );
    }
}
