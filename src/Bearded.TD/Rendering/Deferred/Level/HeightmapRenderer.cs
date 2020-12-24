using System;
using System.Linq;
using amulware.Graphics;
using amulware.Graphics.Pipelines;
using amulware.Graphics.Pipelines.Context;
using amulware.Graphics.Rendering;
using amulware.Graphics.RenderSettings;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using OpenToolkit.Graphics.OpenGL;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering.Deferred.Level
{
    sealed class HeightmapRenderer
    {
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

        public HeightmapRenderer(GameInstance game)
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
                            .SetViewport(r => new(0, 0, r, r))
                            .SetBlendMode(BlendMode.Premultiplied),
                        Pipeline<int>.InOrder(
                            Pipeline<int>.ClearColor(Constants.Rendering.WallHeight, 0, 0, 0),
                            Pipeline<int>.Do(_ => renderHeightmapInBatches())
                        )
                    )
                );

            heightmapSplats = findHeightmapSplats(game);
            heightMapSplatRenderer = heightmapSplats.CreateRendererWithSettings(HeightmapRadiusUniform);
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
            //   - surface man shader + simple coloured vertex
            // - create simple smooth and stepped transition splat sprites
            // - use splats to render all tile transitions (try different orders)
            // - make sure cliffs read easily (do we have to turn the grid by 30 degrees to line up with tile edges?)
            // - make it prettier (try some more detailed sprites, add some variations, try different sprites for different transition types)

            var splat = heightmapSplats.GetSprite("splat-hex");

            var allTiles = Tilemap.EnumerateTilemapWith(level.Radius).Select(t => (Tile: t, Info: geometryLayer[t]));

            var count = 0;
            foreach (var (tile, info) in
                // ReSharper disable PossibleMultipleEnumeration
                // ReSharper disable once InvokeAsExtensionMethod
                Enumerable.Concat(
                    allTiles.Where(t => t.Info.Type == TileType.Crevice),
                    allTiles.Where(t => t.Info.Type == TileType.Floor)
                )
                // ReSharper restore PossibleMultipleEnumeration
            )
            {
                var p = Tiles.Level.GetPosition(tile).NumericValue
                    .WithZ(info.DrawInfo.Height.NumericValue);

                var size = Constants.Game.World.HexagonWidth * 2 / Math.Max(splat.BaseSize.X, splat.BaseSize.Y);

                var angle = StaticRandom.Int(0, 6) * 60.Degrees();

                splat.Draw(p, Color.White, size, angle.Radians);

                count++;

                if (count > 10000)
                {
                    heightMapSplatRenderer.Render();
                    heightmapSplats.MeshBuilder.Clear();
                    count = 0;
                }
            }

            heightMapSplatRenderer.Render();
            heightmapSplats.MeshBuilder.Clear();
        }
    }
}
