using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Pipelines.Context;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
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
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static Bearded.TD.Game.Simulation.World.TileType;
using ColorVertexData = Bearded.TD.Rendering.Vertices.ColorVertexData;
using Rectangle = System.Drawing.Rectangle;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Rendering.Deferred.Level
{
    using static Pipeline;
    using static Pipeline<int>;

    sealed class HeightmapRenderer
    {
        private readonly int splatSeedOffset;

        //TODO: organise fields
        private readonly int tileMapWidth;
        private float heightMapPixelsPerTile;
        private readonly Tiles.Level level;
        private readonly GeometryLayer geometryLayer;
        private readonly PassabilityLayer passabilityLayer;
        private readonly float heightMapWorldSize;
        public FloatUniform HeightmapRadiusUniform { get; } = new("heightmapRadius");
        public FloatUniform HeightmapPixelSizeUVUniform { get; } = new("heightmapPixelSizeUV");
        private readonly PipelineTexture heightmap;
        private readonly PipelineRenderTarget heightmapTarget; // H
        private readonly DrawableSpriteSet<HeightmapSplatVertex, (float MinH, float MaxH)> heightmapSplats;
        private readonly IRenderer heightMapSplatRenderer;
        private int heightMapResolution;
        private bool isHeightmapGenerated;
        private readonly IPipeline<int> renderHeightmapAtResolution;

        private readonly IndexedTrianglesMeshBuilder<ColorVertexData> heightmapBaseMeshBuilder;
        private readonly Renderer heightmapBaseRenderer;
        private readonly ShapeDrawer2<ColorVertexData, Void> heightmapBaseDrawer;

        public HeightmapRenderer(GameInstance game, RenderContext context)
        {
            splatSeedOffset = game.GameSettings.Seed;
            geometryLayer = game.State.GeometryLayer;
            passabilityLayer = game.State.PassabilityManager.GetLayer(Passability.WalkingUnit);

            level = game.State.Level;
            tileMapWidth = level.Radius * 2 + 1;
            heightMapWorldSize = tileMapWidth * Constants.Game.World.HexagonWidth;

            heightmap = Texture(PixelInternalFormat.R16f, setup: t =>
            {
                t.SetFilterMode(TextureMinFilter.Linear, TextureMagFilter.Linear);
                t.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            });

            heightmapTarget = RenderTargetWithColors(heightmap);

            renderHeightmapAtResolution =
                InOrder(
                    Resize(r => new Vector2i(r, r), heightmap),
                    WithContext(c => c
                            .BindRenderTarget(heightmapTarget)
                            .SetViewport(r => new Rectangle(0, 0, r, r))
                            .SetBlendMode(BlendMode.Premultiplied),
                        InOrder(
                            ClearColor(0, 0, 0, 0),
                            Do(renderHeightmap)
                        )
                    )
                );

            (heightmapSplats, heightMapSplatRenderer) = findHeightmapSplats(game);

            (heightmapBaseMeshBuilder, heightmapBaseRenderer, heightmapBaseDrawer) = initializeBaseDrawing(context);
        }

        private (IndexedTrianglesMeshBuilder<ColorVertexData>, Renderer, ShapeDrawer2<ColorVertexData, Void>)
            initializeBaseDrawing(RenderContext context)
        {
            var meshBuilder = new IndexedTrianglesMeshBuilder<ColorVertexData>();
            var baseRenderer = Renderer.From(meshBuilder.ToRenderable(), HeightmapRadiusUniform);
            var baseDrawer = new ShapeDrawer2<ColorVertexData, Void>(meshBuilder, (p, _) => new ColorVertexData(p, default));

            context.Shaders.GetShaderProgram("terrain-base").UseOnRenderer(baseRenderer);

            return (meshBuilder, baseRenderer, baseDrawer);
        }

        public TextureUniform GetHeightmapUniform(string name, TextureUnit unit)
        {
            return new(name, unit, heightmap.Texture);
        }

        public void SetScale(float scale)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (heightMapPixelsPerTile == scale)
            {
                return;
            }

            heightMapPixelsPerTile = scale;
            heightMapResolution = (int) (tileMapWidth * scale);

            HeightmapRadiusUniform.Value = heightMapWorldSize * 0.5f;
            HeightmapPixelSizeUVUniform.Value = 1f / heightMapResolution;

            isHeightmapGenerated = false;
        }

        public void CleanUp()
        {
            heightmapBaseMeshBuilder.Dispose();
            heightmapBaseRenderer.Dispose();

            heightmapTarget.Dispose();
            heightmap.Dispose();
        }

        private (DrawableSpriteSet<HeightmapSplatVertex, (float MinH, float MaxH)>, IRenderer) findHeightmapSplats(GameInstance game)
        {
            return game.Blueprints.Sprites[ModAwareId.ForDefaultMod("terrain-splats")]
                .MakeCustomRendererWith<HeightmapSplatVertex, (float MinH, float MaxH)>(
                    game.Meta.SpriteRenderers,
                    HeightmapSplatVertex.Create,
                    game.Meta.Blueprints.Shaders[ModAwareId.ForDefaultMod("heightmap-splatter")],
                    HeightmapRadiusUniform
                );
        }

        public void TileChanged(Tile tile)
        {
            // TODO: redraw only tiles that have changed instead of everything

            isHeightmapGenerated = false;
        }

        public void EnsureHeightmapIsUpToDate()
        {
            if (isHeightmapGenerated)
            {
                return;
            }

            renderHeightmapAtResolution.Execute(heightMapResolution);

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

                heightmapBaseDrawer.FillCircle(p, Constants.Game.World.HexagonSide, default, 6);

                count++;

                if (count > 10000)
                {
                    heightmapBaseRenderer.Render();
                    heightmapBaseMeshBuilder.Clear();
                    count = 0;
                }
            }

            heightmapBaseRenderer.Render();
            heightmapBaseMeshBuilder.Clear();
        }

        private static readonly (Direction Direction, Vector2 Offset, Vector2 UnitX, Vector2 UnitY)[] transitionDirections =
            new []{
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
                    var random = new Random((tile.X + tile.Y * level.Radius * 3) * 3 + (int)direction + splatSeedOffset);

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
                        (Crevice, Crevice) => smoothTransitions,
                        (Wall, _) => floorWallTransitions,
                        (_, Wall) => floorWallTransitions,
                        (Crevice, Floor) => creviceFloorTransitions,
                        (Floor, Crevice) => creviceFloorTransitions,
                        _ => throw new InvalidOperationException("Encountered unknown tile type.")
                    };

                    var splat = transitions.RandomElement(random);

                    var splatCenter = (p0.NumericValue + offset).WithZ(0);

                    var invertTransition = neighbourHeight < height;
                    var (uX, heights) = invertTransition
                        ? (-unitX, (neighbourHeight, height))
                        : (unitX, (height, neighbourHeight));
                    var uY = unitY * random.NextSign();

                    splat.Draw(splatCenter, uX, uY, heights);
                }
            }

            heightMapSplatRenderer.Render();
            heightmapSplats.Clear();

            List<DrawableSprite<HeightmapSplatVertex, (float, float)>> getSpritesWithPrefix(string prefix)
                => heightmapSplats.AllSprites.Where(s => s.Name.StartsWith(prefix)).Select(s => s.Sprite).ToList();
        }

        private static float tileHeight(DrawableTileGeometry info) =>
            info.Type switch
            {
                Wall => Constants.Rendering.WallHeight,
                _ => info.DrawInfo.Height.NumericValue
            };
    }
}
