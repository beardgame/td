using System;
using System.Diagnostics;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Meta;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static System.Math;

namespace Bearded.TD.Rendering.Deferred.Level
{
    sealed class HeightmapToLevelRenderer
    {
        private readonly int tileMapWidth;

        private float gridScaleSetting;

        private readonly Material material;
        private readonly FloatUniform heightScaleUniform = new("heightScale");
        private readonly FloatUniform heightOffsetUniform = new("heightOffset");
        private readonly Vector2Uniform gridOffsetUniform = new("gridOffset");
        private readonly Vector2Uniform gridScaleUniform = new("gridScale");

        private readonly RhombusGridMesh gridMeshBuilder;
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

        public void Resize(float scale)
        {
            gridScaleSetting = scale;
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

        private void render()
        {
            var widthToCover = tileMapWidth * 0.5f * Constants.Game.World.HexagonDistanceX;
            var heightToCover = tileMapWidth * 0.5f * Constants.Game.World.HexagonDistanceY;

            var gridMeshWidth = gridMeshBuilder.TilingX.X;
            var gridMeshHeight = gridMeshBuilder.TilingY.Y;

            // TODO: base scale should be revisited once/if we add tesselation
            // other ideas: scale it so that the lowest res grid always 'just' encapsulates the level,
            // instead of having large empty edges around it
            const float baseScale = 1f;
            var scale = baseScale / gridScaleSetting;

            var cellWidth = scale * gridMeshWidth;
            var cellHeight = scale * gridMeshHeight;

            var cellColumnsHalf = MoreMath.CeilToInt(widthToCover / cellWidth);
            var cellRowsHalf = MoreMath.CeilToInt(heightToCover / cellHeight);

            gridScaleUniform.Value = new Vector2(scale);

            for (var y = -cellRowsHalf; y < cellRowsHalf; y++)
            {
                var xMin = Max(-cellColumnsHalf, -cellColumnsHalf - y - 1);
                var xMax = Min(cellColumnsHalf, cellColumnsHalf - y);

                for (var x = xMin; x < xMax; x++)
                {
                    gridOffsetUniform.Value = (x * gridMeshBuilder.TilingX + y * gridMeshBuilder.TilingY) * scale;
                    renderSingleGridCell();
                }
            }
        }

        private void renderSingleGridCell()
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

        private (RhombusGridMesh, IRenderer) setupGridRenderer(RenderContext context, HeightmapRenderer heightmapRenderer)
        {
            var mesh = RhombusGridMesh.CreateDefault();

            var renderer = Renderer.From(mesh.ToRenderable(),
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
                    heightOffsetUniform,
                    gridOffsetUniform,
                    gridScaleUniform
                }.Concat(material.ArrayTextures.Select((t, i) =>
                    new ArrayTextureUniform(t.UniformName!, TextureUnit.Texture0 + i + 1, t.Texture!))));

            material.Shader.RendererShader.UseOnRenderer(renderer);

            return (mesh, renderer);
        }


    }

    sealed class RhombusGridMesh : IDisposable
    {
        private readonly IndexedTrianglesMeshBuilder<LevelVertex> meshBuilder;

        public Vector2 TilingX { get; }
        public Vector2 TilingY { get; }

        public static RhombusGridMesh CreateDefault()
        {
            var (meshBuilder, tilingX, tilingY) = buildRhombusMesh();

            return new RhombusGridMesh(meshBuilder, tilingX, tilingY);
        }

        private RhombusGridMesh(
            IndexedTrianglesMeshBuilder<LevelVertex> meshBuilder, Vector2 tilingX, Vector2 tilingY)
        {
            this.meshBuilder = meshBuilder;
            TilingX = tilingX;
            TilingY = tilingY;
        }

        public IRenderable ToRenderable() => meshBuilder.ToRenderable();

        public void Dispose() => meshBuilder.Dispose();

        private static (IndexedTrianglesMeshBuilder<LevelVertex> meshBuilder, Vector2 tilingX, Vector2 tilingY)
            buildRhombusMesh()
        {
            /* Rhombus section (2x2 example)
             *     y
             *    ^ 0,h--*--w,h
             *   /  / \ / \ /
             *  /  *---*---*
             *    / \ / \ /
             *  0,0--*--w,0
             *      ------->x
             * we address vertices with [x * h + y]
             */

            // vertices of n*n rhombus: v = (n + 1)^2
            // inverted: n = sqrt(v) - 1
            var maxVertexCount = ushort.MaxValue / 16;
            var rhombusSideLength = (int)(0.5 * Sqrt(4 * maxVertexCount + 9) - 3);

            var vertexArrayWidth = rhombusSideLength + 1;
            var vertexArrayHeight = rhombusSideLength + 2;

            var vertexCount = vertexArrayWidth * vertexArrayHeight;
            var triangleCount = rhombusSideLength * rhombusSideLength * 2;
            var indexCount = triangleCount * 3;

            var meshBuilder = new IndexedTrianglesMeshBuilder<LevelVertex>();
            meshBuilder.Add(vertexCount, indexCount, out var vertices, out var indices, out var indexOffset);

            DebugAssert.State.Satisfies(indexOffset == 0);

            var stepX = Direction.Right.Vector();
            var stepY = Direction.UpRight.Vector();

            var tilingX = stepX * rhombusSideLength;
            var tilingY = stepY * rhombusSideLength;

            for (var x = 0; x < rhombusSideLength + 1; x++)
            {
                for (var y = 0; y < rhombusSideLength + 1; y++)
                {
                    var p = x * stepX + y * stepY;
                    vertices[vertexIndex(x, y)] = vertex(p.WithZ(), Vector3.UnitZ, Vector2.Zero, Color.White);
                }
            }

            /* Vertex layout
             *    v2 -- v3
             *   /  \  /
             * v0 -- v1
             */

            var i = 0;
            for (var x = 0; x < rhombusSideLength; x++)
            {
                for (var y = 0; y < rhombusSideLength; y++)
                {
                    // TODO: could use triangle strips here to cut index count in by 2/3
                    var v0 = vertexIndex(x, y);
                    var v1 = vertexIndex(x + 1, y);
                    var v2 = vertexIndex(x, y + 1);
                    var v3 = vertexIndex(x + 1, y + 1);

                    indices[i++] = v0;
                    indices[i++] = v1;
                    indices[i++] = v2;

                    indices[i++] = v1;
                    indices[i++] = v3;
                    indices[i++] = v2;
                }
            }

            return (meshBuilder, tilingX, tilingY);

            ushort vertexIndex(int x, int y)
            {
                return (ushort)(x * vertexArrayHeight + y);
            }

            static LevelVertex vertex(Vector3 v, Vector3 n, Vector2 uv, Color c)
            {
                return new(v, n, uv, c);
            }
        }
    }
}
