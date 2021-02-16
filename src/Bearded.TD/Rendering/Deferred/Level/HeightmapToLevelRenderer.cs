using System;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Meta;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Deferred.Level
{
    sealed class HeightmapToLevelRenderer
    {
        //TODO: organise fields
        private readonly int tileMapWidth;
        private float gridVerticesPerTile;
        private readonly Material material;
        private readonly float fallOffDistance;
        private readonly FloatUniform heightScaleUniform = new("heightScale");
        private readonly FloatUniform heightOffsetUniform = new("heightOffset");
        private readonly ExpandingIndexedTrianglesMeshBuilder<LevelVertex> gridMeshBuilder;
        private readonly IRenderer gridRenderer;
        private float gridToWorld;
        private int gridRadius;

        public HeightmapToLevelRenderer(
            GameInstance game, RenderContext context, Material material,
            HeightmapRenderer heightmapRenderer)
        {
            var level = game.State.Level;
            tileMapWidth = level.Radius * 2 + 1;
            this.material = material;

            fallOffDistance = (level.Radius - 0.25f) * Constants.Game.World.HexagonWidth;

            (gridMeshBuilder, gridRenderer) = setupGridRenderer(context, heightmapRenderer);
        }

        public void CleanUp()
        {
            gridMeshBuilder.Dispose();
            gridRenderer.Dispose();
        }

        public void RenderAll()
        {
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

        public void Resize(float scale)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (gridVerticesPerTile == scale)
            {
                return;
            }

            gridVerticesPerTile = scale;

            var gridWidth = tileMapWidth * scale;
            gridRadius = (int) (gridWidth - 1) / 2;
            gridToWorld = 1 / scale;

            rebuildMesh();
        }


        private void rebuildMesh()
        {
            /* Vertex layout
             * -- v3
             *   /  \
             * v0 -- v2
             *   \  /
             * -- v1
             */

            gridMeshBuilder.Clear();

            // TODO: this can be optimised, since we are not reusing indices
            generateGrid(
                (_, _, _, _, v0, v1, v2, v3) =>
                {
                    gridMeshBuilder.AddTriangle(
                        vertex(v0.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White),
                        vertex(v1.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White),
                        vertex(v2.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White)
                    );

                    gridMeshBuilder.AddTriangle(
                        vertex(v0.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White),
                        vertex(v2.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White),
                        vertex(v3.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White)
                    );
                }
            );
        }

        private (ExpandingIndexedTrianglesMeshBuilder<LevelVertex>, IRenderer) setupGridRenderer(
            RenderContext context, HeightmapRenderer heightmapRenderer)
        {
            var meshBuilder = new ExpandingIndexedTrianglesMeshBuilder<LevelVertex>();

            var renderer = BatchedRenderer.From(meshBuilder.ToRenderable(),
                new IRenderSetting[]
                {
                    context.Settings.ViewMatrixLevel,
                    context.Settings.ProjectionMatrix,
                    context.Settings.FarPlaneDistance,
                    heightmapRenderer.HeightmapRadiusUniform,
                    heightmapRenderer.HeightmapPixelSizeUVUniform,
                    heightmapRenderer.GetHeightmapUniform("heightmap", TextureUnit.Texture0),
                    context.Settings.CameraPosition,
                    heightScaleUniform,
                    heightOffsetUniform
                }.Concat(material.ArrayTextures.Select((t, i) =>
                    new ArrayTextureUniform(t.UniformName!, TextureUnit.Texture0 + i + 1, t.Texture!))));

            material.Shader.RendererShader.UseOnRenderer(renderer);

            return (meshBuilder, renderer);
        }

        private void render()
        {
            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Back);
            heightScaleUniform.Value = 1;
            heightOffsetUniform.Value = 0;
            gridRenderer.Render();

            //GL.Disable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);
            heightScaleUniform.Value = -1;
            heightOffsetUniform.Value = 1.5f;
            gridRenderer.Render();

            GL.FrontFace(FrontFaceDirection.Ccw);
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

            var (v1Offset, v1Step) = (Direction.DownRight.Vector() * gridToWorld, Direction.DownRight.Step());
            var (v2Offset, v2Step) = (Direction.Right.Vector() * gridToWorld, Direction.Right.Step());
            var (v3Offset, v3Step) = (Direction.UpRight.Vector() * gridToWorld, Direction.UpRight.Step());

            foreach (var t0 in Tilemap.EnumerateTilemapWith(gridRadius - 1))
            {
                var t1 = t0.Offset(v1Step);
                var t2 = t0.Offset(v2Step);
                var t3 = t0.Offset(v3Step);

                var v0 = Tiles.Level.GetPosition(t0).NumericValue * gridToWorld;
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

            return new LevelVertex(v, n, uv, (c * a).WithAlpha(c.A));
        }

        private static float hexagonalDistanceToOrigin(Vector2 xy)
        {
            var yf = xy.Y * (1 / Constants.Game.World.HexagonDistanceY);
            var xf = xy.X * (1 / Constants.Game.World.HexagonWidth) - yf * 0.5f;
            var x = Math.Abs(xf);
            var y = Math.Abs(yf);
            var reduction = Math.Sign(xf) != Math.Sign(yf) ? Math.Min(x, y) : 0f;
            return x + y - reduction;
        }
    }
}
