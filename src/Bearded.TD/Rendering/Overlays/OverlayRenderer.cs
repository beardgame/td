using System;
using System.Collections.Generic;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Shapes;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Rendering.Shapes;
using Bearded.Utilities;
using OpenTK.Mathematics;
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

    private sealed class MeshBuilder(IIndexedTrianglesMeshBuilder<ShapeVertex, ushort> mesh)
        : ShapeDrawer.IMeshBuilder
    {
        private readonly ShapeDrawer3<ShapeVertex,(ShapeData Data, ShapeVertex.ShapeComponents Components)> drawer =
            new(mesh, createVertex);

        private static ShapeVertex createVertex(
            Vector3 xyz, (ShapeData Data, ShapeVertex.ShapeComponents Components) p) => new(xyz, p.Data, p.Components);


        public void AddRectangle(
            float x0, float x1, float y0, float y1, float z,
            ShapeVertex.ShapeComponents components,
            ShapeData shapeData)
        {
            z -= 0.1f;
            drawer.DrawCuboid(x0, y0, z, x1 - x0, y1 - y0, 2 - z, (shapeData, components));
        }

        public void AddParallelogram(
            Vector2 xy, Vector2 side1, Vector2 side2, ShapeComponentsForDrawing components, ShapeData shape)
        {
            const float z0 = -0.1f;
            const float z1 = 2.1f;

            var xy1 = xy + side1;
            var xy2 = xy + side1 + side2;
            var xy3 = xy + side2;

            mesh.Add(8, 36, out var vertices, out var indices, out var indexOffset);

            vertices[0] = new ShapeVertex(xy.WithZ(z0), shape, components.Components);
            vertices[1] = new ShapeVertex(xy1.WithZ(z0), shape, components.Components);
            vertices[2] = new ShapeVertex(xy2.WithZ(z0), shape, components.Components);
            vertices[3] = new ShapeVertex(xy3.WithZ(z0), shape, components.Components);

            vertices[4] = new ShapeVertex(xy.WithZ(z1), shape, components.Components);
            vertices[5] = new ShapeVertex(xy1.WithZ(z1), shape, components.Components);
            vertices[6] = new ShapeVertex(xy2.WithZ(z1), shape, components.Components);
            vertices[7] = new ShapeVertex(xy3.WithZ(z1), shape, components.Components);

            Span<int> localIndices = stackalloc int[36]
            {
                0, 3, 2,
                0, 2, 1,
                0, 1, 5,
                0, 5, 4,
                0, 4, 7,
                0, 7, 3,
                6, 5, 1,
                6, 1, 2,
                6, 2, 3,
                6, 3, 7,
                6, 7, 4,
                6, 4, 5,
            };

            for (var i = 0; i < indices.Length; i++)
            {
                indices[i] = (ushort) (indexOffset + localIndices[i]);
            }
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
