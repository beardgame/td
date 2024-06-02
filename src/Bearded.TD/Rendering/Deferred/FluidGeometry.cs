using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics.ImageSharp;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Game.Simulation.World.Fluids;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static Bearded.TD.Tiles.Level;

namespace Bearded.TD.Rendering.Deferred;

sealed class FluidGeometry
{
    private static readonly Vector2 rightVector = Direction.Right.Vector();
    private static readonly Vector2 upRightVector = Direction.UpRight.Vector();
    private static readonly Vector2 upLeftVector = Direction.UpLeft.Vector();

    private readonly int radius;
    private readonly GeometryLayer levelGeometry;
    private readonly Fluid fluid;

    private readonly Tilemap<(float SurfaceLevel, bool HasFluid)> height;
    private readonly Tilemap<Vector2> flow;

    // TODO: use a non-indexed mesh builder instead?
    private readonly ExpandingIndexedTrianglesMeshBuilder<FluidVertex> meshBuilder
        = new();
    private readonly List<Texture> textures = new();

    private readonly IRenderer renderer;

    public FluidGeometry(GameInstance game, Fluid fluid, RenderContext context, Material material)
    {
        radius = game.State.Level.Radius;
        levelGeometry = game.State.GeometryLayer;
        this.fluid = fluid;

        height = new Tilemap<(float, bool)>(radius + 1);
        flow = new Tilemap<Vector2>(radius + 1);

        renderer = BatchedRenderer.From(meshBuilder.ToRenderable(),
            new[]{
                context.Settings.ViewMatrix,
                context.Settings.ProjectionMatrix,
                context.Settings.Time,
                context.Settings.FarPlaneBaseCorner,
                context.Settings.FarPlaneUnitX,
                context.Settings.FarPlaneUnitY,
                context.Settings.CameraPosition,
                context.DeferredRenderer.GetDepthBufferUniform("depthBuffer", TextureUnit.Texture1),
            }.Concat(material.Textures.Select(
                (t, i) =>
                {
                    var texture = Texture.From(ImageTextureData.From(t.Texture), c => c.GenerateMipmap());
                    textures.Add(texture);
                    return new TextureUniform(
                        t.UniformName,
                        TextureUnit.Texture0 + i,
                        texture
                    );
                })
            )
        );
        material.Shader.RendererShader.UseOnRenderer(renderer);
    }

    public void Render()
    {
        if (fluid.IsEmpty)
            return;

        resetFlow();
        prepareHeightAndFlow();
        createGeometry();

        renderer.Render();
        meshBuilder.Clear();
    }

    private void resetFlow()
    {
        foreach (var tile in Tilemap.EnumerateTilemapWith(radius))
        {
            flow[tile] = Vector2.Zero;
        }
    }

    private void prepareHeightAndFlow()
    {
        foreach (var tile in Tilemap.EnumerateTilemapWith(radius))
        {
            var (fluidLevel, fluidFlow) = fluid[tile];
            var groundHeight = levelGeometry[tile].DrawInfo.Height;

            if (fluidLevel.NumericValue <= 0.0001f)
            {
                height[tile] = (groundHeight.NumericValue, false);
                continue;
            }

            height[tile] = ((float) (groundHeight.NumericValue + fluidLevel.NumericValue), true);

            var flowRight = (float) fluidFlow.FlowRight.NumericValue * rightVector;
            var flowUpRight = (float) fluidFlow.FlowUpRight.NumericValue * upRightVector;
            var flowUpLeft = (float) fluidFlow.FlowUpLeft.NumericValue * upLeftVector;

            flow[tile] += flowRight + flowUpRight + flowUpLeft;
            flow[tile.Neighbor(Direction.Right)] += flowRight;
            flow[tile.Neighbor(Direction.UpRight)] += flowUpRight;
            flow[tile.Neighbor(Direction.UpLeft)] += flowUpLeft;
        }
    }

    private void createGeometry()
    {
        foreach (var tile in Tilemap.EnumerateTilemapWith(radius))
        {
            createGeometryForTile(tile);
        }
    }

    private void createGeometryForTile(Tile tile)
    {
        var rightTile = tile.Neighbor(Direction.Right);
        var upRightTile = tile.Neighbor(Direction.UpRight);
        var upLeftTile = tile.Neighbor(Direction.UpLeft);

        var current = height[tile];
        var right = height[rightTile];
        var upRight = height[upRightTile];
        var upLeft = height[upLeftTile];

        var flowCurrent = flow[tile];
        var flowRight = flow[rightTile];
        var flowUpRight = flow[upRightTile];
        var flowUpLeft = flow[upLeftTile];

        if (current.HasFluid || right.HasFluid || upRight.HasFluid)
        {
            addTriangle(
                tile, rightTile, upRightTile,
                current, right, upRight,
                flowCurrent, flowRight, flowUpRight
            );
        }

        if (current.HasFluid || upRight.HasFluid || upLeft.HasFluid)
        {
            addTriangle(
                tile, upRightTile, upLeftTile,
                current, upRight, upLeft,
                flowCurrent, flowUpRight, flowUpLeft
            );
        }
    }

    private void addTriangle(
        Tile t0, Tile t1, Tile t2,
        (float SurfaceLevel, bool HasFluid) h0,
        (float SurfaceLevel, bool HasFluid) h1,
        (float SurfaceLevel, bool HasFluid) h2,
        Vector2 f0, Vector2 f1, Vector2 f2)
    {
        var maxValidHeight = getMaxValidHeight(h0, h1, h2);

        addTriangle(
            GetPosition(t0).NumericValue.WithZ(h0.HasFluid ? h0.SurfaceLevel : maxValidHeight),
            GetPosition(t1).NumericValue.WithZ(h1.HasFluid ? h1.SurfaceLevel : maxValidHeight),
            GetPosition(t2).NumericValue.WithZ(h2.HasFluid ? h2.SurfaceLevel : maxValidHeight),
            f0, f1, f2
        );
    }

    private float getMaxValidHeight(
        (float SurfaceLevel, bool HasFluid) h0,
        (float SurfaceLevel, bool HasFluid) h1,
        (float SurfaceLevel, bool HasFluid) h2)
    {
        var max = h0.HasFluid ? h0.SurfaceLevel : float.NegativeInfinity;
        if (h1.HasFluid)
            max = Math.Max(max, h1.SurfaceLevel);
        if (h2.HasFluid)
            max = Math.Max(max, h2.SurfaceLevel);
        return max;
    }

    private void addTriangle(Vector3 p0, Vector3 p1, Vector3 p2, Vector2 f0, Vector2 f1, Vector2 f2)
    {
        meshBuilder.AddTriangle(
            new FluidVertex(p0, Vector3.Zero, f0),
            new FluidVertex(p1, Vector3.Zero, f1),
            new FluidVertex(p2, Vector3.Zero, f2)
        );
    }

    public void CleanUp()
    {
        meshBuilder.Dispose();
        renderer.Dispose();
        foreach (var t in textures)
            t.Dispose();
    }
}
