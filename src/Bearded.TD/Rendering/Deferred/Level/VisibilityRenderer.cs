using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Pipelines.Context;
using Bearded.Graphics.Rendering;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Rendering.Deferred.Level;

using static Pipeline<Void>;

sealed class VisibilityRenderer : IDisposable
{
    private record struct FadingTile(Tile Tile, Instant Start, Instant Stop);

    private readonly ITimeSource time;
    private readonly Tiles.Level level;
    private readonly VisibilityLayer visibilityLayer;
    private readonly GeometryLayer geometryLayer;

    private readonly IPipeline<Void> renderToHeightmap;

    private readonly DrawableSpriteSet<HeightmapSplatVertex, (float MinH, float MaxH)> splats;
    private readonly IRenderer splatRenderer;

    private bool needsFullRedraw = true;

    private readonly List<FadingTile> tilesFadingIn = new();
    private readonly HashSet<Tile> knownVisibleTiles = new();

    public VisibilityRenderer(GameInstance game, Heightmap heightmap, ITimeSource timeSource)
    {
        time = timeSource;
        level = game.State.Level;
        visibilityLayer = game.State.VisibilityLayer;
        geometryLayer = game.State.GeometryLayer;

        heightmap.ResolutionChanged += () => needsFullRedraw = true;

        renderToHeightmap = heightmap.DrawVisibility(
            WithContext(
                c => c.SetBlendMode(BlendMode.Max),
                Do(renderVisibility)
            )
        );

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
        var now = time.Time;

        var seedTiles = getSeedTilesForZoneFadeIn(zone);

        foreach (var tile in zone.VisibilityTiles)
        {
            var distanceToSeedTile = seedTiles.Min(t => t.DistanceTo(tile));
            var delay = distanceToSeedTile * 0.1.S();
            var start = now + delay;
            var stop = start + 1.S();
            tilesFadingIn.Add(new FadingTile(tile, start, stop));
            knownVisibleTiles.Add(tile);
        }
    }

    private List<Tile> getSeedTilesForZoneFadeIn(Zone zone)
    {
        var zoneVisibilityTiles = zone.VisibilityTiles.ToImmutableArray();
        var seedTiles = zoneVisibilityTiles
            .Where(knownVisibleTiles.Contains)
            .Where(t => geometryLayer[t].Geometry.Type == TileType.Floor)
            .ToList();

        if (seedTiles.Count > 0)
            return seedTiles;

        var averageTilePosition = zoneVisibilityTiles
                .Select(t => Tiles.Level.GetPosition(t).NumericValue)
                .Aggregate(Vector2.Zero, (a, b) => a + b)
            / zoneVisibilityTiles.Length;
        seedTiles.Add(Tiles.Level.GetTile(new Position2(averageTilePosition)));

        return seedTiles;
    }

    public void Render()
    {
        if (needsFullRedraw)
        {
            fadeInAllVisibleTiles();
            needsFullRedraw = false;
        }

        if (tilesFadingIn.Count == 0)
            return;

        renderToHeightmap.Execute();
    }

    private void fadeInAllVisibleTiles()
    {
        var visibleTiles = Tilemap
            .EnumerateTilemapWith(level.Radius)
            .Where(t => visibilityLayer[t].IsVisible());

        var now = time.Time;

        foreach (var tile in visibleTiles)
        {
            var delay = tile.Radius * 0.1.S() + 0.75.S();
            var start = now + delay;
            var stop = start + 0.3.S();
            tilesFadingIn.Add(new FadingTile(tile, start, stop));
            knownVisibleTiles.Add(tile);
        }
    }

    private void renderVisibility()
    {
        var splat = splats.GetSprite("visibility-hex");

        var now = time.Time;

        foreach (var (tile, start, stop) in tilesFadingIn)
        {
            var p = Tiles.Level.GetPosition(tile).NumericValue.WithZ(0);

            var angle = tile.X * 1.3f + tile.Y * 2.9f;

            var t = (float)((now - start) / (stop - start)).Clamped(0, 1);

            splat.Draw(p, Constants.Game.World.HexagonSide * 0.75f, angle, (0, t));
        }

        splatRenderer.Render();
        splats.Clear();

        tilesFadingIn.RemoveAll(t => now > t.Stop);
    }
}
