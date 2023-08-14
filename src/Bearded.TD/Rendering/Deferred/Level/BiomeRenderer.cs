using System;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Tiles;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Rendering.Deferred.Level;

using static Pipeline<Void>;

internal sealed class BiomeRenderer : IDisposable
{
    private readonly BiomeMaterials biomeMaterials;
    private readonly Tiles.Level level;
    private readonly BiomeLayer biomeLayer;

    private readonly IPipeline<Void> renderBiomes;

    private readonly IndexedTrianglesMeshBuilder<BiomeMapVertex> baseMeshBuilder;
    private readonly Renderer baseRenderer;
    private readonly ShapeDrawer2<BiomeMapVertex, byte> baseDrawer;

    private bool needsRedraw = true;

    public BiomeRenderer(GameInstance game, BiomeMap biomeMap, BiomeMaterials biomeMaterials, RenderContext context)
    {
        this.biomeMaterials = biomeMaterials;
        level = game.State.Level;
        biomeLayer = game.State.BiomeLayer;

        biomeMap.ResolutionChanged += () => needsRedraw = true;

        renderBiomes = biomeMap.DrawBiomeIndex(Do(render));

        (baseMeshBuilder, baseRenderer, baseDrawer) = initializeBaseDrawing(context, biomeMap);
    }

    private static (IndexedTrianglesMeshBuilder<BiomeMapVertex>, Renderer, ShapeDrawer2<BiomeMapVertex, byte>)
        initializeBaseDrawing(RenderContext context, BiomeMap biomeMap)
    {
        var meshBuilder = new IndexedTrianglesMeshBuilder<BiomeMapVertex>();
        var renderer = Renderer.From(meshBuilder.ToRenderable(), biomeMap.RadiusUniform, biomeMap.PixelSizeUVUniform);
        var drawer =
            new ShapeDrawer2<BiomeMapVertex, byte>(meshBuilder, (p, id) => new BiomeMapVertex(p.Xy, id));

        context.Shaders.GetShaderProgram("biome-base").UseOnRenderer(renderer);

        return (meshBuilder, renderer, drawer);
    }

    public void Render()
    {
        if (!needsRedraw)
            return;

        renderBiomes.Execute();

        needsRedraw = false;
    }

    private void render()
    {
        var count = 0;
        foreach (var tile in Tilemap.EnumerateTilemapWith(level.Radius))
        {
            var p = Tiles.Level.GetPosition(tile).NumericValue;
            var biome = biomeLayer[tile];
            byte biomeId = biomeMaterials.GetId(biome);

            baseDrawer.FillCircle(p, Constants.Game.World.HexagonSide, biomeId, 6);

            count++;

            if (count > 10000)
            {
                baseRenderer.Render();
                baseMeshBuilder.Clear();
                count = 0;
            }
        }

        baseRenderer.Render();
        baseMeshBuilder.Clear();
    }

    public void Dispose()
    {
        baseMeshBuilder.Dispose();
        baseRenderer.Dispose();
    }
}
