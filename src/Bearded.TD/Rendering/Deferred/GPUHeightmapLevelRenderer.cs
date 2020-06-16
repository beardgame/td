using System;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.World;
using Bearded.TD.Meta;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static Bearded.TD.Constants.Game.World;
using static Bearded.TD.Tiles.Direction;

namespace Bearded.TD.Rendering.Deferred
{
    class GPUHeightmapLevelRenderer : LevelRenderer
    {
        private readonly int tileMapWidth;

        private float heightMapPixelsPerTile;
        private float gridVerticesPerTile;

        private readonly RenderContext context;
        private readonly Material material;
        private readonly Level level;
        private readonly GeometryLayer geometryLayer;
        private readonly float heightMapWorldSize;
        private readonly float fallOffDistance;

        private readonly FloatUniform heightmapRadiusUniform = new FloatUniform("heightmapRadius");
        private readonly FloatUniform heightmapPixelSizeUVUniform = new FloatUniform("heightmapPixelSizeUV");
        private readonly FloatUniform heightScaleUniform = new FloatUniform("heightScale");
        private readonly FloatUniform heightOffsetUniform = new FloatUniform("heightOffset");

        private readonly Texture heightmap;
        private readonly RenderTarget heightmapTarget; // H

        private readonly ExpandingVertexSurface<LevelVertex> gridSurface;
        private readonly PackedSpriteSet heightmapSplats;

        private int heightMapResolution;
        private float gridToWorld;
        private int gridRadius;

        private bool isHeightmapGenerated;

        public GPUHeightmapLevelRenderer(GameInstance game, RenderContext context, Material material)
            : base(game)
        {
            this.context = context;
            this.material = material;
            level = game.State.Level;
            geometryLayer = game.State.GeometryLayer;

            tileMapWidth = level.Radius * 2 + 1;
            heightMapWorldSize = tileMapWidth * HexagonWidth;

            fallOffDistance = (level.Radius - 0.25f) * HexagonWidth;

            heightmap = setupHeightmapTexture();
            heightmapTarget = new RenderTarget(heightmap);

            gridSurface = setupSurface(context);

            heightmapSplats = setupHeightmapSplats(game);

            resizeIfNeeded();
        }

