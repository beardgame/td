using System;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.World;
using Bearded.TD.Meta;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Rendering.Deferred
{
    class GPUHeightmapLevelRenderer : HeightmapLevelRenderer
    {
        private static float wallHeight = 1f;

        private readonly Texture heightmap;
        private readonly RenderTarget heightmapTarget; // H

        private readonly ExpandingVertexSurface<LevelVertex> gridSurface;
        private readonly PackedSpriteSet heightmapSplats;

        private bool isHeightmapGenerated;

        public GPUHeightmapLevelRenderer(GameInstance game, RenderContext context, Material material)
            : base(game, context, material)
        {
            heightmap = setupHeightmapTexture();
            heightmapTarget = new RenderTarget(heightmap);

            gridSurface = setupSurface();

            heightmapSplats = setupHeightmapSplats(game);


            // TODO: add normal generation to vertex shader
            // TODO: can we reuse the same textures for multiple materials with different shaders?
            //     - right now the gpu shader is hard-coded in the material
            // TODO: figure out how to better use splats from mod
            // -> discuss these content pipeline points
        }

        private PackedSpriteSet setupHeightmapSplats(GameInstance game)
        {
            var splats = game.Blueprints.Sprites["terrain-splats"].Sprites;
            splats.Surface.AddSetting(new FloatUniform("heightmapRadius", HeightMapWorldSize * 0.5f));

            return splats;
        }

        private Texture setupHeightmapTexture()
        {
            var hm = new Texture(
                HeightMapResolution, HeightMapResolution,
                PixelInternalFormat.R16f);
            hm.SetParameters(
                TextureMinFilter.Linear, TextureMagFilter.Linear,
                TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge
                );
            return hm;
        }

        private ExpandingVertexSurface<LevelVertex> setupSurface()
        {
            // TODO: use smaller vertices with positions only
            //     - later these can contain biome info, etc.

            var s = new ExpandingVertexSurface<LevelVertex>()
            {
                ClearOnRender = false,
                IsStatic = true,
            };
            UseMaterialOnSurface(s);
            s.AddSettings(
                new FloatUniform("heightmapRadius", HeightMapWorldSize * 0.5f),
                new TextureUniform("heightmap", heightmap),
                new FloatUniform("heightmapPixelSizeUV", 1f / HeightMapResolution)
                );

            /* Vertex layout
             * -- v3
             *   /  \
             * v0 -- v2
             *   \  /
             * -- v1
             */

            GenerateGrid(
                (t0, t1, t2, t3, v0, v1, v2, v3) =>
                {
                    s.AddVertices(
                        Vertex(v0.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White),
                        Vertex(v2.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White),
                        Vertex(v1.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White)
                    );

                    s.AddVertices(
                        Vertex(v0.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White),
                        Vertex(v3.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White),
                        Vertex(v2.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White)
                    );
                }
                );

            return s;
        }

        protected override void OnTileChanged(Tile tile)
        {
            // TODO: mark tile to be redrawn
        }

        public override void PrepareForRender()
        {
            ensureHeightmap();
        }

        public override void RenderAll()
        {
            if (UserSettings.Instance.Debug.WireframeLevel)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                gridSurface.Render();
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
            else
            {
                gridSurface.Render();
            }
        }

        private void ensureHeightmap()
        {
            if (isHeightmapGenerated)
            {
                // TODO: redraw tiles that have changed if any

                return;
            }

            redrawHeightmap();

            isHeightmapGenerated = true;
        }

        private void redrawHeightmap()
        {
            GL.Viewport(0, 0, HeightMapResolution, HeightMapResolution);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, heightmapTarget);

            GL.ClearColor(wallHeight, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            var splat = heightmapSplats.GetSprite("splat-hex");

            var allTiles = Tilemap.EnumerateTilemapWith(Level.Radius).Select(t => (Tile: t, Info: GeometryLayer[t]));

            var count = 0;
            foreach (var (tile, info) in
                Enumerable.Concat(
                    allTiles.Where(t => t.Info.Type == TileType.Crevice),
                    allTiles.Where(t => t.Info.Type == TileType.Floor)
                    )
                )
            {
                var p = Level.GetPosition(tile).NumericValue
                    .WithZ(info.DrawInfo.Height.NumericValue);

                var size = HexagonWidth * 2 / Math.Max(splat.BaseSize.X, splat.BaseSize.Y);

                var angle = StaticRandom.Int(0, 6) * 30.Degrees().Radians;

                splat.Draw(p, Color.White, size, angle);

                count++;

                if (count > 10000)
                {
                    heightmapSplats.Surface.Render();
                    count = 0;
                }
            }

            heightmapSplats.Surface.Render();

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        }

        public override void CleanUp()
        {
            heightmapTarget.Dispose();
            heightmap.Dispose();
        }
    }
}
