﻿using System.Collections.Generic;
using Bearded.Graphics.RenderSettings;
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
            mesh => new OverlayMeshBuilder(mesh)
        );

        var drawer = new OverlayDrawer(shapeDrawer, components, gradients);

        renderers.CreateAndRegisterRendererFor(
            shapeShader.RendererShader,
            DrawOrderGroup.LevelProjectedFloatingOverlay, 0,
            settings => new OverlayRenderer(shapeDrawer, gradients, components, overlays, drawer, settings)
        );
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
            foreach (var (layer, mask) in overlays.LayersFor(drawOrder))
            {
                drawer.Mask = mask;
                layer.Draw(drawer);
            }
        }

        gradients.Flush();
        components.Flush();
        base.Render();
    }
}