        private void resizeIfNeeded()
        {
            var settings = UserSettings.Instance.Graphics;

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (heightMapPixelsPerTile != settings.TerrainHeightmapResolution)
            {
                resizeHeightmap(settings.TerrainHeightmapResolution);
            }

            if (gridVerticesPerTile != settings.TerrainMeshResolution)
            {
                resizeGrid(settings.TerrainMeshResolution);
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }
        private void resizeHeightmap(float scale)
        {
            heightMapPixelsPerTile = scale;
            heightMapResolution = (int) (tileMapWidth * scale);

            heightmap.Resize(heightMapResolution, heightMapResolution, PixelInternalFormat.R16f);
            heightmapRadiusUniform.Float = heightMapWorldSize * 0.5f;
            heightmapPixelSizeUVUniform.Float = 1f / heightMapResolution;

            isHeightmapGenerated = false;
        }

        private void resizeGrid(float scale)
        {
            gridVerticesPerTile = scale;

            var gridWidth = tileMapWidth * scale;
            gridRadius = (int) (gridWidth - 1) / 2;
            gridToWorld = 1 / scale;

            resizeSurface();
        }

        private void resizeSurface()
        {
            /* Vertex layout
             * -- v3
             *   /  \
             * v0 -- v2
             *   \  /
             * -- v1
             */

            gridSurface.Clear();

            generateGrid(
                (t0, t1, t2, t3, v0, v1, v2, v3) =>
                {
                    gridSurface.AddVertices(
                        vertex(v0.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White),
                        vertex(v1.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White),
                        vertex(v2.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White)
                    );

                    gridSurface.AddVertices(
                        vertex(v0.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White),
                        vertex(v2.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White),
                        vertex(v3.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White)
                    );
                }
            );
        }

        private PackedSpriteSet setupHeightmapSplats(GameInstance game)
        {
            var splats = game.Blueprints.Sprites[ModAwareId.ForDefaultMod("terrain-splats")].Sprites;
            splats.Surface.AddSetting(heightmapRadiusUniform);

            return splats;
        }

        private Texture setupHeightmapTexture()
        {
            var hm = new Texture();
            hm.SetParameters(
                TextureMinFilter.Linear, TextureMagFilter.Linear,
                TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge
                );
            return hm;
        }

        private ExpandingVertexSurface<LevelVertex> setupSurface(RenderContext context)
        {
            var s = new ExpandingVertexSurface<LevelVertex>()
            {
                ClearOnRender = false,
                IsStatic = true,
            }.WithShader(material.Shader.SurfaceShader)
                .AndSettings(
                    context.Surfaces.ViewMatrixLevel,
                    context.Surfaces.ProjectionMatrix,
                    context.Surfaces.FarPlaneDistance,
                    heightmapRadiusUniform,
                    heightmapPixelSizeUVUniform,
                    new TextureUniform("heightmap", heightmap),
                    context.Surfaces.CameraPosition,
                    heightScaleUniform,
                    heightOffsetUniform
                );

            var textureUnit = TextureUnit.Texture0;

            foreach (var (uniformName, texture) in material.ArrayTextures)
            {
                s.AddSetting(new ArrayTextureUniform(uniformName, texture, textureUnit));

                textureUnit++;
            }

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
            resizeIfNeeded();

            if (UserSettings.Instance.Debug.WireframeLevel)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                render();
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
            else
            {
                render();
            }
        }

        private void render()
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            heightScaleUniform.Float = 1;
            heightOffsetUniform.Float = 0;
            gridSurface.Render();

            GL.CullFace(CullFaceMode.Front);
            GL.FrontFace(FrontFaceDirection.Cw);
            heightScaleUniform.Float = -1;
            heightOffsetUniform.Float = 1.5f;
            gridSurface.Render();

            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Disable(EnableCap.CullFace);
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
            GL.Viewport(0, 0, heightMapResolution, heightMapResolution);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, heightmapTarget);

            GL.ClearColor(Constants.Rendering.WallHeight, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

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

        private delegate void GenerateTile(
            Tile t0, Tile t1, Tile t2, Tile t3,
            Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3
        );

        private void generateGrid(GenerateTile generateTile)
        {
            /* Vertex layout
             * -- v3
             *   /  \
             * v0 -- v2
             *   \  /
             * -- v1
             */

            var (v1Offset, v1Step) = (DownRight.Vector() * gridToWorld, DownRight.Step());
            var (v2Offset, v2Step) = (Right.Vector() * gridToWorld, Right.Step());
            var (v3Offset, v3Step) = (UpRight.Vector() * gridToWorld, UpRight.Step());

            foreach (var t0 in Tilemap.EnumerateTilemapWith(gridRadius - 1))
            {
                var t1 = t0.Offset(v1Step);
                var t2 = t0.Offset(v2Step);
                var t3 = t0.Offset(v3Step);

                var v0 = Level.GetPosition(t0).NumericValue * gridToWorld;
                var v1 = v0 + v1Offset;
                var v2 = v0 + v2Offset;
                var v3 = v0 + v3Offset;

                generateTile(t0, t1, t2, t3, v0, v1, v2, v3);
            }
        }

        private LevelVertex vertex(Vector3 v, Vector3 n, Vector2 uv, Color c)
        {
            var a = 1f; //(1 - Abs(v.Z * v.Z * 1f)).Clamped(0f, 1);

            var distanceFalloff = ((fallOffDistance - hexagonalDistanceToOrigin(v.Xy)) * 0.3f)
                .Clamped(0f, 1f).Squared();

            a *= distanceFalloff;

            return new LevelVertex(v, n, uv, new Color(c * a, c.A));
        }

        private static float hexagonalDistanceToOrigin(Vector2 xy)
        {
            var yf = xy.Y * (1 / HexagonDistanceY);
            var xf = xy.X * (1 / HexagonWidth) - yf * 0.5f;
            var x = Math.Abs(xf);
            var y = Math.Abs(yf);
            var reduction = Math.Sign(xf) != Math.Sign(yf) ? Math.Min(x, y) : 0f;
            return x + y - reduction;
        }
    }
}
