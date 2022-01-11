using System;
using System.Linq;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Pipelines.Context;
using Bearded.Graphics.Rendering;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Rendering.Deferred.Level;

using static Pipeline<Void>;

sealed class VisibilityRenderer : IDisposable
{
    private readonly Tiles.Level level;
    private readonly VisibilityLayer visibilityLayer;

    private readonly IPipeline<Void> renderToHeightmap;

    private readonly DrawableSpriteSet<HeightmapSplatVertex, (float MinH, float MaxH)> splats;
    private readonly IRenderer splatRenderer;

    private bool needsFullRedraw = true;

    public VisibilityRenderer(GameInstance game, Heightmap heightmap)
    {
        level = game.State.Level;
        visibilityLayer = game.State.VisibilityLayer;

        heightmap.ResolutionChanged += () => needsFullRedraw = true;

        renderToHeightmap = heightmap.DrawVisibility(
            WithContext(
                c => c.SetBlendMode(BlendMode.Max),
                InOrder(
                    ClearColor(0, 0, 0, 0),
                    Do(renderVisibility)
                )));

        (splats, splatRenderer) = findHeightmapSplats(game, heightmap);
    }

    private static (DrawableSpriteSet<HeightmapSplatVertex, (float MinH, float MaxH)>, IRenderer)
        findHeightmapSplats(GameInstance game, Heightmap heightmap)
    {
        return game.Blueprints.Sprites[ModAwareId.ForDefaultMod("terrain-splats")]
            .MakeCustomRendererWith<HeightmapSplatVertex, (float MinH, float MaxH)>(
                game.Meta.SpriteRenderers,
                HeightmapSplatVertex.Create,
                game.Meta.Blueprints.Shaders[ModAwareId.ForDefaultMod("heightmap-splatter")],
                heightmap.RadiusUniform
            );
    }

    public void Dispose()
    {
        splats.Dispose();
        splatRenderer.Dispose();
    }

    public void ZoneChanged(Zone zone)
    {
        // TODO: only redraw what's needed please
        needsFullRedraw = true;
    }

    public void Render()
    {
        if (!needsFullRedraw)
            return;

        renderToHeightmap.Execute();

        needsFullRedraw = false;
    }

    private void renderVisibility()
    {
        var visibleTiles = Tilemap
            .EnumerateTilemapWith(level.Radius)
            .Where(t => visibilityLayer[t].IsVisible());

        var splat = splats.GetSprite("visibility-hex");

        foreach (var tile in visibleTiles)
        {
            var p = Tiles.Level.GetPosition(tile).NumericValue.WithZ(0);

            var angle = tile.X * 1.3f + tile.Y * 2.9f;

            splat.Draw(p, Constants.Game.World.HexagonSide * 0.75f, angle, (0, 1));
        }

        splatRenderer.Render();
        splats.Clear();
    }
}
