using System.Collections.Generic;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Shapes;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Rendering.Shapes;
using static Bearded.TD.Constants.Content.CoreUI;

namespace Bearded.TD.Rendering.Overlays;

static class OverlayRenderers
{
    public static void Configure(
        ActiveOverlays overlays,
        RenderContext context,
        IDrawableRenderers renderers,
        Blueprints blueprints)
    {
        var shapeShader = blueprints.Shaders[DefaultShaders.Shapes];

        var gradients = new GradientBuffer();
        var components = new ComponentBuffer();

        var shapeDrawer = ShapeDrawer.CreateUnregistered(
            [
                (gradients.TextureUniform, "gradientBuffer"),
                (components.TextureUniform, "componentBuffer"),
                (context.DeferredRenderer.GetDepthBufferUniform, "depthBuffer"),
            ],
            mesh => new MeshBuilder(mesh)
        );

        var drawer = new OverlayDrawer(shapeDrawer, components, gradients);

        renderers.CreateAndRegisterRendererFor(
            shapeShader.RendererShader,
            DrawOrderGroup.LevelProjected, 0,
            settings => new OverlayRenderer(shapeDrawer, gradients, components, overlays, drawer, settings)
        );
    }

    private sealed class MeshBuilder : ShapeDrawer.IMeshBuilder
    {
        private readonly ShapeDrawer3<ShapeVertex,(ShapeData Data, ShapeVertex.ShapeComponents Components)> drawer;

        public MeshBuilder(IIndexedTrianglesMeshBuilder<ShapeVertex, ushort> mesh)
        {
            drawer = new ShapeDrawer3<ShapeVertex, (ShapeData Data, ShapeVertex.ShapeComponents Components)>(mesh,
                (xyz, p) => new ShapeVertex(xyz, p.Data, p.Components)
            );
        }

        public void AddQuad(
            float x0, float x1, float y0, float y1, float z,
            ShapeVertex.ShapeComponents components,
            ShapeData shapeData)
        {
            z -= 0.1f;
            drawer.DrawCuboid(x0, y0, z, x1 - x0, y1 - y0, 2 - z, (shapeData, components));
        }
    }
}

sealed class OverlayRenderer(
    ShapeDrawer shapes,
    GradientBuffer gradients,
    ComponentBuffer components,
    ActiveOverlays overlays,
    OverlayDrawer drawer,
    IEnumerable<IRenderSetting> settings)
    : RendererDecorator(shapes, settings, [gradients, components])
{

    public override void Render()
    {
        gradients.Clear();
        components.Clear();
        shapes.Clear();

        foreach (var drawOrder in ActiveOverlays.DrawOrdersInOrder)
        {
            foreach (var layer in overlays.LayersFor(drawOrder))
            {
                layer.Draw(drawer);
            }
        }

        gradients.Flush();
        components.Flush();
        base.Render();
    }
}
