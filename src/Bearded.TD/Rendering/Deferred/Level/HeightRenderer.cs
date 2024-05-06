using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Pipelines.Context;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.Shapes;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using OpenTK.Mathematics;
using static Bearded.TD.Game.Simulation.World.TileType;
using ColorVertexData = Bearded.TD.Rendering.Vertices.ColorVertexData;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Rendering.Deferred.Level;

using static Pipeline<Void>;

sealed class HeightRenderer
{
    private readonly Tiles.Level level;
    private readonly GeometryLayer geometryLayer;
    private readonly PassabilityLayer passabilityLayer;
    private readonly int splatSeedOffset;

    private readonly DrawableSpriteSet<HeightmapSplatVertex, (float MinH, float MaxH)> splats;
    private readonly IRenderer splatRenderer;
    private readonly IPipeline<Void> renderToHeightmap;

    private readonly IndexedTrianglesMeshBuilder<ColorVertexData> baseMeshBuilder;
    private readonly Renderer baseRenderer;
    private readonly ShapeDrawer2<ColorVertexData, Void> baseDrawer;

    private bool isHeightmapGenerated;

    public HeightRenderer(GameInstance game, RenderContext context, Heightmap heightmap)
    {
        level = game.State.Level;
        geometryLayer = game.State.GeometryLayer;
        passabilityLayer = game.State.PassabilityObserver.GetLayer(Passability.WalkingUnit);
        splatSeedOffset = game.GameSettings.Seed;

        heightmap.ResolutionChanged += () => isHeightmapGenerated = false;

        renderToHeightmap =
            heightmap.DrawHeights(
                WithContext(
                    c => c.SetBlendMode(BlendMode.Premultiplied),
                    InOrder(
                        ClearColor(0, 0, 0, 0),
                        Do(renderHeightmap)
                    )));

        (splats, splatRenderer) = findHeightmapSplats(game, heightmap);
        (baseMeshBuilder, baseRenderer, baseDrawer) = initializeBaseDrawing(context, heightmap);
    }

    private static (IndexedTrianglesMeshBuilder<ColorVertexData>, Renderer, ShapeDrawer2<ColorVertexData, Void>)
        initializeBaseDrawing(RenderContext context, Heightmap heightmap)
    {
        var meshBuilder = new IndexedTrianglesMeshBuilder<ColorVertexData>();
        var renderer = Renderer.From(meshBuilder.ToRenderable(), heightmap.RadiusUniform);
        var drawer =
            new ShapeDrawer2<ColorVertexData, Void>(meshBuilder, (p, _) => new ColorVertexData(p, default));

        context.Shaders.GetShaderProgram("terrain-base").UseOnRenderer(renderer);

        return (meshBuilder, renderer, drawer);
    }

    private static (DrawableSpriteSet<HeightmapSplatVertex, (float MinH, float MaxH)>, IRenderer)
        findHeightmapSplats(GameInstance game, Heightmap heightmap)
    {
        return game.Blueprints.Sprites[ModAwareId.ForDefaultMod("terrain-splats")]
            .MakeCustomRendererWith<HeightmapSplatVertex, (float MinH, float MaxH)>(
                game.Meta.DrawableRenderers,
                HeightmapSplatVertex.Create,
                game.Meta.Blueprints.Shaders[ModAwareId.ForDefaultMod("heightmap-splatter")],
                heightmap.RadiusUniform
            );
    }

    public void CleanUp()
    {
        baseMeshBuilder.Dispose();
        baseRenderer.Dispose();
    }

    public void TileChanged(Tile tile)
    {
        // TODO: redraw only tiles that have changed instead of everything

        isHeightmapGenerated = false;
    }

    public void Render()
    {
        if (isHeightmapGenerated)
        {
            return;
        }

        renderToHeightmap.Execute();

        isHeightmapGenerated = true;
    }

