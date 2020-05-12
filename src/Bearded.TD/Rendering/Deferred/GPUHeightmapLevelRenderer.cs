using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.Deferred
{
    class GPUHeightmapLevelRenderer : HeightmapLevelRenderer
    {
        private static float wallHeight = 5;

        private readonly Texture heightmap;
        private readonly RenderTarget heightmapTarget; // H

        private readonly ExpandingVertexSurface<LevelVertex> gridSurface;

        private bool isHeightmapGenerated;

        public GPUHeightmapLevelRenderer(GameInstance game, RenderContext context, Material material)
            : base(game, context, material)
        {
            heightmap = setupTexture();
            heightmapTarget = new RenderTarget(heightmap);

            gridSurface = setupSurface();

            // TODO: move terrain heights and normal generation to vertex shader
            //     - can we reuse the same textures for multiple materials with different shaders?
            // TODO: load sprites for tiles/details and start using them
        }

        private Texture setupTexture()
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
                        Vertex(v0.WithZ(), Vector3.Zero, Vector2.Zero, Color.White),
                        Vertex(v2.WithZ(), Vector3.Zero, Vector2.Zero, Color.White),
                        Vertex(v1.WithZ(), Vector3.Zero, Vector2.Zero, Color.White)
                    );

                    s.AddVertices(
                        Vertex(v0.WithZ(), Vector3.Zero, Vector2.Zero, Color.White),
                        Vertex(v3.WithZ(), Vector3.Zero, Vector2.Zero, Color.White),
                        Vertex(v2.WithZ(), Vector3.Zero, Vector2.Zero, Color.White)
                    );
                }
                );

            return s;
        }

        protected override void OnTileChanged(Tile tile)
        {
            // TODO: mark tile to be redrawn
        }

        public override void RenderAll()
        {
            ensureHeightmap();

            gridSurface.Render();
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

            // TODO: draw all tiles

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        }

        public override void CleanUp()
        {
            heightmapTarget.Dispose();
            heightmap.Dispose();
        }
    }
}
