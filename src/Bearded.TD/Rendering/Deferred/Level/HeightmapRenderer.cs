using System;
using System.Linq;
using amulware.Graphics;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Pipelines;
using amulware.Graphics.Pipelines.Context;
using amulware.Graphics.Rendering;
using amulware.Graphics.RenderSettings;
using amulware.Graphics.Shapes;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using ColorVertexData = Bearded.TD.Rendering.Vertices.ColorVertexData;
using Rectangle = System.Drawing.Rectangle;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Rendering.Deferred.Level
{
    sealed class HeightmapRenderer
    {
        //TODO: organise fields
        private readonly int tileMapWidth;
        private float heightMapPixelsPerTile;
        private readonly Tiles.Level level;
        private readonly GeometryLayer geometryLayer;
        private readonly float heightMapWorldSize;
        public FloatUniform HeightmapRadiusUniform { get; } = new("heightmapRadius");
        public FloatUniform HeightmapPixelSizeUVUniform { get; } = new("heightmapPixelSizeUV");
        private readonly PipelineTexture heightmap;
        private readonly PipelineRenderTarget heightmapTarget; // H
        private readonly PackedSpriteSet heightmapSplats;
        private readonly IRenderer heightMapSplatRenderer;
        private int heightMapResolution;
        private bool isHeightmapGenerated;
        private readonly IPipeline<int> renderHeightmapAtResolution;

        private readonly IndexedTrianglesMeshBuilder<ColorVertexData> heightmapBaseMeshBuilder;
        private readonly Renderer heightmapBaseRenderer;
        private readonly ShapeDrawer2<ColorVertexData, Void> heightmapBaseDrawer;

        public HeightmapRenderer(GameInstance game, RenderContext context)
        {
            geometryLayer = game.State.GeometryLayer;

            level = game.State.Level;
            tileMapWidth = level.Radius * 2 + 1;
            heightMapWorldSize = tileMapWidth * Constants.Game.World.HexagonWidth;

            heightmap = Pipeline.Texture(PixelInternalFormat.R16f, setup: t =>
            {
                t.SetFilterMode(TextureMinFilter.Linear, TextureMagFilter.Linear);
                t.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            });

            heightmapTarget = Pipeline.RenderTargetWithColors(heightmap);

            renderHeightmapAtResolution =
                Pipeline<int>.InOrder(
                    Pipeline<int>.Resize(r => new Vector2i(r, r), heightmap),
                    Pipeline<int>.WithContext(c => c
                            .BindRenderTarget(heightmapTarget)
                            .SetViewport(r => new Rectangle(0, 0, r, r))
                            .SetBlendMode(BlendMode.Premultiplied),
                        Pipeline<int>.InOrder(
                            Pipeline<int>.ClearColor(Constants.Rendering.WallHeight, 0, 0, 0),
                            Pipeline<int>.Do(_ => renderHeightmapInBatches())
                        )
                    )
                );

            heightmapSplats = findHeightmapSplats(game);
            heightMapSplatRenderer = heightmapSplats.CreateRendererWithSettings(HeightmapRadiusUniform);

            (heightmapBaseMeshBuilder, heightmapBaseRenderer, heightmapBaseDrawer) = initialiseBaseDrawing(context);
        }

        private (IndexedTrianglesMeshBuilder<ColorVertexData>, Renderer, ShapeDrawer2<ColorVertexData, Void>)
            initialiseBaseDrawing(RenderContext context)
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

        public void Resize(float scale)
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

        private static PackedSpriteSet findHeightmapSplats(GameInstance game)
        {
            return game.Blueprints.Sprites[ModAwareId.ForDefaultMod("terrain-splats")].Sprites;
        }

        public void EnsureHeightmapIsUpToDate()
        {
            if (isHeightmapGenerated)
            {
                // TODO: redraw tiles that have changed if any

                return;
            }

            renderHeightmapAtResolution.Execute(heightMapResolution);

            isHeightmapGenerated = true;
        }

        private void renderHeightmapInBatches()
        {
            // TODO:
            // - implement hard edge base layer using solid geometry and no textures
            //   - core shader + simple coloured vertex
            // - create simple smooth and stepped transition splat sprites
            // - use splats to render all tile transitions (try different orders)
            // - make sure cliffs read easily (do we have to turn the grid by 30 degrees to line up with tile edges?)
            // - make it prettier (try some more detailed sprites, add some variations, try different sprites for different transition types)

            var allTiles = Tilemap.EnumerateTilemapWith(level.Radius).Select(t => (Tile: t, Info: geometryLayer[t]));

            var count = 0;
            foreach (var (tile, info) in allTiles)
            {
                var p = Tiles.Level.GetPosition(tile).NumericValue
                    .WithZ(info.DrawInfo.Height.NumericValue);

                heightmapBaseDrawer.FillCircle(p, Constants.Game.World.HexagonSide, default, 6);

                count++;

                if (count > 10000)
                {
                    heightmapBaseRenderer.Render();
                    heightmapBaseMeshBuilder.Clear();
                    count = 0;
                }
            }

            heightMapSplatRenderer.Render();
            heightmapSplats.MeshBuilder.Clear();
        }
    }
}