    private void renderHeightmap()
    {
        renderTileBaseHeights();
        renderTransitions();
    }

    private void renderTileBaseHeights()
    {
        var allTiles = Tilemap.EnumerateTilemapWith(level.Radius).Select(t => (Tile: t, Info: geometryLayer[t]));

        var count = 0;
        foreach (var (tile, info) in allTiles)
        {
            var p = Tiles.Level.GetPosition(tile).NumericValue
                .WithZ(tileHeight(info));

            baseDrawer.FillCircle(p, Constants.Game.World.HexagonSide, default, 6);

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

    private static readonly (Direction Direction, Vector2 Offset, Vector2 UnitX, Vector2 UnitY)[]
        transitionDirections =
            new[]
            {
                Direction.Right,
                Direction.UpRight,
                Direction.UpLeft,
            }.Select(d => (
                Direction: d,
                Offset: d.Vector() * Constants.Game.World.HexagonWidth * 0.5f,
                UnitX: d.Vector() * Constants.Game.World.HexagonWidth * 0.5f,
                UnitY: d.Vector().PerpendicularLeft * Constants.Game.World.HexagonDiameter * 0.5f
            )).ToArray();


    private void renderTransitions()
    {
        var smoothTransitions = getSpritesWithPrefix("transition-smooth");
        var harshTransitions = getSpritesWithPrefix("transition-harsh");
        var floorWallTransitions = getSpritesWithPrefix("transition-floor-wall");
        var creviceFloorTransitions = getSpritesWithPrefix("transition-crevice-floor");

        var allTiles = Tilemap.EnumerateTilemapWith(level.Radius).Select(t => (Tile: t, Info: geometryLayer[t]));

        foreach (var (tile, info) in allTiles)
        {
            var p0 = Tiles.Level.GetPosition(tile);
            var passableDirections = passabilityLayer[tile].PassableDirections;
            var height = tileHeight(info);

            foreach (var (direction, offset, unitX, unitY) in transitionDirections)
            {
                var random =
                    new Random((tile.X + tile.Y * level.Radius * 3) * 3 + (int)direction + splatSeedOffset);

                var neighbour = tile.Neighbor(direction);

                if (!level.IsValid(neighbour))
                    continue;

                var neighbourInfo = geometryLayer[neighbour];
                var neighbourHeight = tileHeight(neighbourInfo);

                var transitions = (info.Type, neighbourInfo.Type) switch
                {
                    (Floor, Floor) when passableDirections.Includes(direction) => smoothTransitions,
                    (Floor, Floor) => harshTransitions,
                    (Wall, Wall) => smoothTransitions,
                    (Crevice, Crevice) => creviceFloorTransitions,
                    (Wall, _) => floorWallTransitions,
                    (_, Wall) => floorWallTransitions,
                    (Crevice, Floor) => creviceFloorTransitions,
                    (Floor, Crevice) => creviceFloorTransitions,
                    _ => throw new InvalidOperationException("Encountered unknown tile type.")
                };

                var invertTransition = neighbourHeight < height;
                var (uX, heights) = invertTransition
                    ? (-unitX, (neighbourHeight, height))
                    : (unitX, (height, neighbourHeight));
                var uY = unitY * random.NextSign();

                var splatCenter = (p0.NumericValue + offset).WithZ(0);
                var splat = transitions.RandomElement(random);
                splat.Draw(splatCenter, uX, uY, heights);
            }
        }

        splatRenderer.Render();
        splats.Clear();

        List<DrawableSprite<HeightmapSplatVertex, (float, float)>> getSpritesWithPrefix(string prefix)
            => splats.AllSprites.Where(s => s.Name.StartsWith(prefix)).Select(s => s.Sprite).ToList();
    }

    private static float tileHeight(DrawableTileGeometry info) =>
        info.Type switch
        {
            Wall => Constants.Rendering.WallHeight,
            _ => info.DrawInfo.Height.NumericValue
        };
}
