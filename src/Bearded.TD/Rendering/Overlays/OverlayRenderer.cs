using System.Collections.Generic;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Rendering.Overlays;

static class OverlayRenderers
{
    public static void Configure(ActiveOverlays overlays, RenderContext context, IDrawableRenderers renderers)
    {
        // TODO: move these straight into the renderer?
        var mesh = new ExpandingIndexedTrianglesMeshBuilder<ColorVertexData>();
        var drawer = new OverlayDrawer(mesh);

        // TODO: use same overload for fluid geometry?
        renderers.CreateAndRegisterRendererFor(
            context.Shaders.GetShaderProgram("geometry"),
            DrawOrderGroup.IgnoreDepth, -100,
            settings => new OverlayRenderer(mesh, overlays, drawer, settings)
        );
    }
}

sealed class OverlayRenderer(
    ExpandingIndexedTrianglesMeshBuilder<ColorVertexData> meshBuilder,
    ActiveOverlays overlays,
    OverlayDrawer drawer,
    IEnumerable<IRenderSetting> settings)
    : RendererDecorator(BatchedRenderer.From(meshBuilder.ToRenderable(), [..settings]))
{
    public override void Render()
    {
        meshBuilder.Clear();

        foreach (var drawOrder in ActiveOverlays.DrawOrdersInOrder)
        {
            foreach (var layer in overlays.LayersFor(drawOrder))
            {
                layer.Draw(drawer);
            }
        }

        base.Render();
    }

    public override void Dispose()
    {
        meshBuilder.Dispose();
        base.Dispose();
    }
}
