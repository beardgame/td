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
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static System.Math;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Rendering.Deferred.Level
{
    sealed class HeightmapToLevelRenderer
    {

        private readonly CoreRenderSettings renderSettings;
        private readonly Material material;
        private readonly int tileMapWidth;

        private readonly FloatUniform heightScaleUniform = new("heightScale");
        private readonly FloatUniform heightOffsetUniform = new("heightOffset");
        private readonly Vector2Uniform gridOffsetUniform = new("gridOffset");
        private readonly Vector2Uniform gridScaleUniform = new("gridScale");

        private readonly RhombusGridMesh gridMeshBuilder;
        private readonly IRenderer gridRenderer;

        private float gridScaleSetting;

        public HeightmapToLevelRenderer(
            GameInstance game, RenderContext context, Material material,
            HeightmapRenderer heightmapRenderer)
        {
            renderSettings = context.Settings;
            this.material = material;
            var level = game.State.Level;
            tileMapWidth = level.Radius * 2 + 1;

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
            var cameraFrustumBounds = getCameraFrustumBounds();

            // ensure minimum level of detail based on graphics settings
            var (cellColumnsHalf, cellRowsHalf) = getMinimumGridDimensions();

            var scale = (tileMapWidth * 0.5f * HexagonDistanceX) / cellColumnsHalf / gridMeshBuilder.TilingX.X;

            var gridSubdivision = getGridSubdivision(cameraFrustumBounds, scale);

            scale /= gridSubdivision;
            cellColumnsHalf *= gridSubdivision;
            cellRowsHalf *= gridSubdivision;

            gridScaleUniform.Value = new Vector2(scale);

            var cellsDrawn = 0;

            var tilingX = gridMeshBuilder.TilingX * scale;
            var tilingY = gridMeshBuilder.TilingY * scale;
            var cellVisibleWidth = tilingX.X + tilingY.X;

            for (var y = -cellRowsHalf; y < cellRowsHalf; y++)
            {
                var rowMin = y * tilingY.Y;
                var rowMax = rowMin + tilingY.Y;

                if (rowMin > cameraFrustumBounds.Bottom || rowMax < cameraFrustumBounds.Top)
                    continue;

                var xMin = Max(-cellColumnsHalf, -cellColumnsHalf - y - 1);
                var xMax = Min(cellColumnsHalf, cellColumnsHalf - y);

                for (var x = xMin; x < xMax; x++)
                {
                    var offset = x * tilingX + y * tilingY;

                    if (offset.X > cameraFrustumBounds.Right || offset.X + cellVisibleWidth < cameraFrustumBounds.Left)
                        continue;

                    gridOffsetUniform.Value = offset;
                    renderSingleGridCell();
                    cellsDrawn++;
                }
            }

            Console.WriteLine(cellsDrawn);
        }

        private int getGridSubdivision(Rectangle cameraFrustumBounds, float baseScale)
        {
            var gridCellArea = gridMeshBuilder.TilingX.X * gridMeshBuilder.TilingY.Y * baseScale.Squared();

            var baseGridCellsInCameraFrustum = cameraFrustumBounds.Area / gridCellArea;

            // TODO: this should depend on resolution and on a setting
            var desiredNumberOfGridCellsOnScreen = 160;

            var idealSubCellsPerCell = desiredNumberOfGridCellsOnScreen / baseGridCellsInCameraFrustum;
            var idealSubdivisionsPerCell = idealSubCellsPerCell.Sqrted();

            var gridSubdivisionAsPowerOf2 = (int)Pow(2, (int) Log2(idealSubdivisionsPerCell));
            gridSubdivisionAsPowerOf2 = Max(1, gridSubdivisionAsPowerOf2);

            return gridSubdivisionAsPowerOf2;
        }

        private (int CellColumnsHalf, int CellRowsHalf) getMinimumGridDimensions()
        {
            var widthToCover = tileMapWidth * 0.5f * HexagonDistanceX;
            var heightToCover = tileMapWidth * 0.5f * HexagonDistanceY;

            var gridMeshWidth = gridMeshBuilder.TilingX.X;
            var gridMeshHeight = gridMeshBuilder.TilingY.Y;

            // TODO: base scale should be revisited once/if we add tesselation
            const float baseScale = 1f;
            var scale = baseScale / gridScaleSetting;

            var cellWidth = scale * gridMeshWidth;
            var cellHeight = scale * gridMeshHeight;

            var cellColumnsHalf = MoreMath.CeilToInt(widthToCover / cellWidth);
            var cellRowsHalf = MoreMath.CeilToInt(heightToCover / cellHeight);

            return (cellColumnsHalf, cellRowsHalf);
        }

        private Rectangle getCameraFrustumBounds()
        {
            var cameraPosition = -renderSettings.CameraPosition.Value;
            var farPlaneBaseCorner = renderSettings.FarPlaneBaseCorner.Value;
            var farPlaneUnitX = renderSettings.FarPlaneUnitX.Value * 2;
            var farPlaneUnitY = renderSettings.FarPlaneUnitY.Value * 2;

            var corner00 = farPlaneBaseCorner.Xy + cameraPosition.Xy;
            var corner10 = corner00 + farPlaneUnitX.Xy;
            var corner01 = corner00 + farPlaneUnitY.Xy;
            var corner11 = corner10 + farPlaneUnitY.Xy;

            var minX = Min(Min(corner00.X, corner10.X), Min(corner01.X, corner11.X));
            var minY = Min(Min(corner00.Y, corner10.Y), Min(corner01.Y, corner11.Y));
            var maxX = Max(Max(corner00.X, corner10.X), Max(corner01.X, corner11.X));
            var maxY = Max(Max(corner00.Y, corner10.Y), Max(corner01.Y, corner11.Y));

            var width = maxX - minX;
            var height = maxY - minY;

            return new Rectangle(minX, minY, width, height);
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
