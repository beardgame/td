using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Meta;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static System.Math;
using static Bearded.Graphics.Vertices.VertexData;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Rendering.Deferred.Level;

[StructLayout(LayoutKind.Sequential)]
readonly struct HeightmapInstanceVertex(Vector2 offset) : IVertexData
{
    private readonly Vector2 offset = offset;

    public static ImmutableArray<VertexAttribute> VertexAttributes { get; }
        = MakeAttributeArray(
            MakeAttributeTemplate<Vector2>("instanceOffset", instanced: true)
        );
}

sealed class TessellatedHeightmapToLevelRenderer : IHeightmapToLevelRenderer
{
    private readonly Shader shader;

    private readonly Tiles.Level level;
    private readonly CoreRenderSettings renderSettings;

    private readonly FloatUniform heightScaleUniform = new("heightScale");
    private readonly FloatUniform heightOffsetUniform = new("heightOffset");
    private readonly Vector2Uniform gridScaleUniform = new("gridScale");

    private readonly RhombusGridMesh gridMeshBuilder;
    private readonly IRenderer gridRenderer;

    private float gridScaleSetting;

    public TessellatedHeightmapToLevelRenderer(GameInstance game, RenderContext context,
        Heightmap heightmap, BiomeBuffer biomeBuffer, BiomeMaterials biomeMaterials, Shader shader)
    {
        this.shader = shader;
        renderSettings = context.Settings;
        level = game.State.Level;

        (gridMeshBuilder, gridRenderer) = setupGridRenderer(context, heightmap, biomeBuffer, biomeMaterials);
    }

    public void Dispose()
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
        GL.PatchParameter(PatchParameterInt.PatchVertices, 3);

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
        var visibleArea = renderSettings.GetCameraFrustumBoundsAtFarPlane();

        var gridDimensions = HeightmapToLevelRendererHelpers.Grid.For(
            level, gridScaleSetting, gridMeshBuilder.TilingX, gridMeshBuilder.TilingY);

        renderLevelGrid(visibleArea, gridDimensions);
    }

    private void renderLevelGrid(Rectangle visibleArea, HeightmapToLevelRendererHelpers.Grid grid)
    {
        gridScaleUniform.Value = new Vector2(grid.Scale);

        HeightmapToLevelRendererHelpers.IterateLevelCells(visibleArea, grid, gridMeshBuilder.AddInstance);

        drawInstances();
        gridMeshBuilder.Clear();
    }

    private void drawInstances()
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

    private (RhombusGridMesh, IRenderer) setupGridRenderer(
        RenderContext context,
        Heightmap heightmap,
        BiomeBuffer biomeBuffer,
        BiomeMaterials biomeMaterials)
    {
        var mesh = RhombusGridMesh.CreateDefault();

        var renderer = Renderer.From(mesh.ToRenderable(),
            new IRenderSetting[]
            {
                context.Settings.ViewMatrix,
                context.Settings.ProjectionMatrix,
                context.Settings.FarPlaneDistance,
                heightmap.RadiusUniform,
                heightmap.PixelSizeUVUniform,
                heightmap.GetMapTextureUniform("heightmap", TextureUnit.Texture0),
                biomeBuffer.GetTextureUniform("biomeTilemap", TextureUnit.Texture0 + 1),
                biomeBuffer.GetRadiusUniform("biomeTilemapRadius"),
                context.Settings.CameraPosition,
                heightScaleUniform,
                heightOffsetUniform,
                gridScaleUniform,
            }.Concat(
                biomeMaterials.Samplers
                    .Select((s, i) => s.GetUniform(TextureUnit.Texture0 + i + 2))
            ));

        shader.RendererShader.UseOnRenderer(renderer);

        return (mesh, renderer);
    }


}

sealed class RhombusGridMesh : IDisposable
{
    private readonly Buffer<LevelVertex> vertices;
    private readonly Buffer<ushort> indices;
    private readonly BufferStream<HeightmapInstanceVertex> instances;

    public Vector2 TilingX { get; }
    public Vector2 TilingY { get; }

    public static RhombusGridMesh CreateDefault()
    {
        var (vertices, indices, tilingX, tilingY) = buildRhombusMesh();

        var instances = new BufferStream<HeightmapInstanceVertex>(new Buffer<HeightmapInstanceVertex>());

        return new RhombusGridMesh(vertices, indices, instances, tilingX, tilingY);
    }

    private RhombusGridMesh(
        Buffer<LevelVertex> vertices, Buffer<ushort> indices,
        BufferStream<HeightmapInstanceVertex> instances,
        Vector2 tilingX, Vector2 tilingY)
    {
        this.vertices = vertices;
        this.indices = indices;
        this.instances = instances;
        TilingX = tilingX;
        TilingY = tilingY;
    }

    public void Clear()
    {
        instances.Clear();
    }

    public void AddInstance(Vector2 offset)
    {
        instances.Add(new HeightmapInstanceVertex(offset));
    }

    public IRenderable ToRenderable()
    {
        return Renderable.Build(
            PrimitiveType.Patches,
            b => b
                .With(vertices.AsVertexBuffer())
                .With(instances.AsVertexBuffer())
                .With(indices.AsIndexBuffer())
                .InstancedWith(() => instances.Count)
        );
    }

    public void Dispose()
    {
        vertices.Dispose();
        indices.Dispose();
        instances.Buffer.Dispose();
    }

    private static (Buffer<LevelVertex> vertices, Buffer<ushort> indices, Vector2 tilingX, Vector2 tilingY)
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

        var vertices = new LevelVertex[vertexCount];
        var indices = new ushort[indexCount];

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

        var vertexBuffer = new Buffer<LevelVertex>();
        using (var t = vertexBuffer.Bind())
        {
            t.Upload(vertices);
        }
        var indexBuffer = new Buffer<ushort>();
        using (var t = indexBuffer.Bind())
        {
            t.Upload(indices);
        }

        return (vertexBuffer, indexBuffer, tilingX, tilingY);

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
