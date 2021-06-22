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
        private readonly int tileMapWidth;

        private int gridRadius;
        private float gridVerticesPerTile;
        private float gridToWorld;

        private readonly Material material;
        private readonly FloatUniform heightScaleUniform = new("heightScale");
        private readonly FloatUniform heightOffsetUniform = new("heightOffset");

        private readonly ExpandingIndexedTrianglesMeshBuilder<LevelVertex> gridMeshBuilder;
        private readonly IRenderer gridRenderer;

        public HeightmapToLevelRenderer(
            GameInstance game, RenderContext context, Material material,
            HeightmapRenderer heightmapRenderer)
        {
            var level = game.State.Level;
            tileMapWidth = level.Radius * 2 + 1;
            this.material = material;

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

            var v1Offset = Direction.DownRight.Vector() * gridToWorld;
            var v2Offset = Direction.Right.Vector() * gridToWorld;
            var v3Offset = Direction.UpRight.Vector() * gridToWorld;

            foreach (var t0 in Tilemap.EnumerateTilemapWith(gridRadius - 1))
            {
                var v0 = Tiles.Level.GetPosition(t0).NumericValue * gridToWorld;
                var v1 = v0 + v1Offset;
                var v2 = v0 + v2Offset;
                var v3 = v0 + v3Offset;

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

            static LevelVertex vertex(Vector3 v, Vector3 n, Vector2 uv, Color c)
            {
                return new(v, n, uv, c);
            }
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

    }
}
